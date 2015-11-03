using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var automaton = RequestHeaderParser.CompileAutomaton(new List<Header>());
        }
    }
}
