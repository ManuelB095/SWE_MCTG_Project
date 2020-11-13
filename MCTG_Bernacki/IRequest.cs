using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    interface IRequest
    {
        String Type { get; }
        String URL { get; }
        Dictionary<String,String> Header { get; }

        String Payload { get; }
    }
}
