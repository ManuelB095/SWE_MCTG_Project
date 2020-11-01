using System;
namespace MCTG_Bernacki
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server on port 8080");
            HTTPServer server = new HTTPServer(8080);
            server.Start();
        }
    }
}
