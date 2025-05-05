using System;
using System.Linq;

namespace PROJET_PSI
{
    public class Program
    {
        /// <summary>
        /// Affiche un menu permettant à l'utilisateur de :
        /// 1 - Lancer l'application Livin'Paris
        /// 2 - Analyser le graphe (coloration, bipartisme, planarité)
        /// 3 - Exporter le graphe au format JSON ou XML
        /// 4 - Tester les fonctionnalités d'import/export JSON et XML
        /// 5 - Quitter l'application
        /// </summary>
        static void Main(string[] args)
        {
            var graphe = new Graphe<int>();
            graphe.ChargerGraphe("metro.xlsx");

            bool quitter = false;

            while (quitter == false)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Principal ===");
                Console.WriteLine("1. Lancer Livin'Paris");
                Console.WriteLine("2. Analyser le graphe (Welsh-Powell, Biparti, Planarité)");
                Console.WriteLine("3. Exporter le graphe (JSON/XML)");
                Console.WriteLine("4. Tester Export/Import JSON et XML");
                Console.WriteLine("5. Quitter");
                Console.Write("Choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        MYSQL.AfficherMenuPrincipal(graphe);
                        break;

                    case "2":
                        Console.Clear();
                        graphe.AfficherStationsAvecColoration();

                        int nbCouleurs = graphe.ColorationWelshPowell().Values.Distinct().Count();
                        Console.WriteLine("Le graphe est " + nbCouleurs + "-Coloriable");

                        if (graphe.EstBiparti() == true)
                        {
                            Console.WriteLine("Le graphe est biparti");
                        }
                        else
                        {
                            Console.WriteLine("Le graphe n'est pas biparti");
                        }

                        if (graphe.EstPlanaire() == true)
                        {
                            Console.WriteLine("Le graphe est plannaire");
                        }
                        else
                        {
                            Console.WriteLine("Le graphe n'est pas plannaire");
                        }

                        Console.WriteLine("\nAppuie sur une touche pour revenir au menu...");
                        Console.ReadKey();
                        break;

                    case "3":
                        Console.Clear();
                        Console.WriteLine("=== Export du Graphe ===");
                        Console.WriteLine("1. Exporter en JSON");
                        Console.WriteLine("2. Exporter en XML");
                        Console.Write("Choix : ");

                        string exportChoix = Console.ReadLine();

                        if (exportChoix == "1")
                        {
                            graphe.ExportJson("graphe.json");
                            Console.WriteLine("Export JSON effectué (graphe.json).");
                        }
                        else if (exportChoix == "2")
                        {
                            graphe.ExportXml("graphe.xml");
                            Console.WriteLine("Export XML effectué (graphe.xml).");
                        }
                        else
                        {
                            Console.WriteLine("Choix invalide.");
                        }

                        Console.WriteLine("\nAppuie sur une touche pour revenir au menu...");
                        Console.ReadKey();
                        break;

                    case "4":
                        Console.Clear();
                        Console.WriteLine("=== Test Export/Import JSON/XML ===");

                        string jsonPath = "test_export.json";
                        string xmlPath = "test_export.xml";

                        graphe.ExportJson(jsonPath);
                        Console.WriteLine("Export JSON : " + jsonPath);

                        graphe.ExportXml(xmlPath);
                        Console.WriteLine("Export XML  : " + xmlPath);

                        var grapheJson = new Graphe<int>();
                        grapheJson.ImportJson(jsonPath);
                        Console.WriteLine("Import JSON : " + grapheJson.Noeuds.Count + " sommets chargés");

                        var grapheXml = new Graphe<int>();
                        grapheXml.ImportXml(xmlPath);
                        Console.WriteLine("Import XML  : " + grapheXml.Noeuds.Count + " sommets chargés");

                        Console.WriteLine("\nAppuie sur une touche pour revenir au menu...");
                        Console.ReadKey();
                        break;

                    case "5":
                        quitter = true;
                        break;

                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
