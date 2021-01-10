using System;
using System.Collections.Generic;
using System.IO; // For Stream
using System.Net.Sockets;
using System.Text;

namespace MCTG_Bernacki
{
    public interface IMyTcpClient
    {
        Stream GetStream();
        void Close();
        

    }
}
