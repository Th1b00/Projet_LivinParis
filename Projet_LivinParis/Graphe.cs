using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using ExcelDataReader;

using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace PROJET_PSI
{
    public class Graphe<T>
    {
        private Dictionary<int, Noeud<T>> noeuds = new Dictionary<int, Noeud<T>>();
        

        public Dictionary<int, Noeud<T>> Noeuds
        {
            get { return noeuds; }
        }

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

                    var table = result.Tables[1]; // Utiliser la deuxième feuille (celle des stations)

                    foreach (DataRow row in table.Rows)
                    {
                        // Lire les informations depuis chaque ligne du fichier Excel
                        int id = Convert.ToInt32(row["Station Id"]); // Station Id
                        double? precedent = row["Précédent"] as double?; // Station précédente (optionnelle)
                        double? suivant = row["Suivant"] as double?; // Station suivante (optionnelle)
                        double temps = Convert.ToDouble(row["Temps entre 2 stations"]); // Temps entre les stations

                        // Ajouter la station (noeud) si elle n'existe pas déjà
                        AjouterNoeud(id);

                        // Ajouter les liens (uniquement si une station est définie)
                        if (precedent.HasValue)
                        {
                            AjouterLien((int)precedent, id, temps); // Liaison unidirectionnelle (Précédent -> Station)
                        }
                        if (suivant.HasValue)
                        {
                            AjouterLien(id, (int)suivant, temps); // Liaison unidirectionnelle (Station -> Suivant)
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
            var distances = new Dictionary<int, double>();
            var visited = new HashSet<int>();

            // Initialisation des distances à l'infini, sauf la source
            foreach (var noeud in noeuds.Keys)
                distances[noeud] = double.PositiveInfinity;

            distances[source] = 0;

            while (visited.Count < noeuds.Count)
            {
                // Trouver le sommet non visité avec la plus petite distance
                int noeudCourant = -1;
                double minDistance = double.PositiveInfinity;

                foreach (var noeud in noeuds.Keys)
                {
                    if (!visited.Contains(noeud) && distances[noeud] < minDistance)
                    {
                        minDistance = distances[noeud];
                        noeudCourant = noeud;
                    }
                }

                if (noeudCourant == -1) break; // Tous les sommets atteignables ont été traités
                if (noeudCourant == cible) return distances[cible]; // On a atteint la cible, on s'arrête

                visited.Add(noeudCourant);

                // Mise à jour des distances des voisins
                foreach (var lien in noeuds[noeudCourant].Liens)
                {
                    if (!visited.Contains(lien.Destination.Id))
                    {
                        double nouvelleDistance = distances[noeudCourant] + lien.Poids;
                        if (nouvelleDistance < distances[lien.Destination.Id])
                        {
                            distances[lien.Destination.Id] = nouvelleDistance;
                        }
                    }
                }
            }

            return double.PositiveInfinity; // Retourne ∞ si aucun chemin n'existe entre source et cible
        }

        public double BellmanFord(int source, int cible)
        {
            Dictionary<int, double> distances = new Dictionary<int, double>();

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
            // Trouver l'index maximum dans le graphe pour éviter les dépassements de tableau
            int maxIndex = noeuds.Keys.Max() + 1;
            double[,] dist = new double[maxIndex, maxIndex];

            // Initialisation de la matrice
            for (int i = 0; i < maxIndex; i++)
            {
                for (int j = 0; j < maxIndex; j++)
                {
                    if (i == j) dist[i, j] = 0;
                    else dist[i, j] = double.PositiveInfinity;
                }
            }

            // Remplissage de la matrice avec les distances existantes
            foreach (var noeud in noeuds)
            {
                foreach (var lien in noeud.Value.Liens)
                {
                    dist[noeud.Key, lien.Destination.Id] = lien.Poids;
                }
            }

            // Algorithme de Floyd-Warshall
            for (int k = 0; k < maxIndex; k++)
            {
                for (int i = 0; i < maxIndex; i++)
                {
                    for (int j = 0; j < maxIndex; j++)
                    {
                        if (dist[i, k] != double.PositiveInfinity && dist[k, j] != double.PositiveInfinity)
                        {
                            dist[i, j] = Math.Min(dist[i, j], dist[i, k] + dist[k, j]);
                        }
                    }
                }
            }

            // Vérification que source et cible existent dans le graphe
            if (!noeuds.ContainsKey(source) || !noeuds.ContainsKey(cible))
            {
                return double.PositiveInfinity; // Retourne une valeur infinie si les sommets n'existent pas
            }

            return dist[source, cible]; // Retourne la distance minimale
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
