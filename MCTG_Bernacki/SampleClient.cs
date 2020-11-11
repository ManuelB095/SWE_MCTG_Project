using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MCTG_Bernacki
{
    public class SampleClient
    {
        public static void Start()
        {
            Thread.Sleep(2000); // 2 sec so the server is up and running

            using TcpClient client = new TcpClient("localhost", 8080);
            using StreamReader reader = new StreamReader(client.GetStream());
            Console.WriteLine($"{reader.ReadLine()}{Environment.NewLine}{reader.ReadLine()}");
            using StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

            string input = null;
            while ((input = Console.ReadLine()) != "quit")
            {
                writer.WriteLine(input);
            }
            writer.WriteLine("quit");

        }
    }
}