using System;
using System.Collections.Generic;
using System.Text;

namespace MCTG_Bernacki
{
    public class Request
    {
        public String Type { get; set; }
        public String URL { get; set; }
        public String Host { get; set; }

        public String Payload { get; set; }

        private Request(String type, String url, String host, String payload)
        {
            this.Type = type; // GET or POST
            this.URL = url;
            this.Host = host;
            this.Payload = payload;
        }

        public static Request GetRequest(String request)
        {
            if(String.IsNullOrEmpty(request))
            {
                return null;
            }

            String[] tokens = request.Split(' ');
            String type = tokens[0];
            String url = tokens[1];
            String host = tokens[3];
            int found = request.IndexOf("\r\n\r\n");
            String payload = request.Substring(found + 4);
            return new Request(type, url, host, payload);            
        }
    }

}
