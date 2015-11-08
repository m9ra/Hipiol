using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServeRick2.Http;
using ServeRick2.Http.Parsing;
using ServeRick2.Http.HeaderDefinitions;

namespace ServeRick2.Tests.Utilities
{
    internal static class TestUtilities
    {
        /// <summary>
        /// Creates <see cref="RequestTest"/> and fills it with given input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The crated test object.</returns>
        internal static RequestTest AssertCompleteRequest(this string input)
        {
            var request = new RequestTest();
            request.FillData(input);

            return request.AssertComplete();
        }
    }

    internal class RequestTest
    {
        /// <summary>
        /// Compiled automaton for header parsing.
        /// </summary>
        private readonly ParsingAutomaton _automaton;

        /// <summary>
        /// Request which will be used for automaton testing.
        /// </summary>
        private readonly Request _request = new Request();

        internal RequestTest()
        {
            _automaton = RequestHeaderParser.CompileAutomaton(new List<Header>() { 
                new ContentLengthHeader() 
            });
        }

        /// <summary>
        /// Fills automaton with given input.
        /// </summary>
        /// <param name="input">Input for automaton filling.</param>
        /// <returns>This object.</returns>
        internal RequestTest FillData(string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            _request.Inputs = inputBytes;
            _request.InputsEndOffset = inputBytes.Length;

            _automaton(_request);
            return this;
        }

        /// <summary>
        /// Asserts that request is complete.
        /// </summary>
        /// <returns>This object.</returns>
        internal RequestTest AssertComplete()
        {
            Assert.IsTrue(_request.IsComplete, "Request should be complete");
            return this;
        }

        /// <summary>
        /// Asserts content length of request.
        /// </summary>
        /// <param name="expectedContentLength">Expected content length.</param>
        /// <returns>This object.</returns>
        internal RequestTest AssertContentLength(int expectedContentLength)
        {
            Assert.AreEqual(expectedContentLength, _request.ContentLength);
            return this;
        }
    }

}
