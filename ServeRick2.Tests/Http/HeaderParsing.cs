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

using ServeRick2.Tests.Utilities;

namespace ServeRick2.Tests.Http
{
    [TestClass]
    public class HeaderParsing
    {
        [TestMethod]
        public void HeaderParsing_ContentLengthTest()
        {
            var input =
@"PUT abcdef HTTP/1.1
Content-Length: 18457
".AssertCompleteRequest()
 .AssertContentLength(18457);
        }

        [TestMethod]
        public void HeaderParsing_CookieTest()
        {
            var input =
@"PUT url HTTP/1.1
Cookie: abce=ealejf
".AssertCompleteRequest();
        }
    }
}
