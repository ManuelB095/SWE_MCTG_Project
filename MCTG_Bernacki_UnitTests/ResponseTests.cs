using MCTG_Bernacki;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    class ResponseTests
    {
        /* ---------------------------------------------------------------
            Setup
        */

        String getAllMsgs = "GET /messages HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";

        String getAllMsgsBAD = "GET /messages/a HTTP/1.1\r\n" +
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

        String postMsgBAD = "POST /messages/1 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n" +
        "New Content goes here";

        String updateMsgTwo = "PUT /messages/2 HTTP/1.1\r\n" +
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

        String updateMsgOneNOTFOUND = "PUT /messages/99999 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n" +
        "Updated from RequestTests";

        String deleteMsgOne = "DELETE /messages/1 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";

        String deleteMsgOneNOTFOUND = "DELETE /messages/999 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";

        String notAllowed = "SHOW /messages/1 HTTP/1.1\r\n" +
        "Host: localhost:10001\r\n" +
        "User-Agent: curl/7.55.1\r\n" +
        "Accept: */*\r\n" +
        "Content-type: application/json\r\n\r\n";


        /* ---------------------------------------------------------------
            Test From() Method
        */

        [Test]
        // The response depends on the 
        public void From_CreateResponseFromGETAllMsgs_ReturnsResponse()
        {
            //Arrange
            var request = Request.GetRequest(getAllMsgs);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "application/json");
            Assert.AreEqual(response.GetMessage(),
            "{\"1\":\"New Content goes here\"}" +
            "{\"2\":\"New Content goes here\"}");
        }

        [Test] 
        public void From_CreateResponseFromGETMsg_ReturnsResponse()
        {
            //Arrange
            var request = Request.GetRequest(getSecMsg);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "application/json");
            Assert.AreEqual(response.GetMessage(),            
            "{\"2\":\"New Content goes here\"}");
        }

        [Test]
        public void From_CreateResponseFromPOSTMsg_ReturnsResponse()
        {
            //Arrange
            var request = Request.GetRequest(postMsg);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "text/plain");
            Assert.AreEqual(response.GetMessage(),
            "Successfully posted message: \n\nNew Content goes here");

            // Reset Environment
            request = Request.GetRequest(deleteMsgOne);
            response = Response.From(request);
        }

        [Test]
        public void From_CreateResponseFromPUTMsg_ReturnsResponse()
        {
            //Arrange
            var request = Request.GetRequest(updateMsgTwo);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "text/plain");
            Assert.AreEqual(response.GetMessage(),
            "Successfully overwritten message 2.json with text: \n\nUpdated from RequestTests\n");

            // Reset Environment
            request = Request.GetRequest(resetMsgTwo);
            response = Response.From(request);
        }

        [Test]
        public void From_CreateResponseFromDELETEMsg_ReturnsResponse()
        {
            //Arrange
            var request = Request.GetRequest(deleteMsgOne);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "200 OK");
            Assert.AreEqual(response.GetMimeType(), "text/plain");
            Assert.AreEqual(response.GetMessage(),
            "Successfully deleted 1.json\n");

            // Reset Environment
            request = Request.GetRequest(postMsg);
            response = Response.From(request);
        }

        /* ---------------------------------------------------------------
            Test BadRequest with From()
        */

        [Test]
        public void From_BadResponseFromGETMsg_ReturnsBadResponse()
        {
            //Arrange
            var request = Request.GetRequest(getAllMsgsBAD);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "400 Bad Request");
            Assert.AreEqual(response.GetMimeType(), "text/html");
            Assert.AreEqual(response.GetMessage(),
            "<!doctype html>\r\n" +
            "<html lang=\"en\">\r\n" +
            "<head>\r\n" +
            "  <meta charset=\"utf-8\">\r\n" +
            "  <title>400 Bad Request</title>\r\n" +
            "</head>\r\n" +
            "\r\n" +
            "<body>\r\n" +
            "  <h1>Bad Request</h1>\r\n" +
            "  <p>The request was malformed.</p>\r\n" +
            "  <hr>\r\n" +
            "  <address>My HTTP Server - Localhosted </address>\r\n" +
            "</body>\r\n" +
            "</html>");


        }

        [Test]
        public void From_BadResponseFromPOSTMsg_ReturnsBadResponse()
        {
            //Arrange
            var request = Request.GetRequest(postMsgBAD);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "400 Bad Request");
            Assert.AreEqual(response.GetMimeType(), "text/html");
            Assert.AreEqual(response.GetMessage(),
            "<!doctype html>\r\n" +
            "<html lang=\"en\">\r\n" +
            "<head>\r\n" +
            "  <meta charset=\"utf-8\">\r\n" +
            "  <title>400 Bad Request</title>\r\n" +
            "</head>\r\n" +
            "\r\n" +
            "<body>\r\n" +
            "  <h1>Bad Request</h1>\r\n" +
            "  <p>The request was malformed.</p>\r\n" +
            "  <hr>\r\n" +
            "  <address>My HTTP Server - Localhosted </address>\r\n" +
            "</body>\r\n" +
            "</html>");
        }

        /* ---------------------------------------------------------------
            Test NotFound with From()
        */

        [Test]
        public void From_NotFoundFromPUTMsg_ReturnsNotFound()
        {
            //Arrange
            var request = Request.GetRequest(updateMsgOneNOTFOUND);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "404 File not found");
            Assert.AreEqual(response.GetMimeType(), "text/html");
            Assert.AreEqual(response.GetMessage(),
            "<!doctype html>\r\n" +
            "<html lang=\"en\">\r\n" +
            "<head>\r\n" +
            "  <meta charset=\"utf-8\">\r\n" +
            "  <title>404 Not Found</title>\r\n" +
            "</head>\r\n" +
            "\r\n" +
            "<body>\r\n" +
            "  <h1>Not Found</h1>\r\n" +
            "  <p>The requested URL was not found on this server.</p>\r\n" +
            "  <hr>\r\n" +
            "  <address>My HTTP Server - Localhosted </address>\r\n" +
            "</body>\r\n" +
            "</html>");
        }

        [Test]
        public void From_NotFoundFromDELETEMsg_ReturnsNotFound()
        {
            //Arrange
            var request = Request.GetRequest(deleteMsgOneNOTFOUND);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "404 File not found");
            Assert.AreEqual(response.GetMimeType(), "text/html");
            Assert.AreEqual(response.GetMessage(),
           "<!doctype html>\r\n" +
           "<html lang=\"en\">\r\n" +
           "<head>\r\n" +
           "  <meta charset=\"utf-8\">\r\n" +
           "  <title>404 Not Found</title>\r\n" +
           "</head>\r\n" +
           "\r\n" +
           "<body>\r\n" +
           "  <h1>Not Found</h1>\r\n" +
           "  <p>The requested URL was not found on this server.</p>\r\n" +
           "  <hr>\r\n" +
           "  <address>My HTTP Server - Localhosted </address>\r\n" +
           "</body>\r\n" +
           "</html>");
        }

        /* ---------------------------------------------------------------
            Test MethodNotAllowed with From()
        */

        [Test]
        public void From_MethodNotAllowed_ReturnsMethodNotAllowed()
        {
            //Arrange
            var request = Request.GetRequest(notAllowed);

            // Act
            var response = Response.From(request);
            // Assert
            Assert.AreEqual(response.GetStatus(), "405 Method not allowed");
            Assert.AreEqual(response.GetMimeType(), "text/html");
            Assert.AreEqual(response.GetMessage(),
            "<!doctype html>\r\n" +
            "<html lang=\"en\">\r\n" +
            "<head>\r\n" +
            "  <meta charset=\"utf-8\">\r\n" +
            "  <title>405 Method not Allowed</title>\r\n" +
            "</head>\r\n" +
            "\r\n" +
            "<body>\r\n" +
            "  <h1>Method not allowed</h1>\r\n" +
            "  <p>The method is not allowed.</p>\r\n" +
            "  <hr>\r\n" +
            "  <address>My HTTP Server - Localhosted </address>\r\n" +
            "</body>\r\n" +
            "</html>");
        }

        /* ---------------------------------------------------------------
            Test Post() Method
        */

        [Test]
        public void Post_TestsIfCorrectDataIsPosted_ComparesStreamContents()
        {
            // Arrange
            var memory = new MemoryStream();          
            var request = Request.GetRequest(postMsg);
            var response = Response.From(request);

            // Act
            response.Post(memory);
            // Assert
            memory.Position = 0;
            var reader = new StreamReader(memory);
            String postResult = reader.ReadToEnd();
            Assert.AreEqual(postResult, 
                "HTTP/1.1 200 OK\nContentType:" +
                " text/plain\nContentLength: " +
                "52\n\nSuccessfully posted message: " +
                "\n\nNew Content goes here");
            
        }
    }
}
