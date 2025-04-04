using System;
using MySql.Data.MySqlClient;

namespace PROJET_PSI
{
    /// <summary>
    /// Classe de gestion des opérations MySQL pour l'application Livin Paris.
    /// </summary>
    internal class MYSQL
    {
        /// <summary>
        /// Chaîne de connexion à la base de données MySQL.
        /// </summary>
        static string connectionString = "Server=localhost;Database=LivinParis_PSI;Uid=root;Pwd=Tfmi0912;";

        /// <summary>
        /// Affiche le menu principal et gère les différentes options choisies par l'utilisateur.
        /// </summary>
        public static void AfficherMenu()
        {
            bool quitter = false;
            while (!quitter)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Liv'in Paris ===");
                Console.WriteLine("1. Créer un client");
                Console.WriteLine("2. Créer un cuisinier");
                Console.WriteLine("3. Créer une commande");
                Console.WriteLine("4. Afficher les clients");
                Console.WriteLine("5. Afficher les cuisiniers");
                Console.WriteLine("6. Afficher les commandes");
                Console.WriteLine("7. Afficher les utilisateurs");
                Console.WriteLine("8. Supprimer un utilisateur");
                Console.WriteLine("9. Quitter");
                Console.Write("Choix : ");

                switch (Console.ReadLine())
                {
                    case "1": CreerClient(); break;
                    case "2": CreerCuisinier(); break;
                    case "3": CreerCommande(); break;
                    case "4": Afficher("Client"); break;
                    case "5": Afficher("Cuisinier"); break;
                    case "6": Afficher("Commande"); break;
                    case "7": Afficher("Utilisateur"); break;
                    case "8": SupprimerUtilisateur(); break;
                    case "9": quitter = true; break;
                    default: Console.WriteLine("Choix invalide."); break;
                }

                if (!quitter)
                {
                    Console.WriteLine("Appuie sur une touche pour revenir au menu...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Crée un nouveau client (particulier ou entreprise) dans la base de données.
        /// </summary>
        public static void CreerClient()
        {
            Console.WriteLine("--- Création d'un client ---");

            /// Récupération des données utilisateur
            Console.Write("Nom : "); string nom = Console.ReadLine();
            Console.Write("Prénom : "); string prenom = Console.ReadLine();
            Console.Write("Adresse : "); string adresse = Console.ReadLine();
            Console.Write("Téléphone : "); string tel = Console.ReadLine();
            Console.Write("Adresse mail : "); string mail = Console.ReadLine();
            Console.Write("Mot de passe : "); string mdp = Console.ReadLine();
            Console.Write("Métro le plus proche : "); string metro = Console.ReadLine();
            Console.Write("Type de client (Particulier/Entreprise) : "); string type = Console.ReadLine();

            string entreprise = null;
            if (type == "Entreprise")
            {
                Console.Write("Nom de l'entreprise : ");
                entreprise = Console.ReadLine();
            }

            /// Connexion à MySQL et insertion dans Utilisateur
            var con = new MySqlConnection(connectionString);
            con.Open();

            var cmdUser = new MySqlCommand(
                "INSERT INTO Utilisateur (Nom, Prenom, Adresse, Telephone, Adresse_mail, Mot_de_passe, Role, Metro_Proche) " +
                "VALUES (@Nom, @Prenom, @Adresse, @Tel, @Mail, @Mdp, 'Client', @Metro)", con);

            cmdUser.Parameters.AddWithValue("@Nom", nom);
            cmdUser.Parameters.AddWithValue("@Prenom", prenom);
            cmdUser.Parameters.AddWithValue("@Adresse", adresse);
            cmdUser.Parameters.AddWithValue("@Tel", tel);
            cmdUser.Parameters.AddWithValue("@Mail", mail);
            cmdUser.Parameters.AddWithValue("@Mdp", mdp);
            cmdUser.Parameters.AddWithValue("@Metro", metro);
            cmdUser.ExecuteNonQuery();

            int idUtilisateur = (int)cmdUser.LastInsertedId;

            /// Insertion dans la table Client
            var cmdClient = new MySqlCommand("INSERT INTO Client (Id_Client, Type_Client, Nom_Entreprise) VALUES (@Id, @Type, @Ent)", con);
            cmdClient.Parameters.AddWithValue("@Id", idUtilisateur);
            cmdClient.Parameters.AddWithValue("@Type", type);
            cmdClient.Parameters.AddWithValue("@Ent", entreprise);
            cmdClient.ExecuteNonQuery();

            Console.WriteLine("Client ajouté avec succès !");
        }

        /// <summary>
        /// Crée un nouveau cuisinier dans la base de données.
        /// </summary>
        public static void CreerCuisinier()
        {
            Console.WriteLine("--- Création d'un cuisinier ---");

            Console.Write("Nom : "); string nom = Console.ReadLine();
            Console.Write("Prénom : "); string prenom = Console.ReadLine();
            Console.Write("Adresse : "); string adresse = Console.ReadLine();
            Console.Write("Téléphone : "); string tel = Console.ReadLine();
            Console.Write("Adresse mail : "); string mail = Console.ReadLine();
            Console.Write("Mot de passe : "); string mdp = Console.ReadLine();
            Console.Write("Métro le plus proche : "); string metro = Console.ReadLine();
            Console.Write("Spécialité : "); string specialite = Console.ReadLine();

            var con = new MySqlConnection(connectionString);
            con.Open();

            /// Insertion dans la table Utilisateur
            var cmdUser = new MySqlCommand(
                "INSERT INTO Utilisateur (Nom, Prenom, Adresse, Telephone, Adresse_mail, Mot_de_passe, Role, Metro_Proche) " +
                "VALUES (@Nom, @Prenom, @Adresse, @Tel, @Mail, @Mdp, 'Cuisinier', @Metro)", con);

            cmdUser.Parameters.AddWithValue("@Nom", nom);
            cmdUser.Parameters.AddWithValue("@Prenom", prenom);
            cmdUser.Parameters.AddWithValue("@Adresse", adresse);
            cmdUser.Parameters.AddWithValue("@Tel", tel);
            cmdUser.Parameters.AddWithValue("@Mail", mail);
            cmdUser.Parameters.AddWithValue("@Mdp", mdp);
            cmdUser.Parameters.AddWithValue("@Metro", metro);
            cmdUser.ExecuteNonQuery();

            int idUtilisateur = (int)cmdUser.LastInsertedId;

            /// Insertion dans la table Cuisinier
            var cmdCuisinier = new MySqlCommand("INSERT INTO Cuisinier (Id_Cuisinier, Specialite) VALUES (@Id, @Spec)", con);
            cmdCuisinier.Parameters.AddWithValue("@Id", idUtilisateur);
            cmdCuisinier.Parameters.AddWithValue("@Spec", specialite);
            cmdCuisinier.ExecuteNonQuery();

            Console.WriteLine("Cuisinier ajouté avec succès !");
        }

        /// <summary>
        /// Crée une nouvelle commande avec plusieurs repas et une livraison par ligne de commande.
        /// </summary>
        public static void CreerCommande()
        {
            Console.WriteLine("--- Création d'une commande ---");

            Console.Write("ID Client : "); int idClient = int.Parse(Console.ReadLine());
            Console.Write("ID Cuisinier : "); int idCuisinier = int.Parse(Console.ReadLine());
            DateTime dateCommande = DateTime.Now;

            var con = new MySqlConnection(connectionString);
            con.Open();

            /// Paiement par défaut : Carte
            var cmdPaiement = new MySqlCommand("INSERT INTO Paiement (Moyen_Paiement) VALUES ('Carte')", con);
            cmdPaiement.ExecuteNonQuery();
            int idPaiement = (int)cmdPaiement.LastInsertedId;

            /// Insertion de la commande
            var cmdCommande = new MySqlCommand("INSERT INTO Commande (Id_Client, Id_Cuisinier, Date_Commande, Prix_Total, Id_Paiement) " +
                                               "VALUES (@Client, @Cuisinier, @Date, 0, @Paiement)", con);
            cmdCommande.Parameters.AddWithValue("@Client", idClient);
            cmdCommande.Parameters.AddWithValue("@Cuisinier", idCuisinier);
            cmdCommande.Parameters.AddWithValue("@Date", dateCommande);
            cmdCommande.Parameters.AddWithValue("@Paiement", idPaiement);
            cmdCommande.ExecuteNonQuery();
            int idCommande = (int)cmdCommande.LastInsertedId;

            decimal prixTotal = 0;

            /// Ajout de repas à la commande (max 3)
            for (int i = 0; i < 3; i++)
            {
                Console.Write("ID Repas (0 pour arrêter) : ");
                int idRepas = int.Parse(Console.ReadLine());
                if (idRepas == 0) break;

                Console.Write("Quantité : ");
                int quantite = int.Parse(Console.ReadLine());

                /// Récupération du prix unitaire
                var cmdPrix = new MySqlCommand("SELECT Prix FROM Repas WHERE Id_Repas = @Id", con);
                cmdPrix.Parameters.AddWithValue("@Id", idRepas);
                decimal prixUnitaire = Convert.ToDecimal(cmdPrix.ExecuteScalar());

                /// Insertion dans LigneCommande
                var cmdLigne = new MySqlCommand("INSERT INTO LigneCommande (Id_Commande, Id_Repas, Quantite, Prix_Unitaire) " +
                                                "VALUES (@Cmd, @Repas, @Qte, @Prix)", con);
                cmdLigne.Parameters.AddWithValue("@Cmd", idCommande);
                cmdLigne.Parameters.AddWithValue("@Repas", idRepas);
                cmdLigne.Parameters.AddWithValue("@Qte", quantite);
                cmdLigne.Parameters.AddWithValue("@Prix", prixUnitaire);
                cmdLigne.ExecuteNonQuery();
                int idLigne = (int)cmdLigne.LastInsertedId;

                prixTotal += prixUnitaire * quantite;

                /// Livraison pour chaque ligne
                Console.Write("Adresse de livraison : "); string adr = Console.ReadLine();
                Console.Write("Date livraison (yyyy-mm-dd) : "); string date = Console.ReadLine();
                Console.Write("Heure livraison (HH:mm:ss) : "); string heure = Console.ReadLine();

                var cmdLiv = new MySqlCommand("INSERT INTO Livraison (Id_LigneCommande, Adresse_Livraison, Date_Livraison, Heure_Livraison) " +
                                              "VALUES (@Id, @Adr, @Date, @Heure)", con);
                cmdLiv.Parameters.AddWithValue("@Id", idLigne);
                cmdLiv.Parameters.AddWithValue("@Adr", adr);
                cmdLiv.Parameters.AddWithValue("@Date", date);
                cmdLiv.Parameters.AddWithValue("@Heure", heure);
                cmdLiv.ExecuteNonQuery();
            }

            /// Mise à jour du prix total
            var updatePrix = new MySqlCommand("UPDATE Commande SET Prix_Total = @Total WHERE Id_Commande = @Id", con);
            updatePrix.Parameters.AddWithValue("@Total", prixTotal);
            updatePrix.Parameters.AddWithValue("@Id", idCommande);
            updatePrix.ExecuteNonQuery();

            Console.WriteLine("Commande enregistrée avec succès.");
        }

        /// <summary>
        /// Supprime un utilisateur de la base de données à partir de son ID.
        /// </summary>
        public static void SupprimerUtilisateur()
        {
            Console.WriteLine("--- Suppression d'un utilisateur ---");
            Console.Write("ID de l'utilisateur à supprimer : ");
            int id = int.Parse(Console.ReadLine());

            var con = new MySqlConnection(connectionString);
            con.Open();

            var cmd = new MySqlCommand("DELETE FROM Utilisateur WHERE Id_Utilisateur = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            int lignes = cmd.ExecuteNonQuery();

            if (lignes > 0)
                Console.WriteLine("Utilisateur supprimé avec succès.");
            else
                Console.WriteLine("Aucun utilisateur trouvé avec cet ID.");
        }

        /// <summary>
        /// Affiche le contenu de la table spécifiée.
        /// </summary>
        /// <param name="table">Nom de la table à afficher.</param>
        public static void Afficher(string table)
        {
            var con = new MySqlConnection(connectionString);
            con.Open();

            var cmd = new MySqlCommand("SELECT * FROM " + table, con);
            var reader = cmd.ExecuteReader();

            Console.WriteLine("--- Liste des " + table + " ---");

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    Console.Write(reader.GetValue(i) + " ");
                Console.WriteLine();
            }
        }
    }
}
