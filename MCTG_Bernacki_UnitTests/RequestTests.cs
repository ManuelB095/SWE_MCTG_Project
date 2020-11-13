using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MCTG_Bernacki;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    class RequestTests
    {
        /* ---------------------------------------------------------------
            Setup
        */

        String getAllMsgs = "GET /messages HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";        

        String getSecMsg = "GET /messages/2 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";

        String postMsg = "POST /messages HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n" +
        "New Content goes here";

        String updateMsgOne = "PUT /messages/1 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n" +
        "Updated from RequestTests";

        String resetMsgTwo = "PUT /messages/2 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n" +
        "New Content goes here";

        String deleteMsgOne = "DELETE /messages/1 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";

        /* ---------------------------------------------------------------
            Test GetRequest() Method
        */

        [Test]
        public void GetRequest_ConvertrsStrToGET_ReturnsRequest()
        {          
            // Arrange   
            // Act
            Request req = Request.GetRequest(getAllMsgs);

            // Assert
            Assert.AreEqual(req.Type, "GET");
            Assert.AreEqual(req.URL, "/messages");
            Assert.AreEqual(req.Header["Host"], "localhost:10001");
            Assert.AreEqual(req.Header["User-Agent"], "curl/7.55.1");
            Assert.AreEqual(req.Header["Accept"], "*/*");
            Assert.AreEqual(req.Header["Content-type"], "application/json");
        }

        [Test]
        public void GetRequest_ConvertrsStrToPOST_ReturnsRequest()
        {
            // Arrange            
            // Act
            Request req = Request.GetRequest(postMsg);

            // Assert
            Assert.AreEqual(req.Type, "POST");
            Assert.AreEqual(req.URL, "/messages");
            Assert.AreEqual(req.Header["Host"], "localhost:10001");
            Assert.AreEqual(req.Header["User-Agent"], "curl/7.55.1");
            Assert.AreEqual(req.Header["Accept"], "*/*");
            Assert.AreEqual(req.Header["Content-type"], "application/json");
            Assert.AreEqual(req.Payload, "New Content goes here");
        }

        [Test]
        public void GetRequest_ConvertrsStrToPUT_ReturnsRequest()
        {
            // Arrange            
            // Act
            Request req = Request.GetRequest(updateMsgOne);

            // Assert
            Assert.AreEqual(req.Type, "PUT");
            Assert.AreEqual(req.URL, "/messages/1");
            Assert.AreEqual(req.Header["Host"], "localhost:10001");
            Assert.AreEqual(req.Header["User-Agent"], "curl/7.55.1");
            Assert.AreEqual(req.Header["Accept"], "*/*");
            Assert.AreEqual(req.Header["Content-type"], "application/json");
            Assert.AreEqual(req.Payload, "Updated from RequestTests");
        }

        [Test]
        public void GetRequest_ConvertrsStrToDELETE_ReturnsRequest()
        {
            // Arrange   
            // Act
            Request req = Request.GetRequest(deleteMsgOne);

            // Assert
            Assert.AreEqual(req.Type, "DELETE");
            Assert.AreEqual(req.URL, "/messages/1");
            Assert.AreEqual(req.Header["Host"], "localhost:10001");
            Assert.AreEqual(req.Header["User-Agent"], "curl/7.55.1");
            Assert.AreEqual(req.Header["Accept"], "*/*");
            Assert.AreEqual(req.Header["Content-type"], "application/json");
        }
    }
}
