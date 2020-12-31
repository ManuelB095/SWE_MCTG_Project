using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace MCTG_Bernacki
{
    public class Response : IResponse
    {
        private String status;
        private String mimeType;
        private String message;

        public const String EnvironmentPath = @"C:\Users\mbern\" +
        @"Downloads\FH-Stuff\3.Semester\SWE\MCTG\MCTG_Bernacki\bin\Debug\netcoreapp3.1";

        private Response(String status, String mimeType, String message)
        {            
            this.status = status;
            this.mimeType = mimeType;
            this.message = message;
        }

        public String GetStatus()
        {
            return this.status;
        }
        public String GetMimeType()
        {
            return this.mimeType;
        }
        public String GetMessage()
        {
            return this.message;
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
                    
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Header["Host"]);
                    return new Response("200 OK", "application/json", response);
                }
                else if(m.Success)
                {
                    int found = request.URL.IndexOf("/", 1);
                    String msg_number = request.URL.Substring(found + 1);
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Header["Host"]);

                    String json = Response.ReadMsgFromNumber(msg_number);
                    
                    if(json != "")
                    {
                        return new Response("200 OK", "application/json", json);
                    }      
                    Response notFound = Response.MakeFileNotFound();
                    return notFound;
                }
                Response badReq = Response.MakeBadRequest();
                return badReq;
            }
            else if(request.Type == "POST")
            {
                if(request.URL == "/sessions")
                {
                    UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(request.Payload);
                    Debug.WriteLine(ui.Username);
                    Debug.WriteLine(ui.Password);

                    if(Response.LogInUser(ui.Username, ui.Password) == true)
                    {
                        Response.CreateToken(ui.Username);
                        return new Response("200 OK", "text/plain", "Successfully logged in!\nWelcome " + ui.Username);
                    }
                    else
                    {
                        Response badLogin = Response.MakeBadLogin();
                        return badLogin;
                    }

                }

                else if(request.URL == "/users")
                {
                    UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(request.Payload);
                    Debug.WriteLine(ui.Username);
                    Debug.WriteLine(ui.Password);                    

                    if (Response.CreateUser(ui.Username, ui.Password) == true)
                    {
                        return new Response("200 OK", "text/plain", "Successfully created User: " + ui.Username);
                    }
                    else
                    {
                        Response badRequest = Response.MakeBadRequest();
                        return badRequest;
                    }
                }

                else if(request.URL == "/cards")
                {
                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        CardInfo ci = JsonConvert.DeserializeObject<CardInfo>(request.Payload);
                        if (Response.CreateCard(ci.Name, ci.Type, ci.Element, ci.Damage, ci.Price) == true)
                        {
                            return new Response("200 OK", "text/plain", "Successfully created Card: " + ci.Name);
                        }
                        else
                        {
                            Response badRequest = Response.MakeBadRequest();
                            return badRequest;
                        }
                    }
                    return new Response("200 OK", "text/plain", "Invalid Login-Token. Please login first!");
                }

                else if(request.URL == "/packages")
                {
                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        List<CardID> cd = JsonConvert.DeserializeObject<List<CardID>>(request.Payload);                   
                        if(Response.CreatePackage(cd) == true)
                        {
                            return new Response("200 OK", "text/plain", "Successfully created Package");
                        }
                        else
                        {
                            Response badRequest = Response.MakeBadRequest();
                            return badRequest;
                        }                        
                    }
                    else
                    {
                        Response badRequest = Response.MakeBadRequest();
                        return badRequest;
                    }
                }

                else if (request.Payload != "" && request.URL == "/messages")
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
            else if(request.Type == "DELETE")
            {
                if(m.Success)
                {
                    int found = request.URL.IndexOf("/", 1);
                    String msg_number = request.URL.Substring(found + 1);
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Header["Host"]);

                    if(Response.DeleteMsgFromNumber(msg_number))
                    {
                        Response.UpdateMsgEntrys(); // Renames files, so entrys are numbered consistently 1 to n again
                        return new Response("200 OK", "text/plain", "Successfully deleted " + msg_number + ".json" + "\n");
                    }
                    Response notFound = Response.MakeFileNotFound();
                    return notFound;                    
                }
                Response badRequest = Response.MakeBadRequest();
                return badRequest;
            }
            else if (request.Type == "PUT")
            {
                if (request.Payload != "" && m.Success)
                {
                    int found = request.URL.IndexOf("/", 1);
                    String msg_number = request.URL.Substring(found + 1);
                    Console.WriteLine("Message received: " + request.Type + request.URL + " " + request.Header["Host"]);

                    if(Response.ReplaceMsg(msg_number, request.Payload))
                    {
                        return new Response("200 OK", "text/plain", "Successfully overwritten message " + msg_number + ".json with text: \n\n" + request.Payload + "\n");
                    }                    
                    Response notFound = Response.MakeFileNotFound();
                    return notFound;
                }               
                Response badReq = Response.MakeBadRequest();
                return badReq;                
            }

            // If none of the above matches, Client sent unknown method to Server:
            Response notAllowed = Response.MakeMethodNotAllowed();
            return notAllowed;
        }

        public void Post(Stream stream)
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
            String path = EnvironmentPath + HTTPServer.STAT_DIR + "\\" + "400.html";
            FileInfo statCode = new FileInfo(path);
            if (statCode.Exists)
            {
                String fileContents = File.ReadAllText(@statCode.FullName);
                return new Response("400 Bad Request", "text/html", fileContents);
            }
            return new Response("400 Bad Request", "text/html", "400 Bad Request");
        }

        private static Response MakeBadLogin()
        {
            String path = EnvironmentPath + HTTPServer.STAT_DIR + "\\" + "401.html";
            FileInfo statCode = new FileInfo(path);
            if (statCode.Exists)
            {
                String fileContents = File.ReadAllText(@statCode.FullName);
                return new Response("401 Unauthorized", "text/html", fileContents);
            }
            return new Response("401 Unauthorized", "text/html", "401 Unauthorized");
        }

        private static Response MakeFileNotFound()
        {
            String path = EnvironmentPath + HTTPServer.STAT_DIR + "\\" + "404.html";
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
            String path = EnvironmentPath + HTTPServer.STAT_DIR + "\\" + "405.html";
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
            String dirPath = EnvironmentPath + HTTPServer.MSG_DIR;
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

        private static bool ReplaceMsg(String msg_number, String payload)
        {
            String f = EnvironmentPath + HTTPServer.MSG_DIR + "\\" + msg_number + ".json";
            FileInfo file = new FileInfo(f);
            String newContent = "{\"" + msg_number + "\":\"" + payload + "\"}";

            if (file.Exists && file.Extension.Contains(".json"))
            {
                File.WriteAllText(file.FullName, newContent); // Overwrites text inside file with new Content
                return true;
            }
            return false;
        }

        private static bool LogInUser(String _username, String _password)
        {            
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            using (var cmd = new NpgsqlCommand("Select * From Users Where username = @username and password = @password", conn))
            {
                cmd.Parameters.AddWithValue("username", _username);
                cmd.Parameters.AddWithValue("password", _password);
                cmd.Prepare();
                //cmd.ExecuteNonQuery();

                NpgsqlDataReader dr = cmd.ExecuteReader();
                
                if (dr.HasRows)
                {
                    conn.Close(); return true;
                }
                else
                {
                    conn.Close(); return false;
                }
            }   
        }

        private static bool CreateUser(String _username, String _password)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Users(username,password,admin) Values(@username,@password,0)", conn))
                {
                    cmd.Parameters.AddWithValue("username", _username);
                    cmd.Parameters.AddWithValue("password", _password);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        private static bool CreateCard(String _name, String _type, String _element, int _damage, int _price)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Card(name,type,element,damage,price) Values(@name,@type,@element,@damage,@price)", conn))
                {
                    cmd.Parameters.AddWithValue("name", _name);
                    cmd.Parameters.AddWithValue("type", _type);
                    cmd.Parameters.AddWithValue("element", _element);
                    cmd.Parameters.AddWithValue("damage", _damage);
                    cmd.Parameters.AddWithValue("price", _price);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        private static bool CreatePackage(List<CardID> cardIDs, int _price = 5)
        {
            int newPackageID = -1;

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Package(price) Values(@price)", conn))
                {
                    cmd.Parameters.AddWithValue("price", _price);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand("Select max(package_id) As maximum From Package", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while(dr.Read())
                        {
                            newPackageID = (int)dr["maximum"];                            
                        }
                    }
                    else
                        newPackageID = 1;
                }
                conn.Close();
                conn.Open();

                foreach (CardID cid in cardIDs)
                {                    
                    using (var cmd = new NpgsqlCommand("Insert Into packagecontent(package_id, card_id) Values(@packageid,@cardid)", conn))
                    {                        
                        cmd.Parameters.AddWithValue("packageid", newPackageID);
                        cmd.Parameters.AddWithValue("cardid", cid.ID);

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }                
                return true;
            }
            catch
            {
                return false;
            }

        }

        private static void CreateToken(String user)
        {
            String dirPath = EnvironmentPath + HTTPServer.TOK_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();

            String newFilename = user + "-mtcgToken" + ".tok";

            // Token stays valid for one day!
            String timestamp = DateTime.Now.AddDays(1).ToString();

            if(files.Length < 1)
            {
                using (StreamWriter w = new StreamWriter(@dirPath + "\\" + newFilename))
                {
                    w.Write(timestamp);
                }
                return;
            }
            foreach (FileInfo f in files)
            {
                if(f.Name == newFilename) //If old token already exists:
                {
                    File.WriteAllText(f.FullName, timestamp); // Override old timestamp
                    return;
                }
                else
                {
                    using (StreamWriter w = new StreamWriter(@dirPath + "\\" + newFilename))
                    {
                        w.Write(timestamp);
                    }
                    return;
                }
            }           
        }

        private static bool TokenIsValid(String user)
        {
            String dirPath = EnvironmentPath + HTTPServer.TOK_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();

            String search = user + ".tok";

            DateTime temp = new DateTime(2000,1,1,0,0,0);
            String timestamp = temp.ToString();
                      
            foreach (FileInfo f in files)
            {
                if (f.Name == search)
                {
                    timestamp = File.ReadAllText(f.FullName);
                    break;
                }                
            }

            if (DateTime.Compare(Convert.ToDateTime(timestamp), DateTime.Now) >= 0)
            {
                // Timestamp is later or same time as DateTime.Now
                return true;
            }
            else
            {
                // If nothing was found or Timestamp is simply obsolete
                return false;
            }
        }

        private static void UpdateMsgEntrys()
        {
            String dirPath = EnvironmentPath + HTTPServer.MSG_DIR;
            DirectoryInfo d = new DirectoryInfo(dirPath);
            FileInfo[] files = d.GetFiles();
            int idCounter = 1;

            foreach(FileInfo file in files)
            {
                if(file.Extension.Contains(".json"))
                {                                        
                    String oldFileContents = File.ReadAllText(file.FullName);
                    int found = file.Name.IndexOf(".json");
                    String oldID = file.Name.Substring(0, found);

                    String newFileContents = Response.ReplaceFirstOccurence(oldFileContents, oldID, idCounter.ToString());
                    String newFullName = dirPath + "\\" + idCounter.ToString() + ".json";

                    File.WriteAllText(file.FullName, newFileContents);
                    File.Move(file.FullName, newFullName, false);
                    idCounter++;
                }
            }
            Debug.Write("Updated Msg_Entrys\n");
        }

        private static bool DeleteMsgFromNumber(String msg_number)
        {
            String f = EnvironmentPath + HTTPServer.MSG_DIR + "\\" + msg_number + ".json";
            FileInfo file = new FileInfo(f);            

            if (file.Exists && file.Extension.Contains(".json"))
            {
                File.Delete(file.FullName);
                return true;
            }
            return false;
        }

        private static String ReadMsgFromNumber(String msg_number)
        {            
            String f = EnvironmentPath + HTTPServer.MSG_DIR + "\\" + msg_number + ".json";
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
            String dirPath = EnvironmentPath + HTTPServer.MSG_DIR;
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

        private static String ReplaceFirstOccurence(String s, String searchString, String replaceString)
        {
            int pos = s.IndexOf(searchString);
            if (pos < 0)
            {
                return s;
            }
            return s.Substring(0, pos) + replaceString + s.Substring(pos + searchString.Length);
        }
    }
}
