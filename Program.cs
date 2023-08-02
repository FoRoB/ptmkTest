using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace ptmkTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=testdb;Trusted_Connection=True;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                switch (args[0])
                {
                    case "1":
                        try
                        {
                            new SqlCommand(@"CREATE TABLE People(
                                            Id INT PRIMARY KEY IDENTITY,
                                            FIO VARCHAR(100) NOT NULL,
                                            DateOfBirth DATE NOT NULL,
                                            Gender VARCHAR(10) NOT NULL)", connection)
                            .ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            Console.WriteLine("Already exists");
                        }
                        break;
                    case "2":
                        try
                        {
                            new SqlCommand(@$"INSERT INTO People (FIO, DateOfBirth, Gender)
                                              VALUES ('{args[1]}','{args[2]}','{args[3]}')", connection)
                            .ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Invalid arguments (date format should be mm.dd.yyyy)");
                        }
                        break;
                    case "3":
                        {
                            var reader = new SqlCommand(@"SELECT FIO, DateOfBirth, max(Gender), DATEDIFF(YEAR, DateOfBirth, GETDATE()) 
										                - CASE WHEN RIGHT(CONVERT(VARCHAR(8), GETDATE(), 112), 4) 
                                                        < RIGHT(CONVERT(VARCHAR(8), DateOfBirth, 112), 4) 
                                                        THEN 1 ELSE 0 END as Age 
                                                    FROM People
                                                    GROUP BY FIO, DateOfBirth
                                                    ORDER BY FIO", connection)
                            .ExecuteReader();
                            while (reader.Read())
                            {
                                Console.WriteLine($"FIO={reader[0]}, DateOfBirth={reader[1]}, Gender={reader[2]}, Age={reader[3]}");
                            }
                        }
                        break;
                    case "4":
                        {
                            var genders = new List<string> { "male", "female" };
                            var firstChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                            var rnd = new Random();
                            for (int i = 0; i < 1000000; i++)
                            {
                                new SqlCommand(@$"INSERT INTO People (FIO, DateOfBirth, Gender)
                                            VALUES ('{firstChar[rnd.Next(26)]}','01.01.2000','{genders[rnd.Next(2)]}')", connection)
                                .ExecuteNonQuery();
                            }
                            for (int i = 0; i < 100; i++)
                            {
                                new SqlCommand(@$"INSERT INTO People (FIO, DateOfBirth, Gender)
                                            VALUES ('F','01.01.2000','male')", connection)
                                .ExecuteNonQuery();
                            }
                        }
                        break;
                    case "5":
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            var reader = new SqlCommand(@"SELECT FIO, DateOfBirth, Gender
                                                    FROM People
                                                    WHERE FIO = 'F' AND Gender = 'male'", connection)
                            .ExecuteReader();
                            sw.Stop();
                            int i = 0;
                            while (reader.Read())
                            {
                                Console.WriteLine($"FIO={reader[0]}, DateOfBirth={reader[1]}, Gender={reader[2]}");
                                i++;
                            }
                            Console.WriteLine($"Rows count: {i}\n" + String.Format("Time spent: {0:00}:{1:00}:{2:00}.{3:00}",
                                sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}