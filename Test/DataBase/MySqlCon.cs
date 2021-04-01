using System;
using MySql.Data.MySqlClient;

namespace Test.DataBase
{
    public class MySqlCon
    {
        public MySqlConnection GetConntction()
        {
            string connStr = "server=localhost;user=root;database=test;port=3306;password=1q2w3e4r5t";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return conn;
        }
    }
}
