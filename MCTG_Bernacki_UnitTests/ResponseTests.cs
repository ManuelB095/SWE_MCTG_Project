using MCTG_Bernacki;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    class ResponseTests
    {
        [Test]
        // The response depends on the 
        public void From_CreateResponseFromRequest_ReturnsResponse()
        {
            String msg = "GET /messages HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n";
            var request = Request.GetRequest(msg);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "application/json");
            Assert.AreEqual(response.GetMessage(),
            "{\"1\":\"New Content goes here\"}" +
            "{\"2\":\"New Content goes here\"}");
        }
    }
}
