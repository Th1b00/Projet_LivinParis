using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Solution_MySQL
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string connectionString = "SERVER=localhost;DATABASE=livinParis;UID=root;PASSWORD=;";
            MySqlConnection connection = new MySqlConnection(connectionString);

            Console.WriteLine("Création de l'utilisateur ");
            Console.WriteLine("Nom d'utilisateur : ");
            string nom = Console.ReadLine();
            Console.WriteLine("Mot de passe : ");
            string mdp = Console.ReadLine();

            try
            {
                connection.Open();
                Console.WriteLine("Connexion réussie !\n");

                List<string> requetes = new List<string>
                {
                    "SELECT * FROM Cuisinier WHERE Metro_proche = 'Trocadéro';",
                    "CREATE USER '"+nom+"'@'localhost' identified by '"+mdp+"';",
                    "GRANT ALL ON livinParis.* to '"+nom+"'@'localhost';"

                };

                foreach (var requete in requetes)
                {
                    Console.WriteLine($"\nExécution de la requête :\n{requete}\n");

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = requete;

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string currentRowAsString = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string valueAsString = reader.GetValue(i).ToString();
                            currentRowAsString += valueAsString + ", ";
                        }
                        Console.WriteLine(currentRowAsString);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
            finally
            {
                connection.Close();
                Console.WriteLine("\nConnexion fermée.");
            }

            Console.ReadLine();




        }
    }
}
