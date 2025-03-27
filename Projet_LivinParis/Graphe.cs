using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using ExcelDataReader;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Antlr.Runtime;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.LinkLabel;
using System.Runtime.ConstrainedExecution;
namespace PROJET_PSI
{
    public class Graphe<T>
    {
        private Dictionary<int, Noeud<T>> noeuds = new Dictionary<int, Noeud<T>>();
        private Dictionary<int, string> lignesStations = new Dictionary<int, string>(); // Dictionnaire pour les lignes des stations
        public Dictionary<int, Noeud<T>> Noeuds => noeuds;
        public Dictionary<int, string> LignesStations => lignesStations; // Accesseur pour les lignes des stations

        public void AjouterNoeud(int id)
        {
            if (!noeuds.ContainsKey(id))
            {
                noeuds[id] = new Noeud<T>(id);
            }
        }

        public void AjouterLien(int id1, int id2, double poids = 1.0)
        {
            AjouterNoeud(id1);
            AjouterNoeud(id2);
            noeuds[id1].Liens.Add(new Lien<T>(noeuds[id2], poids));
            noeuds[id2].Liens.Add(new Lien<T>(noeuds[id1], poids));
        }

        public void ChargerGraphe(string fichier)
        {
            using (var stream = File.Open(fichier, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    var tableNoeuds = result.Tables[0];
                    var tableArcs = result.Tables[1];

                    var nomToIds = new Dictionary<string, List<int>>();

                    foreach (DataRow row in tableNoeuds.Rows)
                    {
                        string nomStation = row["Libelle station"].ToString().Trim();
                        int id = Convert.ToInt32(row["ID Station"]);

                        if (!nomToIds.ContainsKey(nomStation))
                            nomToIds[nomStation] = new List<int>();

                        nomToIds[nomStation].Add(id);
                        AjouterNoeud(id);

                        // Ajout du nom de la station
                        noeuds[id].Nom = nomStation;

                        string ligne = row["Libelle Line"].ToString().Trim();
                        lignesStations[id] = ligne;
                    }

                    foreach (DataRow row in tableArcs.Rows)
                    {
                        int id = Convert.ToInt32(row["Station Id"]);
                        double? precedent = row["Précédent"] as double?;
                        double? suivant = row["Suivant"] as double?;
                        double temps = row["Temps entre 2 stations"] != DBNull.Value ? Convert.ToDouble(row["Temps entre 2 stations"]) : 1.0;

                        if (precedent.HasValue)
                            AjouterLien((int)precedent, id, temps);

                        if (suivant.HasValue)
                            AjouterLien(id, (int)suivant, temps);
                    }

                    var dejaLie = new HashSet<(int, int)>();

                    foreach (DataRow row in tableArcs.Rows)
                    {
                        int id = Convert.ToInt32(row["Station Id"]);
                        string nomStation = row["Station"].ToString().Trim();

                        if (row["Temps de Changement"] != DBNull.Value)
                        {
                            double tempsChangement = Convert.ToDouble(row["Temps de Changement"]);

                            if (nomToIds.ContainsKey(nomStation))
                            {
                                foreach (int autreId in nomToIds[nomStation])
                                {
                                    if (autreId != id)
                                    {
                                        var key = (Math.Min(id, autreId), Math.Max(id, autreId));
                                        if (!dejaLie.Contains(key))
                                        {
                                            AjouterLien(id, autreId, tempsChangement);
                                            dejaLie.Add(key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        public Dictionary<int, List<(int, double)>> ConstruireListeAdj()
            {
            Dictionary<int, List<(int, double)>> listeAdj = new Dictionary<int, List<(int, double)>>();
            foreach (var noeud in noeuds)
            {
                listeAdj[noeud.Key] = new List<(int, double)>();
                foreach (var lien in noeud.Value.Liens)
                {
                    listeAdj[noeud.Key].Add((lien.Destination.Id, lien.Poids));
                }
                listeAdj[noeud.Key].Sort((x, y) => x.Item1.CompareTo(y.Item1));
            }
            return listeAdj;
        }

        public void AfficherListeAdj()
        {
            var listeAdj = ConstruireListeAdj();
            List<int> noeudsTries = new List<int>(listeAdj.Keys);
            noeudsTries.Sort();

            foreach (var key in noeudsTries)
            {
                Console.Write(key + ": ");
                foreach (var lien in listeAdj[key])
                {
                    Console.Write($"({lien.Item1}, {lien.Item2}) ");
                }
                Console.WriteLine();
            }
        }

        public double Dijkstra(int source, int cible)
        {
            // Initialisation
            var distances = new Dictionary<int, double>();
            var previous = new Dictionary<int, int>();
            var visited = new HashSet<int>();

            foreach (var node in noeuds.Keys)
            {
                distances[node] = double.PositiveInfinity;
            }
            distances[source] = 0;

            // Algorithme principal
            while (visited.Count < noeuds.Count)
            {
                int currentNode = -1;
                double minDistance = double.PositiveInfinity;

                foreach (var node in noeuds.Keys)
                {
                    if (!visited.Contains(node) && distances[node] < minDistance)
                    {
                        minDistance = distances[node];
                        currentNode = node;
                    }
                }

                if (currentNode == -1 || currentNode == cible) break;

                visited.Add(currentNode);

                foreach (var edge in noeuds[currentNode].Liens)
                {
                    int neighbor = edge.Destination.Id;
                    if (!visited.Contains(neighbor))
                    {
                        double newDistance = distances[currentNode] + edge.Poids;
                        if (newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            previous[neighbor] = currentNode;
                        }
                    }
                }
            }

            // Reconstruction et affichage du chemin
            List<int> path = new List<int>();
            int current = cible;

            while (current != source && previous.ContainsKey(current))
            {
                path.Add(current);
                current = previous[current];
            }
            path.Add(source);
            path.Reverse();

            // Affichage formaté
            Console.WriteLine("\nItinéraire :");
            Console.WriteLine($"Départ : {noeuds[source].Nom} (Ligne {LignesStations[source]})");

            if (path.Count == 1)
            {
                Console.WriteLine("Vous êtes déjà à la station de destination");
                return 0;
            }

            double totalTime = 0;
            string currentLine = LignesStations[source];
            string previousStation = noeuds[source].Nom;
            double segmentTime = 0;

            for (int i = 1; i < path.Count; i++)
            {
                int from = path[i - 1];
                int to = path[i];
                var edge = noeuds[from].Liens.First(e => e.Destination.Id == to);
                segmentTime += edge.Poids;

                if (i == path.Count - 1 || LignesStations[to] != currentLine)
                {
                    Console.WriteLine($"{previousStation} à {noeuds[from].Nom} ({segmentTime} min)");
                    totalTime += segmentTime;

                    if (LignesStations[to] != currentLine && i != path.Count - 1)
                    {
                        double transferTime = edge.Poids;
                        Console.WriteLine($"Correspondance à {noeuds[from].Nom} ({transferTime} min pour rejoindre la ligne {LignesStations[to]})");
                        totalTime += transferTime;
                        currentLine = LignesStations[to];
                        previousStation = noeuds[from].Nom;
                        segmentTime = 0;
                    }
                }
            }

            Console.WriteLine($"Arrivée à {noeuds[cible].Nom} en {totalTime} minutes");
            return distances[cible];
        }




        public double BellmanFord(int source, int cible)
        {
            var distances = new Dictionary<int, double>();

            // Initialisation : toutes les distances à l'infini, sauf la source à 0
            foreach (var noeud in noeuds.Keys)
            {
                distances[noeud] = double.PositiveInfinity;
            }
            distances[source] = 0;

            // Relaxation des arêtes
            for (int i = 0; i < noeuds.Count - 1; i++)
            {
                foreach (var noeud in noeuds)
                {
                    foreach (var lien in noeud.Value.Liens)
                    {
                        if (distances[noeud.Key] + lien.Poids < distances[lien.Destination.Id])
                        {
                            distances[lien.Destination.Id] = distances[noeud.Key] + lien.Poids;
                        }
                    }
                }
            }

            return distances[cible];
        }

        public double FloydWarshall(int source, int cible)
        {
            // Étape 1 : Créer une table de correspondance ID ↔ index
            var idList = noeuds.Keys.ToList();
            var idToIndex = new Dictionary<int, int>();
            var indexToId = new Dictionary<int, int>();

            for (int i = 0; i < idList.Count; i++)
            {
                idToIndex[idList[i]] = i;
                indexToId[i] = idList[i];
            }

            int n = idList.Count;
            double[,] dist = new double[n, n];

            // Étape 2 : Initialiser la matrice
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = (i == j) ? 0 : -1; // -1 = pas de lien
                }
            }

            // Étape 3 : Remplir la matrice avec les poids du graphe
            foreach (var noeud in noeuds)
            {
                int i = idToIndex[noeud.Key];
                foreach (var lien in noeud.Value.Liens)
                {
                    int j = idToIndex[lien.Destination.Id];
                    dist[i, j] = lien.Poids;
                }
            }

            // Étape 4 : Algorithme Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != -1 && dist[k, j] != -1)
                        {
                            double nouveau = dist[i, k] + dist[k, j];
                            if (dist[i, j] == -1 || nouveau < dist[i, j])
                            {
                                dist[i, j] = nouveau;
                            }
                        }
                    }
                }
            }

            // Étape 5 : Traduire les ID d'entrée en indices
            if (!idToIndex.ContainsKey(source) || !idToIndex.ContainsKey(cible))
                return -1; // Source ou cible inexistante

            double resultat = dist[idToIndex[source], idToIndex[cible]];
            return resultat == -1 ? -1 : resultat;
        }

        public void DessinerGraphe(string outputPath = "graphe.png")
        {
            // Paramètres ajustés pour 333 nœuds
            const int nodeRadius = 3; // Beaucoup plus petit
            const int imageWidth = 2500; // Image plus grande
            const int imageHeight = 2500;
            const int centerX = imageWidth / 2;
            const int centerY = imageHeight / 2;
            int circleRadius = (Math.Min(imageWidth, imageHeight) / 2) - 50;
            const int edgeLabelMargin = 2;

            using (var bitmap = new Bitmap(imageWidth, imageHeight))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Configuration du dessin
                graphics.Clear(Color.White);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Calcul des positions des nœuds
                var nodePositions = new Dictionary<int, PointF>();
                double angleStep = 2 * Math.PI / noeuds.Count;
                double currentAngle = 0;

                // Deux cercles concentriques pour réduire la superposition
                int circleToggle = 0;
                int smallCircleRadius = circleRadius - 30;

                foreach (var node in noeuds.Values)
                {
                    float radius = circleToggle == 0 ? circleRadius : smallCircleRadius;
                    float x = centerX + (float)(radius * Math.Cos(currentAngle));
                    float y = centerY + (float)(radius * Math.Sin(currentAngle));
                    nodePositions[node.Id] = new PointF(x, y);

                    currentAngle += angleStep;
                    circleToggle = 1 - circleToggle; // Alterne entre les deux cercles
                }

                // Dessin des arêtes avec leurs poids (seulement pour les poids significatifs)
                var drawnEdges = new HashSet<string>();
                using (var edgePen = new Pen(Color.FromArgb(100, Color.Gray), 1)) // Arêtes semi-transparentes
                using (var labelFont = new Font("Arial", 6))
                using (var labelBrush = new SolidBrush(Color.Red))
                {
                    foreach (var node in noeuds.Values)
                    {
                        foreach (var edge in node.Liens)
                        {
                            string edgeKey = $"{Math.Min(node.Id, edge.Destination.Id)}-{Math.Max(node.Id, edge.Destination.Id)}";
                            if (!drawnEdges.Contains(edgeKey))
                            {
                                drawnEdges.Add(edgeKey);

                                PointF start = nodePositions[node.Id];
                                PointF end = nodePositions[edge.Destination.Id];

                                // Dessiner la ligne seulement si elle n'est pas trop courte
                                if (Distance(start, end) > nodeRadius * 4)
                                {
                                    graphics.DrawLine(edgePen, start, end);

                                    // Dessiner le poids seulement si différent de 1
                                    if (Math.Abs(edge.Poids - 1.0) > 0.01)
                                    {
                                        PointF middle = new PointF(
                                            (start.X + end.X) / 2 + edgeLabelMargin,
                                            (start.Y + end.Y) / 2 + edgeLabelMargin);
                                        graphics.DrawString(edge.Poids.ToString("F1"), labelFont, labelBrush, middle);
                                    }
                                }
                            }
                        }
                    }
                }

                // Dessin des nœuds (très simplifié)
                using (var nodeBrush = new SolidBrush(Color.FromArgb(200, Color.LightBlue))) // Semi-transparent
                using (var nodePen = new Pen(Color.Black, 0.5f))
                using (var textFont = new Font("Arial", 5))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    foreach (var kvp in nodePositions)
                    {
                        RectangleF nodeRect = new RectangleF(
                            kvp.Value.X - nodeRadius,
                            kvp.Value.Y - nodeRadius,
                            nodeRadius * 2,
                            nodeRadius * 2);

                        graphics.FillEllipse(nodeBrush, nodeRect);
                        graphics.DrawEllipse(nodePen, nodeRect);

                        // On n'affiche le texte que pour certains nœuds pour éviter la surcharge
                        if (kvp.Key % 10 == 0) // Un numéro sur 10
                        {
                            string label = kvp.Key.ToString();
                            SizeF textSize = graphics.MeasureString(label, textFont);
                            PointF textPos = new PointF(
                                kvp.Value.X - textSize.Width / 2,
                                kvp.Value.Y - textSize.Height / 2);

                            graphics.DrawString(label, textFont, textBrush, textPos);
                        }
                    }
                }

                bitmap.Save(outputPath, ImageFormat.Png);
            }

            Console.WriteLine($"Graphe généré dans {outputPath}");
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
