using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading;
using System.Globalization;

using ServeRick2.Http;
using ServeRick2.Http.Parsing;
using ServeRick2.Http.HeaderDefinitions;

namespace ServeRick2.Tests.Http
{
    [TestClass]
    public class HeaderParsing
    {
        [TestMethod]
        public void SimpleTest()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); 
            var automaton = RequestHeaderParser.CompileAutomaton(new List<Header>());

            var request = new Request();
            var input = "PUT abcdef HTTP1.0\n";
            var inputBytes = Encoding.ASCII.GetBytes(input);

            request.Inputs = inputBytes;
            request.InputsEndOffset = inputBytes.Length;

            automaton(request);
        }
    }
}
