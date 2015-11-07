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
        /// Actual offset in inputs array.
        /// </summary>
        private readonly ParameterExpression _inputsActualOffset = Expression.Variable(typeof(int));

        /// <summary>
        /// Parameter where request comes from.
        /// </summary>
        internal readonly ParameterExpression RequestParameter = Expression.Parameter(typeof(Request));

        /// <summary>
        /// Field where method is stored.
        /// </summary>
        internal readonly Expression MethodStorage;

        /// <summary>
        /// Storage with blobs array.
        /// </summary>
        internal readonly Expression BlobsStorage;

        /// <summary>
        /// Storage with blobs offsets.
        /// </summary>
        internal readonly Expression BlobsOffsetsStorage;

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
            StateStorage = Expression.Field(RequestParameter, "State");

            _inputs = Expression.Field(RequestParameter, "Inputs");
            _inputsStartOffset = Expression.Field(RequestParameter, "InputsStartOffset");
            _inputsEndOffset = Expression.Field(RequestParameter, "InputsEndOffset");
        }


        /// <summary>
        /// Creates expression reading string from input into indexed storage ending with given chars.
        /// </summary>
        /// <param name="storageIndex">Index of storage where string will be saved.</param>
        /// <param name="endChars">Characters ending the string.</param>
        /// <returns>Created expression.</returns>
        internal Expression ReadString(int storageIndex, char[] endChars)
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
        internal Expression GoToState(int state)
        {
            return MakeVoid(
                Expression.Block(
                    Expression.Assign(StateStorage, Expression.Constant(state)),
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
    }
}
