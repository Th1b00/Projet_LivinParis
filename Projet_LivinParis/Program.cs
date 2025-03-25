using PROJET_PSI;
using System;

namespace PROJET_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Graphe<string> graphe = new Graphe<string>();
            
            graphe.ChargerGraphe("metro.xlsx");
            
            Console.WriteLine("\n\nListe d'Adjacence : \n");
            graphe.AfficherListeAdj();
            Console.WriteLine("\nOrdre du graphe: " + graphe.Noeuds.Count);
            int taille = 0;
            foreach (var n in graphe.Noeuds.Values) taille += n.Liens.Count;
            Console.WriteLine("\nTaille du graphe: " + Convert.ToString((taille / 2) - 1));
            Console.WriteLine();
            Console.WriteLine("Entre quels sommets souhaitez vous calculer le plus court chemin ? \nsommet a : ");
            int a = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("sommet b : ");
            int b = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("La distance entre " + a + " et " + b + " d'après Dijkstra est : " + graphe.Dijkstra(a, b) + " minutes");
            Console.WriteLine("D'après Bellman-Ford : " + graphe.BellmanFord(a, b) + " minutes");
            Console.WriteLine("D'après Floyd-Warshall : " + graphe.FloydWarshall(a,b) + " minutes");    
            //graphe.DessinerGraphe();
            Console.ReadKey();

        }
    }
}
