using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public interface IHTTPServer
    {
        public Response Response { get; }
        public Request Request { get; }
        void HandleClient(TcpClientWrapper client);
    }
}
