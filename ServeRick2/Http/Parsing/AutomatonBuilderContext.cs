using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick2.Http.Parsing
{
    class AutomatonBuilderContext
    {
        /// <summary>
        /// Index of lastly registered blob reader.
        /// </summary>
        private int _lastRegisteredBlobReader = 0;

        /// <summary>
        /// Initial state for routines of shared blob line reader if requested, <c>null</c> otherwise.
        /// </summary>
        private AutomatonState _initialSharedBlobLineReader;

        /// <summary>
        /// States that has been registered.
        /// </summary>
        private readonly List<AutomatonState> _registeredStates = new List<AutomatonState>();

        /// <summary>
        /// Array with input bytes to process.
        /// </summary>
        private readonly Expression _inputs;

        /// <summary>
        /// Start offset of inputs array.
        /// </summary>
        private readonly Expression _inputsStartOffset;

        /// <summary>
        /// End offset of inputs array.
        /// </summary>
        private readonly Expression _inputsEndOffset;

        /// <summary>
        /// Constant representing 10. Is used for multiplication.
        /// </summary>
        private readonly Expression _10 = Expression.Constant(10);

        /// <summary>
        /// Actual offset in inputs array.
        /// </summary>
        private readonly ParameterExpression _inputsActualOffset = Expression.Variable(typeof(int));

        /// <summary>
        /// Parameter where request comes from.
        /// </summary>
        internal readonly ParameterExpression RequestParameter = Expression.Parameter(typeof(Request));

        /// <summary>
        /// Automaton states registered within current context.
        /// <remarks>WARNING: Collection may be growing during enumeration - usually when <see cref="Compile"/> is called happens.</remarks>
        /// </summary>
        internal IEnumerable<AutomatonState> RegisteredStates
        {
            get
            {
                for (var i = 0; i < _registeredStates.Count; ++i)
                    yield return _registeredStates[i];
            }
        }

        /// <summary>
        /// Field where method is stored.
        /// </summary>
        internal readonly Expression MethodStorage;

        /// <summary>
        /// Storage with flag determining header parsing completition.
        /// </summary>
        internal readonly Expression IsCompleteStorage;

        /// <summary>
        /// Storage with blobs array.
        /// </summary>
        internal readonly Expression BlobsStorage;

        /// <summary>
        /// Storage with blobs offsets.
        /// </summary>
        internal readonly Expression BlobsOffsetsStorage;

        /// <summary>
        /// Index of blob which data are going to be read.
        /// </summary>
        internal readonly Expression BlobPointer;

        /// <summary>
        /// Mapping of blobs.
        /// </summary>
        internal readonly Expression BlobsMapping;

        /// <summary>
        /// Storage where actual state is stored.
        /// </summary>
        internal readonly Expression StateStorage;

        /// <summary>
        /// Variable where actual input byte is stored.
        /// </summary>
        internal readonly ParameterExpression InputVariable = Expression.Variable(typeof(byte));

        /// <summary>
        /// Label on the closest routine for moving to next byte.
        /// 
        /// TODO:optimize for local movings.
        /// </summary>
        internal readonly LabelTarget MoveNextByteLabel = Expression.Label("MoveNext");

        /// <summary>
        /// Label for ending reading of input.
        /// </summary>
        internal readonly LabelTarget EndInputReading = Expression.Label("EndReading");


        internal AutomatonBuilderContext()
        {
            MethodStorage = Expression.Field(RequestParameter, "Method");
            BlobsStorage = Expression.Field(RequestParameter, "Blobs");
            BlobsOffsetsStorage = Expression.Field(RequestParameter, "BlobsOffsets");
            BlobsMapping = Expression.Field(RequestParameter, "BlobsMapping");
            BlobPointer = Expression.Field(RequestParameter, "BlobPointer");
            StateStorage = Expression.Field(RequestParameter, "State");
            IsCompleteStorage = Expression.Field(RequestParameter, "IsComplete");

            _inputs = Expression.Field(RequestParameter, "Inputs");
            _inputsStartOffset = Expression.Field(RequestParameter, "InputsStartOffset");
            _inputsEndOffset = Expression.Field(RequestParameter, "InputsEndOffset");
        }


        /// <summary>
        /// Creates expression reading blob from input into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of stored blob  will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        /// <returns>Created expression.</returns>
        internal Expression ExclusiveReadBlob(int storageIndex, char[] endChars)
        {
            Expression charCondition = null;
            foreach (var endChar in endChars)
            {
                var charComparison = InputNotequalityComparision(endChar);

                if (charCondition == null)
                    charCondition = charComparison;
                else
                    charCondition = Expression.AndAlso(charCondition, charComparison);
            }

            var storeByteBlock = Expression.Block(
                SetStorageByte(storageIndex, InputVariable),
                MoveToNextByte()
                );
            return Expression.IfThen(charCondition, storeByteBlock);
        }

        /// <summary>
        /// Creates new state for constructed automaton.
        /// </summary>
        /// <returns>Created state.</returns>
        internal AutomatonState CreateNewState()
        {
            var state = new AutomatonState(_registeredStates.Count);
            _registeredStates.Add(state);

            return state;
        }

        /// <summary>
        /// Registers index for next blob reader.
        /// </summary>
        /// <returns>The registered index.</returns>
        internal int RegisterBlobReader()
        {
            return ++_lastRegisteredBlobReader;
        }

        /// <summary>
        /// Creates expression which reads line input into the indexed blob.
        /// </summary>
        /// <param name="blobIndex">Index of target blob.</param>
        /// <returns>The created expression.</returns>
        internal Expression SharedBlobLineReader(int blobIndex)
        {
            //set index of blob to read
            var blobMap = Expression.ArrayAccess(BlobsMapping, Expression.PostIncrementAssign(BlobPointer));
            var blobMapAssign = Expression.Assign(blobMap, Expression.Constant(blobIndex));

            //goto to blob reader state
            var gotoBlob = GoToState(getSharedBlobLineReader());
            return Expression.Block(
                blobMapAssign,
                gotoBlob
                );
        }

        /// <summary>
        /// Emits input actions to read int into given storage.
        /// </summary>
        /// <param name="intStorageName">Storage where int will be stored.</param>
        internal Expression ReadInt(string intStorageName)
        {
            var switchCases = new List<SwitchCase>();
            for (var digit = 0; digit < 10; ++digit)
            {
                //parse digits
                var storedInt = Expression.Field(RequestParameter, intStorageName);
                var multipliedInt = Expression.Multiply(storedInt, _10);
                var numberStoring = Expression.Assign(storedInt, Expression.Add(multipliedInt, Expression.Constant(digit)));

                var switchCase = Expression.SwitchCase(Expression.Block(
                    numberStoring,
                    MoveToNextByte()
                    ), Expression.Constant((byte)digit.ToString()[0]));
                switchCases.Add(switchCase);
            }

            return Expression.Switch(InputVariable, switchCases.ToArray());
        }


        /// <summary>
        /// Creates expression passing all data until end of line.
        /// </summary>
        /// <returns>The created expression.</returns>
        internal Expression PassLine()
        {
            var nonNewLineCondition = InputNotequalityComparision('\n');
            return Expression.IfThen(nonNewLineCondition, MoveToNextByte());
        }

        /// <summary>
        /// Creates expression which jumps on routine moving to next input byte.
        /// </summary>
        /// <returns>The created expression.</returns>
        internal Expression MoveToNextByte()
        {
            return Expression.Continue(MoveNextByteLabel);
        }

        /// <summary>
        /// Gets expression which stores output as next byte to indexed storage.
        /// </summary>
        /// <param name="storageIndex">Index of the storage.</param>
        /// <param name="byteExpression">Expression with stored byte.</param>
        /// <returns>The created expression.</returns>
        internal Expression SetStorageByte(int storageIndex, Expression byteExpression)
        {
            var actualStorageOffset = Expression.ArrayAccess(BlobsOffsetsStorage, Expression.Constant(storageIndex));
            var storageElement = Expression.ArrayAccess(BlobsStorage, Expression.PostIncrementAssign(actualStorageOffset));
            var elementAssign = Expression.Assign(storageElement, byteExpression);

            return elementAssign;
        }

        /// <summary>
        /// Creates expression which compares input against compared char.
        /// </summary>
        /// <param name="comparedChar">Char which is compared to input.</param>
        /// <returns>The created expression.</returns>
        internal Expression InputEqualityComparision(char comparedChar)
        {
            return Expression.Equal(InputVariable, Expression.Constant((byte)comparedChar));
        }

        /// <summary>
        /// Creates expression which compares input against compared char.
        /// </summary>
        /// <param name="comparedChar">Char which is compared to input.</param>
        /// <returns>The created expression.</returns>
        internal Expression InputNotequalityComparision(char comparedChar)
        {
            return Expression.NotEqual(InputVariable, Expression.Constant((byte)comparedChar));
        }

        /// <summary>
        /// Creates expression which set next state for the automaton.
        /// </summary>
        /// <param name="state">State to set.</param>
        /// <returns>The created expression.</returns>
        internal Expression GoToState(AutomatonState state)
        {
            return MakeVoid(
                Expression.Block(
                    Expression.Assign(StateStorage, Expression.Constant(state.Id)),
                    Expression.Continue(MoveNextByteLabel)
                ));
        }

        /// <summary>
        /// Creates expression with same effect but the return value which is void.
        /// </summary>
        /// <param name="expressionToVoid">Expression which is been made void.</param>
        /// <returns>The created expression.</returns>
        internal Expression MakeVoid(Expression expressionToVoid)
        {
            return Expression.Block(typeof(void), expressionToVoid);
        }

        /// <summary>
        /// Creates expression that is iterating over input with given processor.
        /// </summary>
        /// <param name="inputProcessor">Processor to iterate the input.</param>
        /// <returns>The created expression.</returns>
        internal Expression IterateInput(SwitchExpression inputProcessor)
        {
            var hasNextInput = Expression.LessThan(_inputsActualOffset, _inputsEndOffset);
            var offsetStep = Expression.Assign(InputVariable, ReadInputsAtOffset(Expression.PostIncrementAssign(_inputsActualOffset)));

            var inputReadLoop =
                Expression.Loop(
                    Expression.IfThenElse(
                        hasNextInput,
                        Expression.Block(
                            offsetStep,
                            inputProcessor
                            ),
                        Expression.Break(EndInputReading)
                        ),
                    EndInputReading,
                    MoveNextByteLabel
                    );

            return Expression.Block(new ParameterExpression[] { _inputsActualOffset, InputVariable },
                ///initialize actual offset
                Expression.Assign(_inputsActualOffset, _inputsStartOffset),

                //process input read
                inputReadLoop
                );
        }

        /// <summary>
        /// Creates expression which reads input at given offset.
        /// </summary>
        /// <param name="offset">Offset where input will be read.</param>
        /// <returns>The created expression.</returns>
        internal Expression ReadInputsAtOffset(Expression offset)
        {
            return Expression.ArrayAccess(_inputs, offset);
        }

        #region Private utilities

        /// <summary>
        /// Gets starting state for shared line blob reader.
        /// </summary>
        /// <returns>The state.</returns>
        private AutomatonState getSharedBlobLineReader()
        {
            if (_initialSharedBlobLineReader == null)
                _initialSharedBlobLineReader = CreateNewState();

            return _initialSharedBlobLineReader;
        }

        #endregion
    }
}
