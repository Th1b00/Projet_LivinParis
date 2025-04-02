using PROJET_PSI;
using System;
using System.Linq;

namespace PROJET_PSI
{
    public class Program
    {
        static void Main(string[] args)
        {
            var graphe = new Graphe<string>();

            graphe.ChargerGraphe("metro.xlsx");

            // Affichage de la liste d'adjacence du graphe
            Console.WriteLine("\n\nListe d'adjacence :\n");
            graphe.AfficherListeAdj();
            Console.WriteLine("\nOrdre du graphe : " + graphe.Noeuds.Count);

            int taille = 0;
            foreach (var noeud in graphe.Noeuds.Values)
            {
                taille += noeud.Liens.Count;
            }

            Console.WriteLine("Taille du graphe : " + Convert.ToString((taille / 2) - 1));

            // Demande à l'utilisateur de choisir les stations
            Console.WriteLine("\nEntre quels sommets souhaitez-vous calculer le plus court chemin ?");
            int sommetA = graphe.LireSommetValide("Sommet a : ");
            int sommetB = graphe.LireSommetValide("Sommet b : ");


            // Calcul des plus courts chemins avec les 3 algorithmes
            Console.WriteLine("\nDistance entre " + sommetA + " et " + sommetB + " :");

            double resultatDijkstra = graphe.Dijkstra(sommetA, sommetB);
            Console.WriteLine("Dijkstra       : " + resultatDijkstra + " minutes");
            Console.WriteLine("Bellman-Ford   : " + graphe.BellmanFord(sommetA, sommetB) + " minutes");
            Console.WriteLine("Floyd-Warshall : " + graphe.FloydWarshall(sommetA, sommetB) + " minutes");

            // Génération du dessin du graphe complet
            Console.WriteLine("\nGénération du dessin du graphe complet...");
            graphe.DessinerGraphe("graphe_metro_complet.png");
            Console.WriteLine("Le graphe complet a été enregistré sous 'graphe_metro_complet.png'.");

            // Message concernant le graphe du chemin
            Console.WriteLine("\nGénération du graphe du chemin...");
            Console.WriteLine("Le graphe du chemin de " + sommetA + " à " + sommetB + " a été généré sous 'chemin_" + sommetA + "_vers_" + sommetB + ".png'");

            string cheminImage = "chemin_" + sommetA + "_vers_" + sommetB + ".png";
            graphe.AfficherCheminDansFenetre(cheminImage);

            // Fin de l'exécution
            Console.WriteLine("\nAppuyez sur une touche pour quitter.");
            Console.ReadKey();
        }
    }
}