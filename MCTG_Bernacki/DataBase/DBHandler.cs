using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace MCTG_Bernacki
{
    public class DBHandler
    {
        public DBHandler() { }

        public static bool LogInUser(String _username, String _password)
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

        public static bool CreateUser(String _username, String _password)
        {
            int uid = DBHandler.GetHighestUserId();
            if (uid == -1)
                uid = 1;
            else
                uid += 1;
            int startCoins = 20;
            int startElo = 100;
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Users(uid,username,password,admin) Values(@uid,@username,@password,0)", conn))
                {
                    cmd.Parameters.AddWithValue("uid", uid);
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

                    while (reader.Read())
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

        public static bool CreateCard(String _name, String _type, String _element, int _damage, int _price)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            int cardID = DBHandler.GetHighestCardId();
            if (cardID == -1)
                cardID = 1;
            else
                cardID += 1;

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Card(card_id,name,type,element,damage,price) Values(@cardID,@name,@type,@element,@damage,@price)", conn))
                {
                    cmd.Parameters.AddWithValue("cardID", cardID);
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

        public static bool CreatePackage(List<CardID> cardIDs, int _price = 5)
        {
            int newPackageID = DBHandler.GetHighestPackageId();
            if (newPackageID == -1)
                newPackageID = 1;
            else
                newPackageID += 1;

            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Insert Into Package(package_id,price) Values(@packID,@price)", conn))
                {
                    cmd.Parameters.AddWithValue("packID", newPackageID);
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

        public static bool AddToken(String tokenName, String user)
        {   
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

        public static String GetUsernameFromToken(String token)
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

        public static String GetUsernameFromUID(int uid)
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

        public static void PlaceBattleRequest(int _pid)
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

        public static int FindOpponentForBattle(int player_pid)
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

                    if (!reader.HasRows)
                    {
                        return -1; // No opponent found!
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
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

        public static int GetUIDFromToken(String token)
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

        public static List<int> GetStack(int uid)
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

        public static bool BuyPackage(int _pid, int package_id)
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
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                CardIDs.Add((int)reader[i]);
                            }
                        }
                    }
                    conn.Close();
                    conn.Open();

                    foreach (int id in CardIDs)
                    {
                        if (DBHandler.AlreadyOwnsCard(_pid, id) == false)
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

        public static bool CheckIfCardsInStack(int _pid, List<CardID> cardIDs)
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

                        if (!reader.HasRows)
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

        public static bool ConfigureDeck(int _pid, List<CardID> deck)
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

        public static CardInfo GetCardInfoFromID(int _cardID)
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

                while (reader.Read())
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

        public static List<int> GetDeck(int _pid)
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

        public static Card GetCardFromCardInfo(CardInfo ci)
        {
            if (ci.Type.ToUpper() == "GOBLIN")
                return new Goblin(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            else if (ci.Type.ToUpper() == "ORC")
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
                if (Response.StrtoElem(ci.Element) == element.NORMAL)
                    return new NormalSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else if (Response.StrtoElem(ci.Element) == element.WATER)
                    return new WaterSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else if (Response.StrtoElem(ci.Element) == element.FIRE)
                    return new FireSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
                else
                    return new NormalSpell(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
            }
            else
                return new Goblin(ci.Name, ci.Price, Response.StrtoElem(ci.Element), ci.Damage);
        }

        public static void DeleteFromQueue(int pl1ID, int pl2ID)
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

        public static bool UserExists(String _username)
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

        public static int GetLeaderboardPos(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            List<int> eloScores = new List<int>();
            int playerElo = DBHandler.GetEloRating(_pid);

            try
            {
                using (var cmd = new NpgsqlCommand("Select elo From Scoreboard", conn))
                {
                    cmd.Prepare();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            eloScores.Add((int)reader[i]);
                        }
                    }
                }

                eloScores.Sort(); // Sorts ascending, i.e. : 1, 4, 6, 7...
                eloScores.Reverse(); // Now its descending, i.e. : 7, 6, 4, 1...
                for (int i = 0; i < eloScores.Count; i++)
                {
                    if (eloScores[i] == playerElo)
                        return i + 1;
                }
                return -1; // Could not determine Score
            }
            catch
            {
                throw;
            }
        }

        public static void CreateMatch(int matchID, int playerID, int opponentID, DateTime matchTime)
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

        public static int GetWins(int _pid)
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

        public static int GetDraws(int _pid)
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

        public static int GetLosses(int _pid)
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

        public static int GetWinstreak(int _pid)
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

        public static void UpdateWinnerScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = DBHandler.GetEloRating(_pid); // There is another Function for that!           
            int wins = DBHandler.GetWins(_pid);
            wins += 1;
            int winstreak = DBHandler.GetWinstreak(_pid);
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

        public static void UpdateLoserScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = DBHandler.GetEloRating(_pid);
            int losses = DBHandler.GetLosses(_pid);
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

        public static void UpdateDrawScoreboard(int _pid)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = DBHandler.GetEloRating(_pid);
            int draws = DBHandler.GetDraws(_pid);
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

        public static Dictionary<String, int> GetPlayerStats(int _pid)
        {
            Dictionary<String, int> Stats = new Dictionary<String, int>();

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

        public static bool MatchesExist()
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

        public static int GetMatchID()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            if (!DBHandler.MatchesExist())
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

        public static int GetEloRating(int _pid)
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


        public static void UpdateEloRating(int _pid, char c, int value)
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            int elo = DBHandler.GetEloRating(_pid); // Get Current Elo

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

        public static bool AlreadyOwnsCard(int _pid, int cardID)
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

        public static int ChooseRandomPackage()
        {
            List<int> unsoldIDs = DBHandler.GetUnsoldPackages();
            if (unsoldIDs.Count <= 0)
            {
                return -1;
            }

            Random rnd = new Random();
            int rndElement = rnd.Next(1, unsoldIDs.Count);
            int chosenID = unsoldIDs[rndElement - 1];
            return chosenID;
        }

        public static List<int> GetUnsoldPackages()
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

        public static bool PackageStillAvailable(int packageID)
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



        public static int GetLowestPackageId()
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

        public static int GetHighestPackageId()
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

        public static int GetHighestUnsoldPackageId()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();
            bool sold = false;

            try
            {
                using (var cmd = new NpgsqlCommand("Select max(package_id) As result From package Where sold = @sold", conn))
                {
                    cmd.Parameters.AddWithValue("sold", sold);
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

        public static int GetHighestCardId()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select max(card_id) As result From card", conn))
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
                return -1; // In case no cards exist!!
            }
        }
        public static int GetHighestUserId()
        {
            using var conn = new NpgsqlConnection(HTTPServer.CONN_STRING);
            conn.Open();

            try
            {
                using (var cmd = new NpgsqlCommand("Select max(uid) As result From users", conn))
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
                return -1; // In case no cards exist!!
            }
        }

        public static bool HasMatched(int matchID)
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

                    while (reader.HasRows)
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

        public static List<Card> PrepareDeck(int pid)
        {
            // -> Get players cardID while looping database function, that looks for them in CardInDeck
            List<int> CardIDs = DBHandler.GetDeck(pid);

            // -> Get card Information by looping database function, that gets card data for every cardID
            List<CardInfo> CardInfo = new List<CardInfo>();

            for (int i = 0; i < CardIDs.Count; i++)
            {
                CardInfo.Add(DBHandler.GetCardInfoFromID(CardIDs[i]));
            }

            List<Card> Deck = new List<Card>();

            for (int i = 0; i < CardInfo.Count; i++)
            {
                Deck.Add(DBHandler.GetCardFromCardInfo(CardInfo[i]));
            }
            return Deck;
        }

        public static List<Card> PrepareStack(int pid)
        {
            // -> Get players cardID while looping database function, that looks for them in CardInDeck
            List<int> CardIDs = DBHandler.GetStack(pid);

            // -> Get card Information by looping database function, that gets card data for every cardID
            List<CardInfo> CardInfo = new List<CardInfo>();

            for (int i = 0; i < CardIDs.Count; i++)
            {
                CardInfo.Add(DBHandler.GetCardInfoFromID(CardIDs[i]));
            }

            List<Card> Deck = new List<Card>();

            for (int i = 0; i < CardInfo.Count; i++)
            {
                Deck.Add(DBHandler.GetCardFromCardInfo(CardInfo[i]));
            }
            return Deck;
        }

        public static DateTime GetMatchDateFromID(int _matchID)
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

        public static void UpdateMatchHistory(int playerID, int opponentID, int winner, int rounds)
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

        public static bool UserIsAdmin(int _uid)
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

    }
}
