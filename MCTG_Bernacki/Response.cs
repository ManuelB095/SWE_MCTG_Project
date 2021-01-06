using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private static Mutex mutex = new Mutex();

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
                if(request.URL == "/cards")
                {
                    if(!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        int uid = Response.GetUIDFromToken(request.Header["Authorization"]);
                        List<Card> deck = Response.PrepareStack(uid);

                        if (deck.Count > 0)
                        {
                            String msg = "\nYou have the following Cards(ID) in your Posession: \n";
                            foreach (Card cd in deck)
                            {
                                msg += "\n" + cd.Name + "| Element: " + cd.Element.ToString() + "| Dmg: " + cd.Damage + "| Card-Price: " + cd.Price;
                            }
                            return new Response("200 OK", "text/plain", msg);

                        }
                    }
                    return new Response("200 OK", "text/plain", "Invalid Login-Token. Please login first!");
                }
                else if(request.URL == "/stats")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        int uid = Response.GetUIDFromToken(request.Header["Authorization"]);
                        String username = Response.GetUsernameFromUID(uid);
                        Dictionary<String, int> stats = Response.GetPlayerStats(uid);
                        int gamesPlayed = stats["wins"] + stats["losses"] + stats["draws"];

                        String statString = "\nPlayer-ID: ";
                        statString += uid;
                        statString += " with Username: " + username + "`s Stats are:\n";
                        statString += "Elo-Rating: " + stats["elo"] + "\n";
                        statString += "Wins: " + stats["wins"] + "\n";
                        statString += "Losses: " + stats["losses"] + "\n";
                        statString += "Draws: " + stats["draws"] + "\n";
                        statString += "Games Played: " + gamesPlayed + "\n";
                        statString += "Longest Winstreak: " + stats["winstreak"] + "\n";

                        statString += "\nLeaderboard-Position: " + Response.GetLeaderboardPos(uid) + "\n";

                        return new Response("200 OK", "text/plain", statString);
                    }
                    return new Response("200 OK", "text/plain", "Invalid Login-Token. Please login first!");
                }

                else if(request.URL == "/deck")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        int uid = Response.GetUIDFromToken(request.Header["Authorization"]);
                        List<Card> deck = Response.PrepareDeck(uid);
                        
                        if (deck.Count > 0)
                        {
                            String msg = "\nYou have the following Cards(ID) in your Deck: \n";
                            foreach (Card cd in deck)
                            {
                                msg += "\n" + cd.Name + "| Element: " + cd.Element.ToString() + "| Dmg: " + cd.Damage + "| Card-Price: " + cd.Price;
                            }
                            return new Response("200 OK", "text/plain", msg);

                        }
                    }
                    return new Response("200 OK", "text/plain", "Invalid Login-Token. Please login first!");
                }

                else if(request.URL == "/messages")
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
                        if(Response.CreateToken(ui.Username)==true)
                        {
                            return new Response("200 OK", "text/plain", "Successfully logged in!\nWelcome " + ui.Username);
                        }
                    }
                    else
                    {
                        Response badLogin = Response.MakeBadLogin();
                        return badLogin;
                    }

                }

                else if(request.URL == "/battles")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        // First things first, place entry in BattleRequests.
                        int player_pid = Response.GetUIDFromToken(request.Header["Authorization"]);
                        Response.PlaceBattleRequest(player_pid);

                        // Make an ID for possible Match:
                        int matchID = Response.GetMatchID();

                        Console.WriteLine("\nSearching for Opponent...");

                        // Then try to find opponent from BattleQueue:                        

                        // MUTEX LOCK HERE!
                        mutex.WaitOne();
                        bool match_running = Response.HasMatched(matchID); // If != -1 : Another thread has already matched and battles!
                        
                        if(match_running == false) // No match running yet => Search For Opponents and create one!
                        {
                            int tries = 0;
                            int maxTries = 100;
                            int opponent_pid = -1;

                            System.Threading.Thread.Sleep(12000); // Opponent (Me) needs a little time (12s) to send second BattleRequest!

                            do
                            {
                                opponent_pid = Response.FindOpponentForBattle(player_pid);
                                tries++;
                            } while (opponent_pid <= 0 && tries < maxTries);

                            if (tries >= maxTries) // => Opponent Not Found
                            {
                                mutex.ReleaseMutex();
                                Response.DeleteFromQueue(player_pid, player_pid);
                                return new Response("200 OK", "text/plain", "Unfortunately, there don`t seem to be any opponents.. ");
                            }

                            if(opponent_pid != -1) // => Opponent Found
                            {
                                Console.WriteLine("\nOpponent found!\nPlayer " + player_pid + "`s opponent is " + opponent_pid + "\n");
                                DateTime matchTime = DateTime.Now;
                                Response.CreateMatch(matchID, player_pid, opponent_pid, matchTime);                                

                                List<Card> playerDeck = Response.PrepareDeck(player_pid);
                                List<Card> opponentDeck = Response.PrepareDeck(opponent_pid);                                

                                BattleHandler game = new BattleHandler(playerDeck, opponentDeck);
                                int winner = game.Battle();
                                List<String> battleLogs = game.ReturnLogs();
                                Response.WriteLogsToFile(battleLogs, matchTime);

                                // Response String for Player One
                                String battleResult = "";
                                foreach (String s in battleLogs)
                                {
                                    battleResult += s;
                                }
                                battleResult += "\n";

                                // Update MatchHistory entry
                                Response.UpdateMatchHistory(player_pid, opponent_pid, winner, game.Rounds);

                                // Delete BattleQueue Entries for both players
                                Response.DeleteFromQueue(player_pid, opponent_pid);

                                // Update Elo and Scoreboard for both players
                                if(winner == 1)
                                {
                                    Response.UpdateEloRating(player_pid, '+', 3);
                                    Response.UpdateEloRating(opponent_pid, '-', 5);
                                    Response.UpdateWinnerScoreboard(player_pid);
                                    Response.UpdateLoserScoreboard(opponent_pid);
                                }
                                else if(winner == 2)
                                {
                                    Response.UpdateEloRating(opponent_pid, '+', 3);
                                    Response.UpdateEloRating(player_pid, '-', 5);
                                    Response.UpdateWinnerScoreboard(opponent_pid);
                                    Response.UpdateLoserScoreboard(player_pid);
                                }
                                else
                                {
                                    Response.UpdateDrawScoreboard(player_pid);
                                    Response.UpdateDrawScoreboard(opponent_pid);
                                }

                                


                                mutex.ReleaseMutex();
                                return new Response("200 OK", "text/plain", battleResult);
                            }

                        }  
                        else if(match_running == true) //Someone has taken up the offer and run the match! => Retrieve the data!
                        {
                            Response.DeleteFromQueue(player_pid, player_pid);
                            DateTime time = Response.GetMatchDateFromID(matchID);
                            String logName = time.ToString("yyyy-dd-M--HH-mm-ss");
                            String logContent = Response.ReadLogsFromFile(logName);
                            mutex.ReleaseMutex();
                            return new Response("200 OK", "text/plain", logContent);
                        }  
                    }
                }

                else if(request.URL == "/users")
                {
                    UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(request.Payload);
                    Debug.WriteLine(ui.Username);
                    Debug.WriteLine(ui.Password);                    

                    if (Response.CreateUser(ui.Username, ui.Password) == true && !(Response.UserExists(ui.Username)))
                    {
                        return new Response("200 OK", "text/plain", "Successfully created User: " + ui.Username);
                    }
                    else if(Response.UserExists(ui.Username))
                    {
                        return new Response("200 OK", "text/plain", "Username " + ui.Username + " already exists.\nPlease choose another\n");
                    }
                    else
                    {
                        Response badRequest = Response.MakeBadRequest();
                        return badRequest;
                    }
                }

                else if(request.URL == "/cards")
                {
                    int uid = Response.GetUIDFromToken(request.Header["Authorization"]);
                    if (Response.TokenIsValid(request.Header["Authorization"]) && Response.UserIsAdmin(uid))
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







                else if(request.URL == "/transactions/packages")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    if (Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        int uid = Response.GetUIDFromToken(request.Header["Authorization"]);

                        // Random Chance, which package the user is gonna get:
                        int pckgToBuy = Response.ChooseRandomPackage();
                        if(pckgToBuy == -1)
                        {
                            return new Response("200 OK", "text/plain", "No packages available!");
                        }

                        if (Response.BuyPackage(uid,pckgToBuy))
                        {
                            return new Response("200 OK", "text/plain", "You bought a package!");
                        }
                        return new Response("200 OK", "text/plain", "Not enough money to buy the package!");

                    }
                    return new Response("200 OK", "text/plain", "Invalid Login-Token. Please login first!");
                }

                else if(request.URL == "/packages")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    int uid = Response.GetUIDFromToken(request.Header["Authorization"]);
                    if (Response.TokenIsValid(request.Header["Authorization"]) && Response.UserIsAdmin(uid))
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
                        Response badLogin = Response.MakeBadLogin();
                        return badLogin;
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

                if(request.URL == "/deck")
                {
                    if (!request.Header.ContainsKey("Authorization"))
                    {
                        Response badRes = Response.MakeBadRequest();
                        return badRes;
                    }

                    int deckSize = 5;
                    if(Response.TokenIsValid(request.Header["Authorization"]))
                    {
                        int pid = Response.GetUIDFromToken(request.Header["Authorization"]);
                        List<CardID>deck = JsonConvert.DeserializeObject<List<CardID>>(request.Payload);
                        if(deck.Count != deckSize)
                        {
                            return new Response("200 OK", "text/plain", "Wrong number of Cards!\nNeed exactly " + deckSize + " Cards in Deck!\n");
                        }

                        if (Response.CheckIfCardsInStack(pid, deck) == true && deck.Count == deckSize)
                        {
                            // Good! - Player owns the Cards AND the right amount of Cards was sent. 
                            // Now we can make configurations on the deck:
                            if(Response.ConfigureDeck(pid, deck)==true)
                            {
                                return new Response("200 OK", "text/plain", "Successfully configured deck!\n");
                            }
                        }
                        return new Response("200 OK", "text/plain", "Could not create Deck with Cards you don`t own.\n");
                    }
                    Response badLogin = Response.MakeBadLogin();
                    return badLogin;
                }

                else if (request.Payload != "" && m.Success)
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
            int uid = -1;
            int startCoins = 20;
            int startElo = 100;
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
                conn.Close();
                conn.Open();

                using (var cmd = new NpgsqlCommand("Select uid From Users Where username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", _username);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        uid = (int)reader["uid"];
                    }

                }
                conn.Close();
                conn.Open();

                using (var cmd = new NpgsqlCommand("Insert Into Player(pid,coins,elo) Values(@pid,@coins,@elo)", conn))
                {
                    cmd.Parameters.AddWithValue("pid", uid);
                    cmd.Parameters.AddWithValue("coins", startCoins);
                    cmd.Parameters.AddWithValue("elo", startElo);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
                conn.Open();

                using (var cmd = new NpgsqlCommand("Insert Into Scoreboard(pid,elo,wins,losses,draws,winstreak) Values(@pid,@elo,@wins,@losses,@draws,@winstreak)", conn))
                {
                    cmd.Parameters.AddWithValue("pid", uid);
                    cmd.Parameters.AddWithValue("elo", 100);
                    cmd.Parameters.AddWithValue("wins", 0);
                    cmd.Parameters.AddWithValue("losses", 0);
                    cmd.Parameters.AddWithValue("draws", 0);
                    cmd.Parameters.AddWithValue("winstreak", 0);

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
            int newPackageID = Response.GetHighestPackageId();
            if (newPackageID == -1)
                newPackageID = 1;
            else
                newPackageID += 1;

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

        private static bool CreateToken(String user)
        {
            String dirPath = EnvironmentPath + HTTPServer.TOK_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();

            String tokenName = user + "-mtcgToken";
            String newFilename = tokenName + ".tok";

            // Token stays valid for one day!
            String timestamp = DateTime.Now.AddDays(1).ToString();

            if(files.Length < 1)
            {
                using (StreamWriter w = new StreamWriter(@dirPath + "\\" + newFilename))
                {
                    w.Write(timestamp);
                }                
            }
            foreach (FileInfo f in files)
            {
                if(f.Name == newFilename) //If old token already exists:
                {
                    File.WriteAllText(f.FullName, timestamp); // Override old timestamp
                    break;
                }
                else
                {
                    using (StreamWriter w = new StreamWriter(@dirPath + "\\" + newFilename))
                    {
                        w.Write(timestamp);
                    }
                }
            }

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Update Users Set token = @token Where username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("token", tokenName);
                    cmd.Parameters.AddWithValue("username", user);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while posting token to DataBase!");
                return false;
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

        private static String GetUsernameFromToken(String token)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select username From Users Where token = @token", conn))
                {
                    cmd.Parameters.AddWithValue("token", token);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        return (String)reader["username"];
                    }
                    return "ERROR";
                }
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while posting token to DataBase!");
                return "ERROR";
            }
        }

        private static String GetUsernameFromUID(int uid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select username From Users Where uid = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("uid", uid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (String)reader["username"];
                    }
                    return "ERROR";
                }
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while posting token to DataBase!");
                return "ERROR";
            }
        }

        private static void PlaceBattleRequest(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into BattleQueue(pid) Values(@pid)", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();                   
                }
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while trying to place Battle request!");
                throw;
            }
        }

        private static int FindOpponentForBattle(int player_pid)
        {
            List<int> allOpponents = new List<int>();
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            // TO DO: Add ELO Comparing System to find players on similar levels.

            try
            {
                using (var cmd = new NpgsqlCommand("Select pid From BattleQueue Where pid != @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", player_pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    if(!reader.HasRows)
                    {
                        return -1; // No opponent found!
                    }
                    else
                    {
                        while(reader.Read())
                        {
                            for(int i=0; i<reader.FieldCount;i++)
                            {
                                allOpponents.Add((int)reader[i]);
                            }
                        }

                        // Choose an opponent from existing list at random
                        Random rnd = new Random();                        
                        int opponent_pid = rnd.Next(1, allOpponents.Count);
                        return allOpponents[opponent_pid - 1];
                    }
                }                
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while trying to FindOpponentForBattle");
                throw;
            }
        }

        private static int GetUIDFromToken(String token)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select uid From Users Where token = @token", conn))
                {
                    cmd.Parameters.AddWithValue("token", token);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["uid"];
                    }
                    return -1;
                }
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while posting token to DataBase!");
                return -1;
            }
        }

        private static List<int> GetStack(int uid)
        {
            List<int> CardIDs = new List<int>();

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select card_id From cardinstack Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", uid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            CardIDs.Add((int)reader[i]);
                        }
                    }
                }
                return CardIDs;
            }
            catch
            {
                //throw;
            }
            return CardIDs;
        }

        private static bool BuyPackage(int _pid, int package_id)
        {
            int availableCoins = -1;
            int packagePrice = -1;
            List<int> CardIDs = new List<int>();

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select coins From Player Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        availableCoins = (int)reader["coins"];
                    }
                }
                conn.Close();
                conn.Open();

                using (var cmd = new NpgsqlCommand("Select price From Package Where package_id = @package_id", conn))
                {
                    cmd.Parameters.AddWithValue("package_id", package_id);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        packagePrice = (int)reader["price"];
                    }
                }
                conn.Close();
                conn.Open();

                if (availableCoins >= packagePrice)
                {
                    int newBalance = availableCoins - packagePrice;

                    using (var cmd = new NpgsqlCommand("Update Player Set coins = @coins Where pid = @pid", conn))
                    {
                        cmd.Parameters.AddWithValue("coins", newBalance);
                        cmd.Parameters.AddWithValue("pid", _pid);

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                    conn.Open();

                    bool sold = true;
                    using (var cmd = new NpgsqlCommand("Update Package Set sold = @sold Where package_id = @packageid", conn))
                    {
                        cmd.Parameters.AddWithValue("sold", sold);
                        cmd.Parameters.AddWithValue("packageid", package_id);

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                    conn.Open();

                    using (var cmd = new NpgsqlCommand("Select card_id From packagecontent Where package_id = @package_id", conn))
                    {
                        cmd.Parameters.AddWithValue("package_id", package_id);
                        cmd.Prepare();
                        NpgsqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            for(int i=0; i < reader.FieldCount;i++)
                            {
                                CardIDs.Add((int)reader[i]);
                            }
                        }
                    }
                    conn.Close();
                    conn.Open();

                    foreach(int id in CardIDs)
                    {
                        if(Response.AlreadyOwnsCard(_pid, id) == false)
                        {
                            using (var cmd = new NpgsqlCommand("Insert Into cardinstack(pid, card_id) Values(@pid, @cardid)", conn))
                            {
                                cmd.Parameters.AddWithValue("pid", _pid);
                                cmd.Parameters.AddWithValue("cardid", id);

                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    conn.Close();                    
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Some error in DataBase while posting token to DataBase!");
                return false;
            }
            return false;
        }

        private static bool CheckIfCardsInStack(int _pid, List<CardID>cardIDs)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            
            try
            {
                foreach (CardID id in cardIDs)
                {
                    using (var cmd = new NpgsqlCommand("Select card_id From cardinstack Where pid = @pid And card_id = @cardid", conn))
                    {
                        cmd.Parameters.AddWithValue("pid", _pid);
                        cmd.Parameters.AddWithValue("cardid", id.ID);

                        cmd.Prepare();
                        NpgsqlDataReader reader = cmd.ExecuteReader();

                        if(!reader.HasRows)
                        {
                            return false; // User does not have all cards in his stack! => Bad request!
                        }
                        conn.Close();
                        conn.Open();
                    }                    
                }                
            }
            catch
            {
                throw;
            }
            // All checks have passed. User has every card in his stack.
            return true;            
         }

        private static bool ConfigureDeck(int _pid, List<CardID>deck)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            // TO DO: Get Current Deck and save it in case of error.
            // Then Delete Current Deck of user.

            try
            {
                using (var cmd = new NpgsqlCommand("Delete From cardindeck Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {
                // Restore Player`s Deck here
                throw;
            }
            conn.Close();
            conn.Open();
            
            try
            {
                foreach (CardID id in deck)
                {
                    using (var cmd = new NpgsqlCommand("Insert Into cardindeck(pid, card_id) Values(@pid, @cardid)", conn))
                    {
                        cmd.Parameters.AddWithValue("pid", _pid);
                        cmd.Parameters.AddWithValue("cardid", id.ID);
                        cmd.Prepare();
                        cmd.ExecuteReader();                       
                    }
                    conn.Close();
                    conn.Open();
                }                
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static CardInfo GetCardInfoFromID(int _cardID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            using (var cmd = new NpgsqlCommand("Select name,type,element,damage,price From Card Where card_id = @cardid", conn))
            {
                cmd.Parameters.AddWithValue("cardid", _cardID);
                cmd.Prepare();
                NpgsqlDataReader reader = cmd.ExecuteReader();
                String name = "Generic";
                String type = "Goblin";
                String element = "Normal";
                int damage = 1;
                int price = 0;

                while(reader.Read())
                {
                    name = (String)reader["name"];
                    type = (String)reader["type"];
                    element = (String)reader["element"];
                    damage = (int)reader["damage"];
                    price = (int)reader["price"];
                }
                
                var ci = new CardInfo(name, type, element, damage, price);
                conn.Close();

                return ci;
            }
        }

        private static List<int> GetDeck(int _pid)
        {
            List<int> CardIDs = new List<int>();

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select card_id From cardindeck Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            CardIDs.Add((int)reader[i]);
                        }
                    }
                }
                return CardIDs;
            }
            catch
            {
                //throw;
            }
            return CardIDs;
        }

        private static Card GetCardFromCardInfo(CardInfo ci)
        {
            if(ci.Type.ToUpper() == "GOBLIN")            
                return new Goblin(ci.Name,ci.Price,Response.StrtoElem(ci.Element), ci.Damage);
            else if(ci.Type.ToUpper() == "ORC")
                return new Orc(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "FIREELVE")
                return new FireElve(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "DRAGON")
                return new Dragon(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "KNIGHT")
                return new Knight(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "KRAKEN")
                return new Kraken(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "WIZZARD")
                return new Wizzard(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "SPELL")
            {
                if(Response.StrtoElem(ci.Element) == element.NORMAL)
                    return new NormalSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else if(Response.StrtoElem(ci.Element) == element.WATER)
                    return new WaterSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else if (Response.StrtoElem(ci.Element) == element.FIRE)
                    return new FireSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else
                    return new NormalSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            }
            else
                return new Goblin(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
        }

        private static void DeleteFromQueue(int pl1ID, int pl2ID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();            

            try
            {               
                using (var cmd = new NpgsqlCommand("Delete From battlequeue Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", pl1ID);
                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
                conn.Close();
                conn.Open();
                using (var cmd = new NpgsqlCommand("Delete From battlequeue Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", pl2ID);
                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {               
                throw;
            }
        }

        private static bool UserExists(String _username)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select * From Users Where username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", _username);                    

                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        private static int GetLeaderboardPos(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            List<int>eloScores = new List<int>();
            int playerElo = Response.GetEloRating(_pid);

            try
            {
                using (var cmd = new NpgsqlCommand("Select elo From Scoreboard", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        for(int i=0; i<reader.FieldCount;i++)
                        {
                            eloScores.Add((int)reader[i]);
                        }
                    }
                }

                eloScores.Sort(); // Sorts ascending, i.e. : 1, 4, 6, 7...
                eloScores.Reverse(); // Now its descending, i.e. : 7, 6, 4, 1...
                for(int i=0; i<eloScores.Count;i++)
                {
                    if (eloScores[i] == playerElo)
                        return i+1;
                }
                return -1; // Could not determine Score
            }
            catch
            {
                throw;
            }
        }

        private static void CreateMatch(int matchID, int playerID, int opponentID, DateTime matchTime)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into MatchHistory(match_id,playeroneid,playertwoid,status,matchdate) Values(@matchid,@pidOne,@pidTwo,@status,@date)", conn))
                {
                    cmd.Parameters.AddWithValue("matchid", matchID);
                    cmd.Parameters.AddWithValue("pidOne", playerID);
                    cmd.Parameters.AddWithValue("pidTwo", opponentID);
                    cmd.Parameters.AddWithValue("status", "Running");
                    cmd.Parameters.AddWithValue("date", matchTime);

                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {
                throw;
            }
        }

        private static int GetWins(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select wins From Scoreboard Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["wins"];
                    }
                }
                conn.Close();
                return -1;
            }
            catch
            {
                throw;
            }
        }

        private static int GetDraws(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select draws From Scoreboard Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["draws"];
                    }
                }
                conn.Close();
                return -1;
            }
            catch
            {
                throw;
            }
        }

        private static int GetLosses(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select losses From Scoreboard Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["losses"];
                    }
                }
                conn.Close();
                return -1;
            }
            catch
            {
                throw;
            }
        }

        private static int GetWinstreak(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select winstreak From Scoreboard Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["winstreak"];
                    }
                }
                conn.Close();
                return -1;
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateWinnerScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = Response.GetEloRating(_pid); // There is another Function for that!           
            int wins = Response.GetWins(_pid);
            wins += 1;
            int winstreak = Response.GetWinstreak(_pid);
            winstreak += 1;

            try
            {
                using (var cmd = new NpgsqlCommand("Update Scoreboard Set elo = @elo, wins = @wins, winstreak = @winstreak Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("elo", elo);
                    cmd.Parameters.AddWithValue("wins", wins);
                    cmd.Parameters.AddWithValue("winstreak", winstreak);
                    cmd.Parameters.AddWithValue("pid", _pid);

                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateLoserScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = Response.GetEloRating(_pid);
            int losses = Response.GetLosses(_pid);
            losses += 1;
            int winstreak = 0;

            try
            {
                using (var cmd = new NpgsqlCommand("Update Scoreboard Set elo = @elo, losses = @losses, winstreak = @winstreak Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("elo", elo);
                    cmd.Parameters.AddWithValue("losses", losses);
                    cmd.Parameters.AddWithValue("winstreak", winstreak);
                    cmd.Parameters.AddWithValue("pid", _pid);

                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateDrawScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = Response.GetEloRating(_pid);
            int draws = Response.GetDraws(_pid);
            draws += 1;
            int winstreak = 0;

            try
            {
                using (var cmd = new NpgsqlCommand("Update Scoreboard Set draws = @draws, winstreak = @winstreak Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("draws", draws);
                    cmd.Parameters.AddWithValue("winstreak", winstreak);
                    cmd.Parameters.AddWithValue("pid", _pid);

                    cmd.Prepare();
                    cmd.ExecuteReader();
                }
            }
            catch
            {
                throw;
            }
        }

        private static Dictionary<String,int> GetPlayerStats(int _pid)
        {
            Dictionary<String,int> Stats = new Dictionary<String, int>();

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select * From scoreboard Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Stats.Add("elo", (int)reader["elo"]);
                        Stats.Add("wins", (int)reader["wins"]);
                        Stats.Add("losses", (int)reader["losses"]);
                        Stats.Add("draws", (int)reader["draws"]);
                        Stats.Add("winstreak", (int)reader["winstreak"]);
                    }
                }
                return Stats;
            }
            catch
            {
                throw;
            }          
        }

        private static bool MatchesExist()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select * From matchhistory", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                            return true;
                        else
                            return false;
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        private static int GetMatchID()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            if (!Response.MatchesExist())
                return 1;

            try
            {
                using (var cmd = new NpgsqlCommand("Select max(match_id) As result From matchhistory", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (!reader.HasRows)
                            return 1;

                        int newID = (int)reader["result"];
                        newID += 1;
                        return newID;
                    }
                }
                return -1; // In case something went horribly wrong..
            }
            catch
            {
                throw;
            }
        }

        private static int GetEloRating(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            
            try
            {
                using (var cmd = new NpgsqlCommand("Select elo From Player Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["elo"];
                    }
                }
                conn.Close();
                return -1;
            }
            catch
            {
                throw;
            }
        }


        private static void UpdateEloRating(int _pid, char c, int value)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = Response.GetEloRating(_pid); // Get Current Elo

            if (c == '+')
                elo += value;
            else if (c == '-')
                elo -= value;  

            try
            {
                using (var cmd = new NpgsqlCommand("Update Player Set elo = @elo Where pid = @pid", conn))
                {
                    cmd.Parameters.AddWithValue("elo", elo);
                    cmd.Parameters.AddWithValue("pid", _pid);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();                   
                }
                conn.Close();                
            }
            catch
            {
                throw;
            }
        }

        private static bool AlreadyOwnsCard(int _pid, int cardID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select * From cardinstack Where pid = @pid And card_id = @cardid", conn))
                {
                    cmd.Parameters.AddWithValue("pid", _pid);
                    cmd.Parameters.AddWithValue("cardid", cardID);

                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        conn.Close();
                        return true;
                    }
                }
                conn.Close();
                return false;
            }
            catch
            {
                throw;
            }
        }

        private static int ChooseRandomPackage()
        {
            List<int> unsoldIDs = Response.GetUnsoldPackages();
            if (unsoldIDs.Count <= 0)
            {
                return -1;
            }

            Random rnd = new Random();
            int rndElement = rnd.Next(1,unsoldIDs.Count);
            int chosenID = unsoldIDs[rndElement - 1];
            return chosenID;            
        }

        private static List<int> GetUnsoldPackages()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            bool sold = false;
            List<int> unsoldIDs = new List<int>(); 

            try
            {
                using (var cmd = new NpgsqlCommand("Select package_id From package Where sold = @sold", conn))
                {
                    cmd.Parameters.AddWithValue("sold", sold);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        unsoldIDs.Add((int)reader["package_id"]);
                    }
                }
                return unsoldIDs;
            }
            catch
            {
                throw;
            }
        }

        private static bool PackageStillAvailable(int packageID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select sold From package Where package_id = @packageid", conn))
                {
                    cmd.Parameters.AddWithValue("packageid", packageID);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        bool isSold = (bool)reader["sold"];
                        if (isSold)
                        {
                            return false;
                        }
                        else
                            return true;
                    }
                }
                return false; 
            }
            catch
            {
                throw;
            }
        }



        private static int GetLowestPackageId()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select min(package_id) As result From package", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {  
                        return (int)reader["result"];
                    }
                }
                return -2;
            }
            catch
            {
                return -1; // In case database is empty
            }
        }

        private static int GetHighestPackageId()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select max(package_id) As result From package", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (int)reader["result"];
                    }
                }
                return -2;
            }
            catch
            {
                return -1; // In case no packages exist!!
            }
        }

        private static String ReadLogsFromFile(String filename)
        {
            String f = EnvironmentPath + HTTPServer.LOG_DIR + "\\" + filename + ".log";
            FileInfo file = new FileInfo(f);
            String logString = "";

            if (file.Exists && file.Extension.Contains(".log"))
            {
                Debug.WriteLine(file.FullName);
                Debug.WriteLine(file.Name);
                using (StreamReader fi = File.OpenText(@file.FullName))
                {
                    logString = fi.ReadToEnd();
                }
            }
            return logString;
        }

        private static void WriteLogsToFile(List<String>logs, DateTime matchTime)
        {
            String dirPath = EnvironmentPath + HTTPServer.LOG_DIR;
            DirectoryInfo dif = new DirectoryInfo(dirPath);
            FileInfo[] files = dif.GetFiles();

            String newFilename = matchTime.ToString("yyyy-dd-M--HH-mm-ss"); ;
            newFilename += ".log";

            using (StreamWriter tw = new StreamWriter(@dirPath + "\\" + newFilename))
            {
                foreach (String s in logs)
                    tw.WriteLine(s);
            }
        }        

        private static bool HasMatched(int matchID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select * From matchhistory Where match_id = @matchid", conn))
                {
                    cmd.Parameters.AddWithValue("matchid", matchID);
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while(reader.HasRows)
                    {
                        return true;
                    }
                }
                return false; // Match does not yet exist!
            }
            catch
            {
                throw;
            }
        }

        private static List<Card> PrepareDeck(int pid)
        {
            // -> Get players cardID while looping database function, that looks for them in CardInDeck
            List<int> CardIDs = Response.GetDeck(pid);

            // -> Get card Information by looping database function, that gets card data for every cardID
            List<CardInfo> CardInfo = new List<CardInfo>();

            for (int i = 0; i < CardIDs.Count; i++)
            {
                CardInfo.Add(Response.GetCardInfoFromID(CardIDs[i]));
            }

            List<Card> Deck = new List<Card>();

            for (int i = 0; i < CardInfo.Count; i++)
            {
                Deck.Add(Response.GetCardFromCardInfo(CardInfo[i]));                
            }
            return Deck;
        }

        private static List<Card> PrepareStack(int pid)
        {
            // -> Get players cardID while looping database function, that looks for them in CardInDeck
            List<int> CardIDs = Response.GetStack(pid);

            // -> Get card Information by looping database function, that gets card data for every cardID
            List<CardInfo> CardInfo = new List<CardInfo>();

            for (int i = 0; i < CardIDs.Count; i++)
            {
                CardInfo.Add(Response.GetCardInfoFromID(CardIDs[i]));
            }

            List<Card> Deck = new List<Card>();

            for (int i = 0; i < CardInfo.Count; i++)
            {
                Deck.Add(Response.GetCardFromCardInfo(CardInfo[i]));
            }
            return Deck;
        }

        private static DateTime GetMatchDateFromID(int _matchID)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select matchdate From matchhistory Where match_id = @matchid", conn))
                {
                    cmd.Parameters.AddWithValue("matchid", _matchID);

                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return (DateTime)reader["matchdate"]; ;
                    }
                }
                return DateTime.Now.AddDays(-1);
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateMatchHistory(int playerID, int opponentID, int winner, int rounds)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Update matchhistory Set status = @poststatus, winner = @winner, rounds = @rounds Where playeroneid = @pidOne And status = @prestatus", conn))
                {
                    cmd.Parameters.AddWithValue("poststatus", "Done");
                    cmd.Parameters.AddWithValue("winner", winner);
                    cmd.Parameters.AddWithValue("rounds", rounds);
                    cmd.Parameters.AddWithValue("pidOne", playerID);
                    cmd.Parameters.AddWithValue("prestatus", "Running");

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

        private static bool UserIsAdmin(int _uid)
        {
            int result = 0;
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select admin From Users Where uid = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("uid", _uid);                    

                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        result = (int)reader["admin"];
                    }                    
                }
                if (result == 1)
                    return true;
                else
                    return false;
            }
            catch
            {
                throw;
            }
        }

        private static element StrtoElem(String s)
        {
            if (s.ToUpper() == "WATER")
            {
                return element.WATER;
            }
            else if (s.ToUpper() == "FIRE")
            {
                return element.FIRE;
            }
            else if (s.ToUpper() == "NORMAL")
            {
                return element.NORMAL;
            }
            else
                return element.NORMAL;
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
