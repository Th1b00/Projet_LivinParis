using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace PROJET_PSI
{
    /// <summary>
    /// Classe de gestion des opérations MySQL pour l'application Livin Paris.
    /// </summary>
    public class MYSQL
    {
        /// <summary>
        /// Chaîne de connexion à la base de données MySQL.
        /// </summary>
        static string connectionString = "Server=localhost;Database=livinparis_psi;Uid=root;Pwd=Tfmi0912;";

        /// <summary>
        /// Affiche le menu principal et gère les différentes options choisies par l'utilisateur.
        /// </summary>
        public static void AfficherMenuPrincipal(Graphe<int> graphe)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("\r\n  .---.    .-./`) ,---.  ,---..-./`) ,---.   .--.          _ _          .-------.    ____    .-------.   .-./`)    .-'''-.  \r\n  | ,_|    \\ .-.')|   /  |   |\\ .-.')|    \\  |  |         ( ' )         \\  _(`)_ \\ .'  __ `. |  _ _   \\  \\ .-.')  / _     \\ \r\n,-./  )    / `-' \\|  |   |  .'/ `-' \\|  ,  \\ |  |        (_{;}_)        | (_ o._)|/   '  \\  \\| ( ' )  |  / `-' \\ (`' )/`--' \r\n\\  '_ '`)   `-'`\"`|  | _ |  |  `-'`\"`|  |\\_ \\|  |         (_,_)         |  (_,_) /|___|  /  ||(_ o _) /   `-'`\"`(_ o _).    \r\n > (_)  )   .---. |  _( )_  |  .---. |  _( )_\\  |                       |   '-.-'    _.-`   || (_,_).' __ .---.  (_,_). '.  \r\n(  .  .-'   |   | \\ (_ o._) /  |   | | (_ o _)  |                       |   |     .'   _    ||  |\\ \\  |  ||   | .---.  \\  : \r\n `-'`-'|___ |   |  \\ (_,_) /   |   | |  (_,_)\\  |                       |   |     |  _( )_  ||  | \\ `'   /|   | \\    `-'  | \r\n  |        \\|   |   \\     /    |   | |  |    |  |                       /   )     \\ (_ o _) /|  |  \\    / |   |  \\       /  \r\n  `--------`'---'    `---`     '---' '--'    '--'                       `---'      '.(_,_).' ''-'   `'-'  '---'   `-...-'   \r\n                                                                                                                            \r\n");
            Console.ResetColor();

            Console.WriteLine("=== Menu Principal ===");
            Console.WriteLine("1. Espace Admin");
            Console.WriteLine("2. Espace Utilisateur");
            Console.WriteLine("3. Quitter");
            Console.Write("Choix : ");

            switch (Console.ReadLine())
            {
                case "1":
                    AfficherMenuAdmin(graphe);
                    break;
                case "2":
                    AfficherMenuUtilisateur(graphe);
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Choix invalide.");
                    Console.ReadKey();
                    break;
            }

            AfficherMenuPrincipal(graphe);
        }

        /// <summary>
        /// Affiche le menu administrateur après vérification des identifiants.
        /// </summary>
        /// <param name="graphe">Le graphe utilisé pour afficher les trajets des commandes.</param>
        public static void AfficherMenuAdmin(Graphe<int> graphe)
        {
            Console.Clear();
            Console.WriteLine("=== Connexion Admin requise ===");

            Console.Write("Nom d'utilisateur : ");
            string login = Console.ReadLine();
            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine();

            if (login != "admin" || mdp != "1234")
            {
                Console.WriteLine("Identifiants incorrects. Accès refusé.");
                Console.WriteLine("Appuie sur une touche pour revenir au menu principal...");
                Console.ReadKey();
                return;
            }

            bool retour = false;
            while (retour != true)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Admin ===");
                Console.WriteLine("1. Afficher les utilisateurs");
                Console.WriteLine("2. Supprimer un utilisateur");
                Console.WriteLine("3. Afficher les commandes");
                Console.WriteLine("4. Voir le graphe des commandes client → cuisinier");
                Console.WriteLine("5. Retour");
                Console.Write("Choix : ");

                string choix = Console.ReadLine();
                if (choix == "1")
                {
                    Afficher("Utilisateur");
                }
                else if (choix == "2")
                {
                    SupprimerUtilisateur();
                }
                else if (choix == "3")
                {
                    Afficher("Commande");
                }
                else if (choix == "4")
                {
                    var commandes = RecupererCommandes();
                    var roles = RecupererRoles();
                    var noms = RecupererStations();
                    graphe.AfficherGrapheCommandesAvecRoles(commandes, roles, noms);
                }
                else if (choix == "5")
                {
                    retour = true;
                }
                else
                {
                    Console.WriteLine("Choix invalide.");
                }

                if (retour != true)
                {
                    Console.WriteLine("Appuie sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Affiche le menu utilisateur pour créer un compte ou se connecter.
        /// </summary>
        /// <param name="graphe">Le graphe utilisé pour les fonctionnalités après connexion.</param>
        public static void AfficherMenuUtilisateur(Graphe<int> graphe)
        {
            bool retour = false;
            while (retour != true)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Utilisateur ===");
                Console.WriteLine("1. Créer un compte");
                Console.WriteLine("2. Se connecter");
                Console.WriteLine("3. Retour");
                Console.Write("Choix : ");

                string choix = Console.ReadLine();
                if (choix == "1")
                {
                    Console.Write("Souhaitez-vous être Client (C) ou Cuisinier (U) ? [C/U] : ");
                    string role = Console.ReadLine().Trim().ToUpper();
                    if (role == "C")
                    {
                        CreerClient();
                    }
                    else
                    {
                        if (role == "U")
                        {
                            CreerCuisinier();
                        }
                        else
                        {
                            Console.WriteLine("Choix invalide.");
                        }
                    }
                }
                else if (choix == "2")
                {
                    var resultat = SeConnecter();
                    int idUtilisateur = resultat.id;
                    string roleUtilisateur = resultat.role;

                    if (idUtilisateur != -1)
                    {
                        if (roleUtilisateur == "Client")
                        {
                            PasserCommande(idUtilisateur, graphe);
                        }
                        else
                        {
                            if (roleUtilisateur == "Cuisinier")
                            {
                                AfficherMenuCuisinier(idUtilisateur, graphe);
                            }
                            else
                            {
                                Console.WriteLine("Rôle non reconnu.");
                            }
                        }
                    }
                }
                else if (choix == "3")
                {
                    retour = true;
                }
                else
                {
                    Console.WriteLine("Choix invalide.");
                }

                if (retour != true)
                {
                    Console.WriteLine("Appuie sur une touche pour continuer...");
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

            Console.Write("Nom : ");
            string nom = Console.ReadLine();

            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();

            Console.Write("Adresse : ");
            string adresse = Console.ReadLine();

            Console.Write("Téléphone : ");
            string tel = Console.ReadLine();

            Console.Write("Adresse mail : ");
            string mail = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine();

            Console.Write("Métro le plus proche : ");
            string metro = Console.ReadLine();

            Console.Write("Type de client (Particulier/Entreprise) : ");
            string type = Console.ReadLine();

            string entreprise = null;
            if (type == "Entreprise")
            {
                Console.Write("Nom de l'entreprise : ");
                entreprise = Console.ReadLine();
            }

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

            var cmdClient = new MySqlCommand(
                "INSERT INTO Client (Id_Client, Type_Client, Nom_Entreprise) VALUES (@Id, @Type, @Ent)", con);
            cmdClient.Parameters.AddWithValue("@Id", idUtilisateur);
            cmdClient.Parameters.AddWithValue("@Type", type);
            cmdClient.Parameters.AddWithValue("@Ent", entreprise);
            cmdClient.ExecuteNonQuery();

            Console.WriteLine("Client ajouté avec succès !");
            con.Close();
        }

        /// <summary>
        /// Crée un nouveau cuisinier dans la base de données.
        /// </summary>
        public static void CreerCuisinier()
        {
            Console.WriteLine("--- Création d'un cuisinier ---");

            Console.Write("Nom : ");
            string nom = Console.ReadLine();

            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();

            Console.Write("Adresse : ");
            string adresse = Console.ReadLine();

            Console.Write("Téléphone : ");
            string tel = Console.ReadLine();

            Console.Write("Adresse mail : ");
            string mail = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine();

            Console.Write("Métro le plus proche : ");
            string metro = Console.ReadLine();

            Console.Write("Spécialité : ");
            string specialite = Console.ReadLine();

            var con = new MySqlConnection(connectionString);
            con.Open();

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

            var cmdCuisinier = new MySqlCommand(
                "INSERT INTO Cuisinier (Id_Cuisinier, Specialite) VALUES (@Id, @Spec)", con);
            cmdCuisinier.Parameters.AddWithValue("@Id", idUtilisateur);
            cmdCuisinier.Parameters.AddWithValue("@Spec", specialite);
            cmdCuisinier.ExecuteNonQuery();

            Console.WriteLine("Cuisinier ajouté avec succès !");
            con.Close();
        }

        /// <summary>
        /// Permet à un utilisateur de se connecter en vérifiant son adresse mail et son mot de passe.
        /// </summary>
        /// <returns>Un tuple contenant l'ID de l'utilisateur et son rôle.</returns>
        public static (int id, string role) SeConnecter()
        {
            Console.Clear();
            Console.WriteLine("=== Connexion ===");

            Console.Write("Adresse mail : ");
            string email = Console.ReadLine();

            Console.Write("Mot de passe : ");
            string motDePasse = Console.ReadLine();

            using (var connexion = new MySqlConnection(connectionString))
            {
                connexion.Open();

                string requete = "SELECT Id_Utilisateur, Role FROM Utilisateur WHERE Adresse_mail = @Email AND Mot_de_passe = @MotDePasse";

                using (var commande = new MySqlCommand(requete, connexion))
                {
                    commande.Parameters.AddWithValue("@Email", email);
                    commande.Parameters.AddWithValue("@MotDePasse", motDePasse);

                    using (var reader = commande.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string role = reader.GetString(1);
                            Console.WriteLine("Connexion réussie !");
                            return (id, role);
                        }
                        else
                        {
                            Console.WriteLine("Identifiants incorrects.");
                            return (-1, null);
                        }
                    }
                }
            }
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
            {
                Console.WriteLine("Utilisateur supprimé avec succès.");
            }
            else
            {
                Console.WriteLine("Aucun utilisateur trouvé avec cet ID.");
            }

            con.Close();
        }

        /// <summary>
        /// Affiche le contenu de la table spécifiée.
        /// </summary>
        /// <param name="table">Nom de la table à afficher.</param>
        public static void Afficher(string table)
        {
            var con = new MySqlConnection(connectionString);
            con.Open();

            if (table == "Commande")
            {
                var cmd = new MySqlCommand(
                    "SELECT c.Id_Commande, c.Id_Client, c.Id_Cuisinier, c.Prix_Total, p.Moyen_Paiement " +
                    "FROM Commande c JOIN Paiement p ON c.Id_Paiement = p.Id_Paiement", con);

                var reader = cmd.ExecuteReader();

                Console.WriteLine("--- Liste des Commandes ---");
                Console.WriteLine();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int clientId = reader.GetInt32(1);
                    int cuisId = reader.GetInt32(2);
                    decimal total = reader.GetDecimal(3);
                    string paiement = reader.GetString(4);

                    Console.WriteLine("Commande #" + id);
                    Console.WriteLine("    • Client ID     : " + clientId);
                    Console.WriteLine("    • Cuisinier ID  : " + cuisId);
                    Console.WriteLine("    • Total         : " + total.ToString("F2") + " euros");
                    Console.WriteLine("    • Paiement      : " + paiement);
                    Console.WriteLine();
                }

                reader.Close();
            }
            else
            {
                var cmd = new MySqlCommand("SELECT * FROM " + table, con);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("--- Liste des " + table + " ---");

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i = i + 1)
                    {
                        Console.Write(reader.GetValue(i) + " ");
                    }
                    Console.WriteLine();
                }

                reader.Close();
            }

            con.Close();
        }

        /// <summary>
        /// Permet à un client de passer une commande auprès d'un cuisinier en sélectionnant des repas.
        /// </summary>
        /// <param name="idUtilisateur">ID de l'utilisateur client connecté.</param>
        /// <param name="graphe">Graphe utilisé pour calculer le trajet de livraison.</param>
        public static void PasserCommande(int idUtilisateur, Graphe<int> graphe)
        {
            var connexion = new MySqlConnection(connectionString);
            connexion.Open();

            int idClient = -1;

            var cmd = new MySqlCommand("SELECT Id_Client FROM Client WHERE Id_Client = @Id", connexion);
            cmd.Parameters.AddWithValue("@Id", idUtilisateur);
            var result = cmd.ExecuteScalar();

            if (result == null)
            {
                Console.WriteLine("Vous n'êtes pas un client.");
                connexion.Close();
                return;
            }

            idClient = Convert.ToInt32(result);

            var cuisiniers = new List<int>();
            Console.WriteLine();
            Console.WriteLine("=== Cuisiniers disponibles ===");

            cmd = new MySqlCommand("SELECT Id_Utilisateur, Nom, Prenom, Metro_Proche FROM Utilisateur WHERE Role = 'Cuisinier'", connexion);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string nom = reader.GetString(1);
                string prenom = reader.GetString(2);
                string station = "";

                if (reader.IsDBNull(3) == false)
                {
                    station = reader.GetString(3);
                }
                else
                {
                    station = "Non spécifiée";
                }

                cuisiniers.Add(id);
                Console.WriteLine(id + " - " + prenom + " " + nom + " (station : " + station + ")");
            }

            reader.Close();

            Console.Write("ID du cuisinier : ");
            int idCuisinier = int.Parse(Console.ReadLine());

            if (cuisiniers.Contains(idCuisinier) == false)
            {
                Console.WriteLine("ID invalide.");
                connexion.Close();
                return;
            }

            var repas = new Dictionary<int, (string nom, decimal prix, string type)>();
            cmd = new MySqlCommand(
                "SELECT r.Id_Repas, r.Nom, r.Prix, r.Type " +
                "FROM cuisinier_repas cr " +
                "JOIN repas r ON cr.Id_Repas = r.Id_Repas " +
                "WHERE cr.Id_Cuisinier = @IdC", connexion);
            cmd.Parameters.AddWithValue("@IdC", idCuisinier);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string nom = reader.GetString(1);
                decimal prix = reader.GetDecimal(2);
                string type = reader.GetString(3).Trim();
                repas[id] = (nom, prix, type);
            }

            reader.Close();

            Console.WriteLine();
            Console.WriteLine("=== Repas proposés ===");

            var types = new List<string>();
            foreach (var element in repas.Values)
            {
                if (types.Contains(element.type) == false)
                {
                    types.Add(element.type);
                }
            }

            foreach (var type in types)
            {
                Console.WriteLine();
                Console.WriteLine("--- " + type + " ---");

                foreach (var paire in repas)
                {
                    if (paire.Value.type == type)
                    {
                        Console.WriteLine(paire.Key + " - " + paire.Value.nom + " (" + paire.Value.prix + " euros)");
                    }
                }
            }

            var commandes = new List<(int idRepas, int quantite, decimal prix)>();
            bool fin = false;

            while (fin == false)
            {
                Console.Write("ID du repas (0 pour terminer) : ");
                int id = int.Parse(Console.ReadLine());

                if (id == 0)
                {
                    fin = true;
                }
                else if (repas.ContainsKey(id) == false)
                {
                    Console.WriteLine("Repas invalide.");
                }
                else
                {
                    Console.Write("Quantité : ");
                    int qte = int.Parse(Console.ReadLine());
                    commandes.Add((id, qte, repas[id].prix));
                }
            }

            if (commandes.Count == 0)
            {
                Console.WriteLine("Commande vide.");
                connexion.Close();
                return;
            }

            decimal total = 0;
            foreach (var c in commandes)
            {
                total = total + (c.quantite * c.prix);
            }

            Console.WriteLine("Total : " + total + " euros");

            Console.Write("Mode de paiement (1 = Carte, 2 = Espèce) : ");
            string mode = Console.ReadLine().Trim();
            string moyenPaiement = "";

            if (mode == "2")
            {
                moyenPaiement = "Espèce";
            }
            else
            {
                moyenPaiement = "Carte";
            }

            cmd = new MySqlCommand("INSERT INTO Paiement (Moyen_Paiement) VALUES (@Moyen)", connexion);
            cmd.Parameters.AddWithValue("@Moyen", moyenPaiement);
            cmd.ExecuteNonQuery();
            int idPaiement = (int)cmd.LastInsertedId;

            cmd = new MySqlCommand(
                "INSERT INTO Commande (Id_Client, Id_Cuisinier, Prix_Total, Id_Paiement) " +
                "VALUES (@Client, @Cuisinier, @Total, @Paiement)", connexion);
            cmd.Parameters.AddWithValue("@Client", idClient);
            cmd.Parameters.AddWithValue("@Cuisinier", idCuisinier);
            cmd.Parameters.AddWithValue("@Total", total);
            cmd.Parameters.AddWithValue("@Paiement", idPaiement);
            cmd.ExecuteNonQuery();
            int idCommande = (int)cmd.LastInsertedId;

            foreach (var c in commandes)
            {
                cmd = new MySqlCommand(
                    "INSERT INTO LigneCommande (Id_Commande, Id_Repas, Quantite, Prix_Unitaire) " +
                    "VALUES (@Commande, @Repas, @Qte, @Prix)", connexion);
                cmd.Parameters.AddWithValue("@Commande", idCommande);
                cmd.Parameters.AddWithValue("@Repas", c.idRepas);
                cmd.Parameters.AddWithValue("@Qte", c.quantite);
                cmd.Parameters.AddWithValue("@Prix", c.prix);
                cmd.ExecuteNonQuery();
            }

            string metroClient = RecupererStationParId(idClient);
            string metroCuisinier = RecupererStationParId(idCuisinier);

            int idStationClient = graphe.GetIdSommetParNom(metroClient);
            int idStationCuisinier = graphe.GetIdSommetParNom(metroCuisinier);

            if (idStationClient == -1 || idStationCuisinier == -1)
            {
                Console.WriteLine("Erreur : station de métro non trouvée dans le graphe.");
                connexion.Close();
                return;
            }

            double tempsLivraison = graphe.Dijkstra(idStationCuisinier, idStationClient);
            Console.WriteLine();
            Console.WriteLine("Livraison estimée en " + tempsLivraison + " minutes.");

            Console.WriteLine("Commande enregistrée !");
            connexion.Close();
        }

        /// <summary>
        /// Récupère le nom de la station de métro associée à un utilisateur donné.
        /// </summary>
        /// <param name="id">Identifiant de l'utilisateur (client ou cuisinier).</param>
        /// <returns>Nom de la station de métro la plus proche.</returns>
        public static string RecupererStationParId(int id)
        {
            string station = "";
            var con = new MySqlConnection(connectionString);
            con.Open();

            /// Préparation de la requête pour extraire la station de métro
            var cmd = new MySqlCommand("SELECT Metro_Proche FROM Utilisateur WHERE Id_Utilisateur = @id", con);
            cmd.Parameters.AddWithValue("@id", id);

            /// Exécution de la requête
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                station = result.ToString();
            }

            con.Close();
            return station;
        }


        /// <summary>
        /// Affiche le menu réservé aux cuisiniers connectés.
        /// </summary>
        /// <param name="idCuisinier">ID du cuisinier connecté.</param>
        /// <param name="graphe">Graphe utilisé pour visualiser les itinéraires de livraison.</param>
        public static void AfficherMenuCuisinier(int idCuisinier, Graphe<int> graphe)
        {
            bool retour = false;

            while (retour == false)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Cuisinier ===");
                Console.WriteLine("1. Ajouter un repas");
                Console.WriteLine("2. Supprimer un repas");
                Console.WriteLine("3. Voir mes commandes");
                Console.WriteLine("4. Retour");
                Console.Write("Choix : ");

                string choix = Console.ReadLine();

                if (choix == "1")
                {
                    AjouterRepas(idCuisinier);
                }
                else if (choix == "2")
                {
                    SupprimerRepas(idCuisinier);
                }
                else if (choix == "3")
                {
                    VoirCommandesCuisinier(idCuisinier, graphe);
                }
                else if (choix == "4")
                {
                    retour = true;
                }
                else
                {
                    Console.WriteLine("Choix invalide.");
                }

                if (retour == false)
                {
                    Console.WriteLine("Appuie sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Supprime un repas proposé par un cuisinier donné.
        /// </summary>
        /// <param name="idCuisinier">Identifiant du cuisinier concerné.</param>
        public static void SupprimerRepas(int idCuisinier)
        {
            var con = new MySqlConnection(connectionString);
            con.Open();

            /// Affichage des repas actuellement proposés par le cuisinier
            Console.WriteLine("--- Vos repas actuels ---");
            var cmd = new MySqlCommand(@"
            SELECT r.Id_Repas, r.Nom
            FROM cuisinier_repas cr
            JOIN repas r ON cr.Id_Repas = r.Id_Repas
            WHERE cr.Id_Cuisinier = @Id", con);
            cmd.Parameters.AddWithValue("@Id", idCuisinier);
            var reader = cmd.ExecuteReader();

            var repas = new List<int>();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string nom = reader.GetString(1);
                repas.Add(id);
                Console.WriteLine(id + " - " + nom);
            }
            reader.Close();

            /// Demande à l'utilisateur le repas à supprimer
            Console.Write("ID du repas à supprimer : ");
            int idRepas = int.Parse(Console.ReadLine());

            /// Vérifie si ce repas appartient bien à ce cuisinier
            if (repas.Contains(idRepas) != true)
            {
                Console.WriteLine("Ce repas ne vous appartient pas.");
                con.Close();
                return;
            }

            /// Suppression de la relation dans la table cuisinier_repas
            cmd = new MySqlCommand("DELETE FROM cuisinier_repas WHERE Id_Cuisinier = @IdC AND Id_Repas = @IdR", con);
            cmd.Parameters.AddWithValue("@IdC", idCuisinier);
            cmd.Parameters.AddWithValue("@IdR", idRepas);
            cmd.ExecuteNonQuery();

            /// Vérifie si le repas est encore lié à d'autres cuisiniers
            cmd = new MySqlCommand("SELECT COUNT(*) FROM cuisinier_repas WHERE Id_Repas = @IdR", con);
            cmd.Parameters.AddWithValue("@IdR", idRepas);
            int count = Convert.ToInt32(cmd.ExecuteScalar());

            /// Si le repas n'est plus associé à aucun cuisinier, on le supprime de la table Repas
            if (count == 0)
            {
                var cmdDel = new MySqlCommand("DELETE FROM Repas WHERE Id_Repas = @Id", con);
                cmdDel.Parameters.AddWithValue("@Id", idRepas);
                cmdDel.ExecuteNonQuery();
            }

            /// Confirmation à l'utilisateur
            Console.WriteLine("Repas supprimé.");
            con.Close();
        }


        /// <summary>
        /// Ajoute un nouveau repas associé au cuisinier connecté.
        /// </summary>
        /// <param name="idCuisinier">ID du cuisinier connecté.</param>
        public static void AjouterRepas(int idCuisinier)
        {
            var con = new MySqlConnection(connectionString);
            con.Open();

            Console.Write("Nom du repas : ");
            string nom = Console.ReadLine();

            Console.Write("Type (Entrée / Plat / Dessert) : ");
            string type = Console.ReadLine();

            Console.Write("Prix : ");
            decimal prix = decimal.Parse(Console.ReadLine());

            Console.Write("Régime : ");
            string regime = Console.ReadLine();

            Console.Write("Nationalité : ");
            string nationalite = Console.ReadLine();

            if (regime == "")
            {
                regime = null;
            }

            if (nationalite == "")
            {
                nationalite = null;
            }

            var cmd = new MySqlCommand(
                "INSERT INTO Repas (Nom, Type, Prix, Regime, Nationalite) " +
                "VALUES (@Nom, @Type, @Prix, @Regime, @Nationalite)", con);

            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.Parameters.AddWithValue("@Prix", prix);

            if (regime == null)
            {
                cmd.Parameters.AddWithValue("@Regime", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Regime", regime);
            }

            if (nationalite == null)
            {
                cmd.Parameters.AddWithValue("@Nationalite", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Nationalite", nationalite);
            }

            cmd.ExecuteNonQuery();
            int idRepas = (int)cmd.LastInsertedId;

            var cmdLien = new MySqlCommand(
                "INSERT INTO cuisinier_repas (Id_Cuisinier, Id_Repas) VALUES (@IdC, @IdR)", con);
            cmdLien.Parameters.AddWithValue("@IdC", idCuisinier);
            cmdLien.Parameters.AddWithValue("@IdR", idRepas);
            cmdLien.ExecuteNonQuery();

            Console.WriteLine("Repas ajouté avec succès !");
            con.Close();
        }


        /// <summary>
        /// Affiche toutes les commandes reçues par un cuisinier, ainsi que les détails et l'itinéraire de livraison.
        /// </summary>
        /// <param name="idCuisinier">ID du cuisinier connecté.</param>
        /// <param name="graphe">Graphe utilisé pour le calcul du trajet de livraison.</param>
        public static void VoirCommandesCuisinier(int idCuisinier, Graphe<int> graphe)
        {
            var con = new MySqlConnection(connectionString);
            con.Open();

            var commandes = new List<(int idCommande, string clientNom, string clientPrenom, decimal total, int idClient)>();

            var cmd = new MySqlCommand(
                "SELECT c.Id_Commande, u.Nom, u.Prenom, c.Prix_Total, u.Id_Utilisateur " +
                "FROM Commande c " +
                "JOIN Utilisateur u ON c.Id_Client = u.Id_Utilisateur " +
                "WHERE c.Id_Cuisinier = @Id", con);
            cmd.Parameters.AddWithValue("@Id", idCuisinier);

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int idCommande = reader.GetInt32(0);
                string nom = reader.GetString(1);
                string prenom = reader.GetString(2);
                decimal total = reader.GetDecimal(3);
                int idClient = reader.GetInt32(4);

                commandes.Add((idCommande, nom, prenom, total, idClient));
            }

            reader.Close();

            Console.WriteLine("--- Vos commandes reçues ---");
            Console.WriteLine();

            foreach (var commande in commandes)
            {
                Console.WriteLine("Commande #" + commande.idCommande + " par " + commande.clientPrenom + " " + commande.clientNom + " - Total : " + commande.total.ToString("F2") + " euros");

                var cmdDetail = new MySqlCommand(
                    "SELECT r.Nom, lc.Quantite, lc.Prix_Unitaire " +
                    "FROM LigneCommande lc " +
                    "JOIN Repas r ON lc.Id_Repas = r.Id_Repas " +
                    "WHERE lc.Id_Commande = @Id", con);
                cmdDetail.Parameters.AddWithValue("@Id", commande.idCommande);

                var rd = cmdDetail.ExecuteReader();

                while (rd.Read())
                {
                    string nomRepas = rd.GetString(0);
                    int qte = rd.GetInt32(1);
                    decimal prix = rd.GetDecimal(2);
                    decimal totalLigne = qte * prix;

                    Console.WriteLine("    • " + qte + " x " + nomRepas + " = " + totalLigne.ToString("F2") + " euros");
                }

                rd.Close();

                string stationClient = RecupererStationParId(commande.idClient);
                string stationCuisinier = RecupererStationParId(idCuisinier);

                int idStationClient = graphe.GetIdSommetParNom(stationClient);
                int idStationCuisinier = graphe.GetIdSommetParNom(stationCuisinier);

                if (idStationClient != -1 && idStationCuisinier != -1)
                {
                    Console.WriteLine();
                    Console.WriteLine("Itinéraire depuis " + stationCuisinier + " jusqu'à " + stationClient + " :");

                    double tempsLivraison = graphe.Dijkstra(idStationCuisinier, idStationClient);

                    Console.WriteLine();
                    Console.WriteLine("Livraison estimée en " + tempsLivraison + " minutes.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Erreur : itinéraire non trouvable (station manquante).");
                }

                Console.WriteLine();
            }

            con.Close();
        }

        /// <summary>
        /// Récupère toutes les paires (client, cuisinier) ayant effectué une commande.
        /// </summary>
        /// <returns>Liste des paires d'identifiants client et cuisinier.</returns>
        public static List<(int idClient, int idCuisinier)> RecupererCommandes()
        {
            var liste = new List<(int, int)>();

            var connexion = new MySqlConnection(connectionString);
            connexion.Open();

            string requete = "SELECT Id_Client, Id_Cuisinier FROM Commande";
            var cmd = new MySqlCommand(requete, connexion);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int idClient = reader.GetInt32(0);
                int idCuisinier = reader.GetInt32(1);
                liste.Add((idClient, idCuisinier));
            }

            reader.Close();
            connexion.Close();

            return liste;
        }

        /// <summary>
        /// Récupère les rôles de tous les utilisateurs.
        /// </summary>
        /// <returns>Dictionnaire associant l'ID de l'utilisateur à son rôle.</returns>
        public static Dictionary<int, string> RecupererRoles()
        {
            var dict = new Dictionary<int, string>();

            var connexion = new MySqlConnection(connectionString);
            connexion.Open();

            string requete = "SELECT Id_Utilisateur, Role FROM Utilisateur";
            var cmd = new MySqlCommand(requete, connexion);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string role = reader.GetString(1);
                dict[id] = role;
            }

            reader.Close();
            connexion.Close();

            return dict;
        }

        /// <summary>
        /// Récupère les stations de métro associées à chaque utilisateur.
        /// </summary>
        /// <returns>Dictionnaire associant l'ID utilisateur à sa station proche.</returns>
        public static Dictionary<int, string> RecupererStations()
        {
            var dict = new Dictionary<int, string>();

            var connexion = new MySqlConnection(connectionString);
            connexion.Open();

            string requete = "SELECT Id_Utilisateur, Metro_Proche FROM Utilisateur";
            var cmd = new MySqlCommand(requete, connexion);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string station;

                if (reader.IsDBNull(1))
                {
                    station = "";
                }
                else
                {
                    station = reader.GetString(1);
                }

                dict[id] = station;
            }

            reader.Close();
            connexion.Close();

            return dict;
        }

    }
}
