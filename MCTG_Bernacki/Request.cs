using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCTG_Bernacki
{
    public class Request : IRequest
    {
        public String Type { get; private set; }
        public String URL { get; private set; }

        public Dictionary<String, String> Header { get; private set; }

        public String Payload { get; set; }

        private Request(String type, String url, Dictionary<String, String> header, String payload)
        {
            this.Type = type; // GET or POST
            this.URL = url;
            this.Payload = payload;
            this.Header = header;
        }

        public static Request GetRequest(String request)
        {
            if (String.IsNullOrEmpty(request))
            {
                return null;
            }

            int foundHeaderStart = request.IndexOf("\r\n");
            int foundHeaderEnd = request.IndexOf("\r\n\r\n");            

            String headLine = request.Substring(0, foundHeaderStart);
            String headerData = request.Substring(foundHeaderStart + 2, foundHeaderEnd - foundHeaderStart);
            String[] headLineTokens = headLine.Split(' ');

            // Get Type, Version and requestedResource (URL)
            if (headLineTokens.Length < 3)
                throw new ArgumentException("Bad Client Request");

            String type = headLineTokens[0];
            String url = headLineTokens[1];
            String version = headLineTokens[2];

            // Get the Headers ( only specific headers allowed/read in )
            String[] allowedHeaders = {"Host", "User-Agent", "Accept", "Content-type", "Content-length"};
            Dictionary<String, String> header = Request.GetAllHeaders(headerData, allowedHeaders);            

            // Get The Payload ( if any )
            int found = request.IndexOf("\r\n\r\n");
            String payload = request.Substring(found + 4);
            return new Request(type, url, header, payload);
        }

        public static String ReturnHeaderData(String header, String searchstring)
        {
            int foundStart = -1;
            if ((foundStart = searchstring.IndexOf(header)) != -1)
            {
                int foundEnd = searchstring.IndexOf("\r\n", foundStart);
                String data = searchstring.Substring
                    (foundStart + header.Length + 2, foundEnd - foundStart - header.Length - 2);
                return data;
            }
            return null;
        }

        public static Dictionary<string,string> GetAllHeaders(String request, String[] allowedHeaders)
        {
            Dictionary<String, String> results = new Dictionary<string, string>();
            foreach (String header in allowedHeaders)
            {
                String data = Request.ReturnHeaderData(header, request);
                if (data != null)
                    results.Add(header, data);
            }
            return results;
        }

    }

}
