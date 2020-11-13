using System;
namespace MCTG_Bernacki
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server on port 10001");
            HTTPServer server = new HTTPServer(10001);
            server.Start();     
        }
    }
}
