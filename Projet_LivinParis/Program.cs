
using PROJET_PSI;
using System;
using System.Linq;

namespace PROJET_PSI
{
    using System;

    namespace Projet_LivinParis
    {
        public class Program
        {
            static void Main(string[] args)
            {
                bool quitter = false;
                while (!quitter)
                {
                    Console.Clear();
                    Console.WriteLine("=== Menu Principal ===");
                    Console.WriteLine("1. Requêtes graphe métro");
                    Console.WriteLine("2. Application Liv'in Paris (SQL)");
                    Console.WriteLine("3. Quitter");
                    Console.Write("Choix : ");
                    string choix = Console.ReadLine();

                    switch (choix)
                    {
                        case "1":
                            var graphe = new Graphe<int>();
                            graphe.ChargerGraphe("metro.xlsx");

                            Console.WriteLine("\n\nListe d'adjacence :\n");
                            graphe.AfficherListeAdj();
                            Console.WriteLine("\nOrdre du graphe : " + graphe.Noeuds.Count);

                            int taille = 0;
                            foreach (var noeud in graphe.Noeuds.Values)
                            {
                                taille += noeud.Liens.Count;
                            }
                            Console.WriteLine("Taille du graphe : " + ((taille / 2) - 1));

                            Console.WriteLine("\nEntre quels sommets souhaitez-vous calculer le plus court chemin ?");
                            int sommetA = graphe.LireSommetValide("Sommet a : ");
                            int sommetB = graphe.LireSommetValide("Sommet b : ");

                            Console.WriteLine("\nDistance entre " + sommetA + " et " + sommetB + " :");
                            Console.WriteLine("Dijkstra       : " + graphe.Dijkstra(sommetA, sommetB) + " minutes");
                            Console.WriteLine("Bellman-Ford   : " + graphe.BellmanFord(sommetA, sommetB) + " minutes");
                            Console.WriteLine("Floyd-Warshall : " + graphe.FloydWarshall(sommetA, sommetB) + " minutes");

                            Console.WriteLine("\nGénération du dessin du graphe complet...");
                            graphe.DessinerGraphe("graphe_metro_complet.png");
                            Console.WriteLine("Le graphe complet a été enregistré sous 'graphe_metro_complet.png'.");

                            Console.WriteLine("\nGénération du graphe du chemin...");
                            Console.WriteLine("Le graphe du chemin de " + sommetA + " à " + sommetB + " a été généré sous 'chemin_" + sommetA + "_vers_" + sommetB + ".png'");

                            string cheminImage = "chemin_" + sommetA + "_vers_" + sommetB + ".png";
                            graphe.AfficherCheminDansFenetre(cheminImage);

                            Console.WriteLine("\nAppuyez sur une touche pour revenir au menu.");
                            Console.ReadKey();
                            break;

                        case "2":
                            MYSQL.AfficherMenu();
                            break;

                        case "3":
                            quitter = true;
                            break;

                        default:
                            Console.WriteLine("Choix invalide. Appuyez sur une touche pour réessayer.");
                            Console.ReadKey();
                            break;
                    }
                }
            }
        }
    }
}
