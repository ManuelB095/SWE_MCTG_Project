using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCTG_Bernacki
{
    interface IResponse
    {
        String GetStatus();
        String GetMimeType();
        String GetMessage();

        void Post(Stream stream);
    }
}
