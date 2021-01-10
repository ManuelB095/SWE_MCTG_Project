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
                        int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                        List<Card> deck = DBHandler.PrepareStack(uid);

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
                        int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                        String username = DBHandler.GetUsernameFromUID(uid);
                        Dictionary<String, int> stats = DBHandler.GetPlayerStats(uid);
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

                        statString += "\nLeaderboard-Position: " + DBHandler.GetLeaderboardPos(uid) + "\n";

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
                        int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                        List<Card> deck = DBHandler.PrepareDeck(uid);
                        
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

                    if(DBHandler.LogInUser(ui.Username, ui.Password) == true)
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
                        int player_pid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                        DBHandler.PlaceBattleRequest(player_pid);

                        // Make an ID for possible Match:
                        int matchID = DBHandler.GetMatchID();

                        Console.WriteLine("\nSearching for Opponent...");

                        // Then try to find opponent from BattleQueue:                        

                        // MUTEX LOCK HERE!
                        mutex.WaitOne();
                        bool match_running = DBHandler.HasMatched(matchID); // If != -1 : Another thread has already matched and battles!
                        
                        if(match_running == false) // No match running yet => Search For Opponents and create one!
                        {
                            int tries = 0;
                            int maxTries = 100;
                            int opponent_pid = -1;

                            System.Threading.Thread.Sleep(12000); // Opponent (Me) needs a little time (12s) to send second BattleRequest!

                            do
                            {
                                opponent_pid = DBHandler.FindOpponentForBattle(player_pid);
                                tries++;
                            } while (opponent_pid <= 0 && tries < maxTries);

                            if (tries >= maxTries) // => Opponent Not Found
                            {
                                mutex.ReleaseMutex();
                                DBHandler.DeleteFromQueue(player_pid, player_pid);
                                return new Response("200 OK", "text/plain", "Unfortunately, there don`t seem to be any opponents.. ");
                            }

                            if(opponent_pid != -1) // => Opponent Found
                            {
                                Console.WriteLine("\nOpponent found!\nPlayer " + player_pid + "`s opponent is " + opponent_pid + "\n");
                                DateTime matchTime = DateTime.Now;
                                DBHandler.CreateMatch(matchID, player_pid, opponent_pid, matchTime);                                

                                List<Card> playerDeck = DBHandler.PrepareDeck(player_pid);
                                List<Card> opponentDeck = DBHandler.PrepareDeck(opponent_pid);                                

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
                                DBHandler.UpdateMatchHistory(player_pid, opponent_pid, winner, game.Rounds);

                                // Delete BattleQueue Entries for both players
                                DBHandler.DeleteFromQueue(player_pid, opponent_pid);

                                // Update Elo and Scoreboard for both players
                                if(winner == 1)
                                {
                                    DBHandler.UpdateEloRating(player_pid, '+', 3);
                                    DBHandler.UpdateEloRating(opponent_pid, '-', 5);
                                    DBHandler.UpdateWinnerScoreboard(player_pid);
                                    DBHandler.UpdateLoserScoreboard(opponent_pid);
                                }
                                else if(winner == 2)
                                {
                                    DBHandler.UpdateEloRating(opponent_pid, '+', 3);
                                    DBHandler.UpdateEloRating(player_pid, '-', 5);
                                    DBHandler.UpdateWinnerScoreboard(opponent_pid);
                                    DBHandler.UpdateLoserScoreboard(player_pid);
                                }
                                else
                                {
                                    DBHandler.UpdateDrawScoreboard(player_pid);
                                    DBHandler.UpdateDrawScoreboard(opponent_pid);
                                }                               


                                mutex.ReleaseMutex();
                                return new Response("200 OK", "text/plain", battleResult);
                            }

                        }  
                        else if(match_running == true) //Someone has taken up the offer and run the match! => Retrieve the data!
                        {
                            DBHandler.DeleteFromQueue(player_pid, player_pid);
                            DateTime time = DBHandler.GetMatchDateFromID(matchID);
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

                    if (!(DBHandler.UserExists(ui.Username)))
                    {
                        if(DBHandler.CreateUser(ui.Username, ui.Password) == true)
                            return new Response("200 OK", "text/plain", "Successfully created User: " + ui.Username);
                        else
                            return new Response("200 OK", "text/plain", "Unsuccessful by creating user");
                    }
                    else if(DBHandler.UserExists(ui.Username))
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
                    int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                    if (Response.TokenIsValid(request.Header["Authorization"]) && DBHandler.UserIsAdmin(uid))
                    {
                        CardInfo ci = JsonConvert.DeserializeObject<CardInfo>(request.Payload);
                        if (DBHandler.CreateCard(ci.Name, ci.Type, ci.Element, ci.Damage, ci.Price) == true)
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
                        int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);

                        // Random Chance, which package the user is gonna get:
                        int pckgToBuy = DBHandler.GetHighestUnsoldPackageId(); // Alternativ: DBHandler.ChooseRandomPackage();
                        
                        if(pckgToBuy == -1)
                        {
                            return new Response("200 OK", "text/plain", "No packages available!");
                        }

                        if (DBHandler.BuyPackage(uid,pckgToBuy))
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

                    int uid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                    if (Response.TokenIsValid(request.Header["Authorization"]) && DBHandler.UserIsAdmin(uid))
                    {
                        List<CardID> cd = JsonConvert.DeserializeObject<List<CardID>>(request.Payload);                   
                        if(DBHandler.CreatePackage(cd) == true)
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
                        int pid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                        List<CardID>deck = JsonConvert.DeserializeObject<List<CardID>>(request.Payload);
                        if(deck.Count != deckSize)
                        {
                            return new Response("200 OK", "text/plain", "Wrong number of Cards!\nNeed exactly " + deckSize + " Cards in Deck!\n");
                        }

                        if (DBHandler.CheckIfCardsInStack(pid, deck) == true && deck.Count == deckSize)
                        {
                            // Good! - Player owns the Cards AND the right amount of Cards was sent. 
                            // Now we can make configurations on the deck:
                            if(DBHandler.ConfigureDeck(pid, deck)==true)
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

            if (DBHandler.AddToken(tokenName, user))
                return true;
            else
                return false;

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

        private static Response HandleBattle(Request request)
        {
            if (!request.Header.ContainsKey("Authorization"))
            {
                Response badRes = Response.MakeBadRequest();
                return badRes;
            }

            if (Response.TokenIsValid(request.Header["Authorization"]))
            {
                // First things first, place entry in BattleRequests.
                int player_pid = DBHandler.GetUIDFromToken(request.Header["Authorization"]);
                DBHandler.PlaceBattleRequest(player_pid);

                // Make an ID for possible Match:
                int matchID = DBHandler.GetMatchID();

                Console.WriteLine("\nSearching for Opponent...");

                // Then try to find opponent from BattleQueue:                        

                // MUTEX LOCK HERE!
                mutex.WaitOne();
                bool match_running = DBHandler.HasMatched(matchID); // If != -1 : Another thread has already matched and battles!

                if (match_running == false) // No match running yet => Search For Opponents and create one!
                {
                    int tries = 0;
                    int maxTries = 100;
                    int opponent_pid = -1;

                    System.Threading.Thread.Sleep(12000); // Opponent (Me) needs a little time (12s) to send second BattleRequest!

                    do
                    {
                        opponent_pid = DBHandler.FindOpponentForBattle(player_pid);
                        tries++;
                    } while (opponent_pid <= 0 && tries < maxTries);

                    if (tries >= maxTries) // => Opponent Not Found
                    {
                        mutex.ReleaseMutex();
                        DBHandler.DeleteFromQueue(player_pid, player_pid);
                        return new Response("200 OK", "text/plain", "Unfortunately, there don`t seem to be any opponents.. ");
                    }

                    if (opponent_pid != -1) // => Opponent Found
                    {
                        Console.WriteLine("\nOpponent found!\nPlayer " + player_pid + "`s opponent is " + opponent_pid + "\n");
                        DateTime matchTime = DateTime.Now;
                        DBHandler.CreateMatch(matchID, player_pid, opponent_pid, matchTime);

                        List<Card> playerDeck = DBHandler.PrepareDeck(player_pid);
                        List<Card> opponentDeck = DBHandler.PrepareDeck(opponent_pid);

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
                        DBHandler.UpdateMatchHistory(player_pid, opponent_pid, winner, game.Rounds);

                        // Delete BattleQueue Entries for both players
                        DBHandler.DeleteFromQueue(player_pid, opponent_pid);

                        // Update Elo and Scoreboard for both players
                        if (winner == 1)
                        {
                            DBHandler.UpdateEloRating(player_pid, '+', 3);
                            DBHandler.UpdateEloRating(opponent_pid, '-', 5);
                            DBHandler.UpdateWinnerScoreboard(player_pid);
                            DBHandler.UpdateLoserScoreboard(opponent_pid);
                        }
                        else if (winner == 2)
                        {
                            DBHandler.UpdateEloRating(opponent_pid, '+', 3);
                            DBHandler.UpdateEloRating(player_pid, '-', 5);
                            DBHandler.UpdateWinnerScoreboard(opponent_pid);
                            DBHandler.UpdateLoserScoreboard(player_pid);
                        }
                        else
                        {
                            DBHandler.UpdateDrawScoreboard(player_pid);
                            DBHandler.UpdateDrawScoreboard(opponent_pid);
                        }


                        mutex.ReleaseMutex();
                        return new Response("200 OK", "text/plain", battleResult);
                    }
                    return new Response("200 OK", "text/plain", "HandleBattle: Logic Error");
                }
                else if (match_running == true) //Someone has taken up the offer and run the match! => Retrieve the data!
                {
                    DBHandler.DeleteFromQueue(player_pid, player_pid);
                    DateTime time = DBHandler.GetMatchDateFromID(matchID);
                    String logName = time.ToString("yyyy-dd-M--HH-mm-ss");
                    String logContent = Response.ReadLogsFromFile(logName);
                    mutex.ReleaseMutex();
                    return new Response("200 OK", "text/plain", logContent);
                }
            }

            Response badReq = Response.MakeBadRequest();
            return badReq;
        }


        public static element StrtoElem(String s)
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
