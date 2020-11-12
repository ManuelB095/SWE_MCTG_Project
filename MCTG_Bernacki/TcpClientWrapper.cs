using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCTG_Bernacki
{
    public class TcpClientWrapper : IMyTcpClient
    {
        private TcpClient wrappedClient;
       
        public TcpClientWrapper()
        {
            this.wrappedClient = new TcpClient();
        }

        public TcpClientWrapper(TcpClient client)
        {
            this.wrappedClient = client;
        }

        public Stream GetStream()
        {
            return wrappedClient.GetStream();
        }

        public void Close()
        {
            this.wrappedClient.Close();
        }
    }
}
