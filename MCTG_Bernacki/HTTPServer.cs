using System;
using System.Collections.Generic;
using System.Net; // For IPAddress Class
using System.Net.Sockets; // For TcpListener Class
using System.Threading; // For Thread Class
using System.Text;
using System.IO;
using System.Diagnostics; // For Debug Class

namespace MCTG_Bernacki
{
    public class HTTPServer
    {
        //private int port;        
        private bool running = false;
        public const String MSG_DIR = "/root/messages";
        public const String STAT_DIR = "/root/statcodes";
        public const String TOK_DIR = "/root/tokens";
        public const String VERSION = "HTTP/1.1";
        public const String NAME = "Manuel`s HTTP Server";
        public const String CONN_STRING = "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";


        public Response Response { get; private set; }
        public Request Request { get; private set; }


        private TcpListener listener;

        public HTTPServer(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            Thread serverThread = new Thread(new ThreadStart(Run));
            serverThread.Start();
        }

        private void Run()
        {
            running = true;
            listener.Start();

            while(running)
            {
                Console.WriteLine("waiting for connection...");

                TcpClient temp = listener.AcceptTcpClient();
                IMyTcpClient client = new TcpClientWrapper(temp);

                Console.WriteLine("Client Connected");

                HandleClient(client);

                client.Close();
            }
            running = false;
            listener.Stop();            
        }

        public void HandleClient(IMyTcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());
            String msg = "";
            while(reader.Peek() != -1)
            {
                msg += (char)reader.Read();
            }
            Console.WriteLine("Request: \n" + msg);
            
            this.Request = Request.GetRequest(msg);
            this.Response = Response.From(this.Request);
            this.Response.Post(client.GetStream());
        }

    }

   
}
