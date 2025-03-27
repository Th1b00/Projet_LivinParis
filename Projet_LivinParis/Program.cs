using PROJET_PSI;
using System;
using System.Linq;

namespace PROJET_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var graphe = new Graphe<string>();
            graphe.ChargerGraphe("metro.xlsx");

            // Affichage de la liste d'adjacence du graphe
            Console.WriteLine("\n\nListe d'Adjacence :\n");
            graphe.AfficherListeAdj();
            Console.WriteLine("\nOrdre du graphe: " + graphe.Noeuds.Count);

            int taille = 0;
            foreach (var n in graphe.Noeuds.Values)
                taille += n.Liens.Count;
            Console.WriteLine("Taille du graphe: " + Convert.ToString((taille / 2) - 1));

            // Demande à l'utilisateur de choisir les stations pour calculer le plus court chemin
            Console.Write("\nEntre quels sommets souhaitez-vous calculer le plus court chemin ?\nSommet a : ");
            int a = Convert.ToInt32(Console.ReadLine());
            Console.Write("Sommet b : ");
            int b = Convert.ToInt32(Console.ReadLine());

            // Calcul du plus court chemin
            Console.WriteLine($"\nDistance entre {a} et {b} :");
            double dijkstraResult = graphe.Dijkstra(a, b);  // Cette ligne affichera l'itinéraire
            Console.WriteLine("Dijkstra       : " + dijkstraResult + " minutes");
            Console.WriteLine("Bellman-Ford   : " + graphe.BellmanFord(a, b) + " minutes");
            Console.WriteLine("Floyd-Warshall : " + graphe.FloydWarshall(a, b) + " minutes");

            // Génération du dessin du graphe et sauvegarde dans un fichier image
            Console.WriteLine("\nGénération du dessin du graphe...");
            graphe.DessinerGraphe("graphe_metro.png");
            Console.WriteLine("Le graphe a été enregistré sous 'graphe_metro.png'.");

            // Fin de l'exécution
            Console.WriteLine("\nAppuyez sur une touche pour quitter.");
            Console.ReadKey();
        }
    }
}
