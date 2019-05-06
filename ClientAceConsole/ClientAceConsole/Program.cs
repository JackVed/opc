using System;
using MySql.Data.MySqlClient;

namespace ClientAceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = "Data Source=localhost;Database=mydb;userid=root;Password=4dmin95";
            MySqlConnection conn = new MySqlConnection(connString);

            Console.WriteLine("Inserisci il giorno");
            int day = int.Parse(Console.ReadLine());
            Console.WriteLine("Inserisci il mese");
            int month = Convert.ToInt32(Console.ReadLine());
            
            conn.Open();
            string select = "SELECT * FROM plantStatus WHERE MONTH(DATE)="+month+" AND DAY(DATE)="+day+"";
            var comm = new MySqlCommand(select, conn);
            var reader = comm.ExecuteReader();

            while (reader.Read())
            {
                var id = (int)reader["id"];                
                var status = (int)reader["status"];
                DateTime date = (DateTime)reader["date"];

                Console.WriteLine($"{id} - {status} - {date}");
            }

            conn.Close();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
