using System;
using System.Data;
using Npgsql;
namespace MCTG_Bernacki
{
    class Program
    {
        static void Main(string[] args)
        {
            // Below code WORKS!!
            //var connString = "Host=localhost;Username=postgres;Password=postgres;Database=MTCG";
            //using var conn = new NpgsqlConnection(connString);
            //conn.Open();

            //using (var cmd = new NpgsqlCommand("INSERT INTO test (uid,username,password) " +
            //    "VALUES (@uid,@username,@password)", conn))
            //{
            //    cmd.Parameters.AddWithValue("uid", 2);
            //    cmd.Parameters.AddWithValue("username", "ursula");
            //    cmd.Parameters.AddWithValue("password", "user");

            //    cmd.Prepare();
            //    cmd.ExecuteNonQuery();
            //}
            // conn.Close();


            Console.WriteLine("Starting Server on port 10001");
            HTTPServer server = new HTTPServer(10001);
            server.Start();     
        }
    }
}
