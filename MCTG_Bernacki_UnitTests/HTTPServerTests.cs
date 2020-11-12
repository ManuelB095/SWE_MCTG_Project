using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Moq;
using MCTG_Bernacki;
using System.IO;

namespace MCTG_Bernacki_UnitTests
{
    [TestFixture]
    class HTTPServerTests
    { 
        [Test]
        public void HandleClient_ReadAllMessages_ChecksInternalResponse()
        {
            // Arrange
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory) { AutoFlush = true};
            String requestAllMessagesRead =
            "GET /messages HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n";
           
            writer.Write(requestAllMessagesRead);
            memory.Position = 0;

            var mockClient = new Mock<IMyTcpClient>();
            mockClient.Setup(x => x.GetStream()).Returns(memory);

            // Act
            var server = new HTTPServer(8080);
            server.HandleClient(mockClient.Object);

            // Assert            
            Assert.AreEqual(server.Response.GetStatus(), "200 OK");
            Assert.AreEqual(server.Response.GetMimeType(), "application/json");
            Assert.AreEqual(server.Response.GetMessage(),
                "{\"1\":\"New Content goes here\"}" +
                "{\"2\":\"New Content goes here\"}");
        }

        [Test]
        public void HandleClient_ReadMessage2_ChecksInternalResponse()
        {
            // Arrange
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory) { AutoFlush = true };
            String requestAllMessagesRead =
            "GET /messages/2 HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n";            

            writer.Write(requestAllMessagesRead);
            memory.Position = 0;

            var mockClient = new Mock<IMyTcpClient>();
            mockClient.Setup(x => x.GetStream()).Returns(memory);

            // Act
            var server = new HTTPServer(8080);
            server.HandleClient(mockClient.Object);

            // Assert    
            Assert.AreEqual(server.Response.GetStatus(), "200 OK");
            Assert.AreEqual(server.Response.GetMimeType(), "application/json");
            Assert.AreEqual(server.Response.GetMessage(),
                "{\"2\":\"New Content goes here\"}");
        }
        [Test]
        public void HandleClient_PostNewMessage_ChecksInternalResponse()
        {
            // Arrange
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory) { AutoFlush = true };
            String requestAllMessagesRead =
            "POST /messages HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n" +
            "New Content goes here";        

            writer.Write(requestAllMessagesRead);
            memory.Position = 0;

            var mockClient = new Mock<IMyTcpClient>();
            mockClient.Setup(x => x.GetStream()).Returns(memory);

            // Act
            var server = new HTTPServer(8080);
            server.HandleClient(mockClient.Object);

            // Assert                
            Assert.AreEqual(server.Response.GetStatus(), "200 OK");
            Assert.AreEqual(server.Response.GetMimeType(), "text/plain");
            Assert.AreEqual(server.Response.GetMessage(), "Successfully posted message: \n\nNew Content goes here");
        }

        [Test]
        public void HandleClient_DeleteMessage1_ChecksInternalResponse()
        {
            // Arrange
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory) { AutoFlush = true };
            String requestAllMessagesRead =
            "DELETE /messages/1 HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n";

            writer.Write(requestAllMessagesRead);
            memory.Position = 0;

            var mockClient = new Mock<IMyTcpClient>();
            mockClient.Setup(x => x.GetStream()).Returns(memory);

            // Act
            var server = new HTTPServer(8080);
            server.HandleClient(mockClient.Object);

            // Assert            
            Assert.AreEqual(server.Response.GetStatus(), "200 OK");
            Assert.AreEqual(server.Response.GetMimeType(), "text/plain");
            Assert.AreEqual(server.Response.GetMessage(), "Successfully deleted 1.json\n");
        }
        [Test]
        public void HandleClient_UpdateMessage1_ChecksInternalResponse()
        {
            // Arrange
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory) { AutoFlush = true };
            String requestAllMessagesRead =
            "PUT /messages/1 HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: curl/7.55.1\\n" +
            "Accept: */*\r\n" +
            "Content-type: application/json\r\n\r\n" +
            "This is updated";
  
            writer.Write(requestAllMessagesRead);
            memory.Position = 0;

            var mockClient = new Mock<IMyTcpClient>();
            mockClient.Setup(x => x.GetStream()).Returns(memory);

            // Act
            var server = new HTTPServer(8080);
            server.HandleClient(mockClient.Object);

            // Assert
            Assert.AreEqual(server.Response.GetStatus(), "200 OK");
            Assert.AreEqual(server.Response.GetMimeType(), "text/plain");
            Assert.AreEqual(server.Response.GetMessage(),
                "Successfully overwritten message 1.json with text: \n\nThis is updated\n");
        }
    }
}
