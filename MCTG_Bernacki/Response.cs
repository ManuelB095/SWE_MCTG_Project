﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCTG_Bernacki
{
    public class Response
    {
        private String status;
        private String mimeType;
        private String message;

        private Response(String status, String mimeType, String message)
        {            
            this.status = status;
            this.mimeType = mimeType;
            this.message = message;
        }

        public static Response From(Request request)
        {
            Debug.WriteLine(request.Type + " " + request.URL);

            String msgWithNumber = @"/messages/[0-9]+";
            Match m = Regex.Match(request.URL, msgWithNumber);
            
            if (request.Type == "GET")
            {
                if(request.URL == "/messages")
                {
                    List<String> msgsJson = Response.ReadAllMsgs();
                    String response = "";
                    foreach(String s in msgsJson)
                    {
                        response += s;
                    }
                    
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Host);
                    return new Response("200 OK", "application/json", response);
                }
                else if(m.Success)
                {
                    int found = request.URL.IndexOf("/", 1);
                    String msg_number = request.URL.Substring(found + 1);
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Host);

                    String json = Response.ReadMsgFromNumber(msg_number);
                    
                    if(json != "")
                    {
                        return new Response("200 OK", "application/json", json);
                    }      
                    Response notFound = Response.MakeFileNotFound();
                    return notFound;
                }
            }
            else if(request.Type == "POST")
            {
                if (request.Payload != "" && request.URL == "/messages")
                {
                    Response.CreateNewMsgEntry(request.Payload);
                    return new Response("200 OK", "text/plain", "Successfully posted message: \n\n" + request.Payload);
                }
                else
                {
                    Response badReq = Response.MakeBadRequest();
                    return badReq;
                }
            }            
            Response notAllowed = Response.MakeMethodNotAllowed();
            return notAllowed;
        }

        public void Post(System.Net.Sockets.NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true};
            String responseText = HTTPServer.VERSION + " " + this.status + "\n";
            responseText += "ContentType: " + this.mimeType + "\n";
            responseText += "ContentLength: " + Encoding.UTF8.GetBytes(this.message).Length + "\n";
            responseText += "\n" + this.message;
            Debug.WriteLine(responseText);
            writer.Write(responseText);
            
        }

        private static Response MakeBadRequest()
        {
            String path = Environment.CurrentDirectory + HTTPServer.STAT_DIR + "\\" + "400.html";
            FileInfo statCode = new FileInfo(path);
            if (statCode.Exists)
            {
                String fileContents = File.ReadAllText(@statCode.FullName);
                return new Response("400 Bad Request", "text/html", fileContents);
            }
            return new Response("400 Bad Request", "text/html", "400 Bad Request");
        }

        private static Response MakeFileNotFound()
        {
            String path = Environment.CurrentDirectory + HTTPServer.STAT_DIR + "\\" + "404.html";
            FileInfo statCode = new FileInfo(path);
            if (statCode.Exists)
            {
                String fileContents = File.ReadAllText(@statCode.FullName);
                return new Response("404 File not found", "text/html", fileContents);
            }
            return new Response("404 File not found", "text/html", "404 File not found");
        }

        private static Response MakeMethodNotAllowed()
        {
            String path = Environment.CurrentDirectory + HTTPServer.STAT_DIR + "\\" + "405.html";
            FileInfo statCode = new FileInfo(path);
            if (statCode.Exists)
            {
                String fileContents = File.ReadAllText(@statCode.FullName);
                return new Response("405 Method not allowed", "text/html", fileContents);
            }
            return new Response("405 Method not allowed", "text/html", "405 Method not allowed");
        }

        private static void CreateNewMsgEntry(String payload)
        {
            String dirPath = Environment.CurrentDirectory + HTTPServer.MSG_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();
            String msgID = (files.Length + 1).ToString();

            String newFilename = msgID + ".json";
            String newContent = "{\"" + msgID + "\":\"" + payload + "\"}";
            // For Serialization maybe
            //List<String> newContents = new List<String>();
            //newContents.Add(msgID);
            //newContents.Add(payload);

            using (StreamWriter w = new StreamWriter(@dirPath + "\\" + newFilename))
            {
                // Serialize works only with class. Not needed for now.
                // String json = JsonConvert.SerializeObject(newContents);
                w.Write(newContent);
            }
        }

        private static String ReadMsgFromNumber(String msg_number)
        {            
            String f = Environment.CurrentDirectory + HTTPServer.MSG_DIR + "\\" + msg_number + ".json";
            FileInfo file = new FileInfo(f);
            String json = "";

            if (file.Exists && file.Extension.Contains(".json"))
            {
                Debug.WriteLine(file.FullName);
                Debug.WriteLine(file.Name);
                using (StreamReader fi = File.OpenText(@file.FullName))
                {
                    json = fi.ReadToEnd();
                }                
            }
            return json;
        }

        private static List<String> ReadAllMsgs()
        {            
            String dirPath = Environment.CurrentDirectory + HTTPServer.MSG_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();

            List<String> json = new List<String>();
            foreach (FileInfo f in files)
            {
                if (f.Extension.Contains(".json"))
                {
                    String temp;
                    using (StreamReader file = File.OpenText(@f.FullName))
                    {
                        temp = file.ReadToEnd();
                    }
                    json.Add(temp);
                }
            }
            return json;
        }
    }
}
