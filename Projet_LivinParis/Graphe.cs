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
        private Dictionary<int, string> lignesStations = new Dictionary<int, string>();

        public Dictionary<int, Noeud<T>> Noeuds
        {
            get { return noeuds; }
            set { noeuds = value; }
        }

        public Dictionary<int, string> LignesStations
        {
            get { return lignesStations; }
            set { lignesStations = value; }
        }

        public void AjouterNoeud(int id)
        {
            if (noeuds.ContainsKey(id) != true)
            {
                noeuds[id] = new Noeud<T>(id);
            }
        }

        private bool ExisteLienEntre(int source, int destination)
        {
            foreach (var lien in noeuds[source].Liens)
            {
                if (lien.Destination.Id == destination)
                {
                    return true;
                }
            }
            return false;
        }

        public void AjouterLien(int id1, int id2, double poids = 1.0)
        {
            AjouterNoeud(id1);
            AjouterNoeud(id2);

            if (ExisteLienEntre(id1, id2) != true)
            {
                noeuds[id1].Liens.Add(new Lien<T>(noeuds[id2], poids));
            }

            if (ExisteLienEntre(id2, id1) != true)
            {
                noeuds[id2].Liens.Add(new Lien<T>(noeuds[id1], poids));
            }
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

                    var idParStation = new Dictionary<string, List<int>>();


                    foreach (DataRow row in tableNoeuds.Rows)
                    {
                        string nomStation = row["Libelle station"].ToString().Trim();
                        int id = Convert.ToInt32(row["ID Station"]);

                        if (idParStation.ContainsKey(nomStation) != true)
                        {
                            idParStation[nomStation] = new List<int>();
                        }


                        idParStation[nomStation].Add(id);
                        AjouterNoeud(id);

                        // Ajout du nom de la station
                        noeuds[id].Nom = nomStation;

                        string ligne = row["Libelle Line"].ToString().Trim();
                        lignesStations[id] = ligne;
                    }

                    foreach (DataRow row in tableArcs.Rows)
                    {
                        int id = Convert.ToInt32(row["Station Id"]);
                        double precedent = -1;
                        double suivant = -1;
                        double temps = 1.0;

                        if (row["Précédent"] != DBNull.Value)
                        {
                            precedent = Convert.ToDouble(row["Précédent"]);
                        }

                        if (row["Suivant"] != DBNull.Value)
                        {
                            suivant = Convert.ToDouble(row["Suivant"]);
                        }

                        if (row["Temps entre 2 stations"] != DBNull.Value)
                        {
                            temps = Convert.ToDouble(row["Temps entre 2 stations"]);
                        }

                        if (precedent != -1)
                        {
                            AjouterLien((int)precedent, id, temps);
                        }

                        if (suivant != -1)
                        {
                            AjouterLien(id, (int)suivant, temps);
                        }
                    }

                    var dejaLie = new HashSet<(int, int)>();

                    var dejaLiee = new HashSet<(int, int)>();

                    foreach (DataRow row in tableArcs.Rows)
                    {
                        int id = Convert.ToInt32(row["Station Id"]);
                        string nomStation = row["Station"].ToString().Trim();

                        if (row["Temps de Changement"] != DBNull.Value)
                        {
                            double tempsChangement = Convert.ToDouble(row["Temps de Changement"]);

                            if (idParStation.ContainsKey(nomStation) != false)
                            {
                                foreach (int autreId in idParStation[nomStation])
                                {
                                    if (autreId != id)
                                    {
                                        var cle = (Math.Min(id, autreId), Math.Max(id, autreId));
                                        if (dejaLiee.Contains(cle) != true)
                                        {
                                            AjouterLien(id, autreId, tempsChangement);
                                            dejaLiee.Add(cle);
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

                listeAdj[noeud.Key].Sort(ComparerVoisinsParId);
            }

            return listeAdj;
        }

        private int ComparerVoisinsParId((int, double) x, (int, double) y)
        {
            return x.Item1.CompareTo(y.Item1);
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
                    Console.Write("(" + lien.Item1 + ", " + lien.Item2 + ") ");
                }
                Console.WriteLine();
            }
        }

        public double Dijkstra(int depart, int arrivee)
        {
            var distances = new Dictionary<int, double>();
            var precedent = new Dictionary<int, int>();
            var visites = new HashSet<int>();

            foreach (var identifiant in noeuds.Keys)
            {
                distances[identifiant] = double.PositiveInfinity;
            }
            distances[depart] = 0;

            while (visites.Count < noeuds.Count)
            {
                int noeudActuel = -1;
                double distanceMinimale = double.PositiveInfinity;

                foreach (var identifiant in noeuds.Keys)
                {
                    if (visites.Contains(identifiant) != true && distances[identifiant] < distanceMinimale)
                    {
                        distanceMinimale = distances[identifiant];
                        noeudActuel = identifiant;
                    }
                }

                if (noeudActuel == -1 || noeudActuel == arrivee)
                {
                    break;
                }

                visites.Add(noeudActuel);

                foreach (var lien in noeuds[noeudActuel].Liens)
                {
                    int voisin = lien.Destination.Id;
                    if (visites.Contains(voisin) != true)
                    {
                        double nouvelleDistance = distances[noeudActuel] + lien.Poids;
                        if (nouvelleDistance < distances[voisin])
                        {
                            distances[voisin] = nouvelleDistance;
                            precedent[voisin] = noeudActuel;
                        }
                    }
                }
            }

            // Reconstruction du chemin
            List<int> chemin = new List<int>();
            int actuel = arrivee;

            while (actuel != depart && precedent.ContainsKey(actuel))
            {
                chemin.Add(actuel);
                actuel = precedent[actuel];
            }

            chemin.Add(depart);
            chemin.Reverse();

            string cheminImage = "chemin_" + depart + "_vers_" + arrivee + ".png";
            DessinerChemin(chemin, cheminImage);

            // Affichage console
            Console.WriteLine("\nItinéraire :");

            if (chemin.Count == 1)
            {
                Console.WriteLine("Vous êtes déjà à la station de destination.");
                return 0;
            }

            Console.WriteLine("Départ de " + noeuds[depart].Nom + " (Ligne " + LignesStations[depart] + ")");

            string ligneActuelle = LignesStations[depart];
            double tempsSegment = 0;
            double tempsTotal = 0;

            for (int i = 1; i < chemin.Count; i++)
            {
                int depuis = chemin[i - 1];
                int vers = chemin[i];

                var lien = noeuds[depuis].Liens.First(e => e.Destination.Id == vers);
                string ligneSuivante = LignesStations[vers];
                tempsSegment += lien.Poids;

                bool estDerniereEtape = (i == chemin.Count - 1);
                if (ligneSuivante != ligneActuelle || estDerniereEtape)
                {
                    Console.WriteLine("Ligne empruntée : " + ligneActuelle + " (" + Math.Round(tempsSegment) + " min)");
                    tempsTotal += tempsSegment;

                    if (estDerniereEtape != true && ligneSuivante != ligneActuelle)
                    {
                        Console.WriteLine("Changement à " + noeuds[vers].Nom + " (+" + Math.Round(lien.Poids) + " min)");
                        ligneActuelle = ligneSuivante;
                        tempsSegment = 0;
                    }
                }
            }

            Console.WriteLine("Arrivée à " + noeuds[chemin[chemin.Count - 1]].Nom + " en " + Math.Round(tempsTotal) + " minutes");

            return distances[arrivee];
        }




        public void DessinerChemin(List<int> chemin, string outputPath)
        {
            if (chemin == null || chemin.Count == 0) return;

            const int nodeRadius = 10;
            const int imageWidth = 1200;
            const int imageHeight = 800;
            const int margin = 50;

            using (var bitmap = new Bitmap(imageWidth, imageHeight))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Positionnement des nœuds
                var nodePositions = new Dictionary<int, PointF>();
                float stepX = (imageWidth - 2 * margin) / (float)(chemin.Count - 1);

                for (int i = 0; i < chemin.Count; i++)
                {
                    float x = margin + i * stepX;
                    float y = imageHeight / 2;
                    nodePositions[chemin[i]] = new PointF(x, y);
                }

                // Dessin des liens
                using (var edgePen = new Pen(Color.Blue, 3))
                {
                    for (int i = 0; i < chemin.Count - 1; i++)
                    {
                        int from = chemin[i];
                        int to = chemin[i + 1];

                        graphics.DrawLine(edgePen, nodePositions[from], nodePositions[to]);

                        // Affichage du temps
                        var lien = noeuds[from].Liens.First(l => l.Destination.Id == to);
                        PointF middle = new PointF(
                            (nodePositions[from].X + nodePositions[to].X) / 2,
                            (nodePositions[from].Y + nodePositions[to].Y) / 2 - 15);

                        graphics.DrawString($"{lien.Poids} min",
                                         new Font("Arial", 8),
                                         Brushes.Black,
                                         middle);
                    }
                }

                // Dessin des nœuds
                using (var nodeBrush = new SolidBrush(Color.Red))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    foreach (var kvp in nodePositions)
                    {
                        graphics.FillEllipse(nodeBrush,
                                           kvp.Value.X - nodeRadius,
                                           kvp.Value.Y - nodeRadius,
                                           nodeRadius * 2,
                                           nodeRadius * 2);

                        string nom = noeuds[kvp.Key].Nom;
                        graphics.DrawString(nom,
                                           new Font("Arial", 8),
                                           textBrush,
                                           kvp.Value.X - nodeRadius,
                                           kvp.Value.Y + nodeRadius + 5);

                        graphics.DrawString(LignesStations[kvp.Key],
                                          new Font("Arial", 7, FontStyle.Bold),
                                          Brushes.Green,
                                          kvp.Value.X - nodeRadius,
                                          kvp.Value.Y + nodeRadius + 20);
                    }
                }

                bitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine($"\nGraphe du chemin généré : {outputPath}");
            }
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
            if (idToIndex.ContainsKey(source) != true || idToIndex.ContainsKey(cible) != true)
            {
                return -1;
            }

            double resultat = dist[idToIndex[source], idToIndex[cible]];

            if (resultat == -1)
            {
                return -1;
            }
            else
            {
                return resultat;
            }

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

            Console.WriteLine("Graphe généré dans " + outputPath + ")");
        }

        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public void AfficherCheminDansFenetre(string cheminImage)
        {
            if (File.Exists(cheminImage) != true)
            {
                Console.WriteLine("Le fichier image '" + cheminImage + "' est introuvable.");
                return;
            }

            Form fenetre = new Form
            {
                Text = "Itinéraire du chemin le plus court",
                Width = 1300,
                Height = 900,
                StartPosition = FormStartPosition.CenterScreen
            };

            PictureBox imageBox = new PictureBox
            {
                Image = Image.FromFile(cheminImage),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            fenetre.Controls.Add(imageBox);

            System.Windows.Forms.Application.Run(fenetre);
        }

        public int LireSommetValide(string message)
        {
            int valeur = -1;
            string saisie = "";
            while (valeur < 1 || valeur > 332)
            {
                Console.Write(message);
                saisie = Console.ReadLine();
                valeur = Convert.ToInt32(saisie);

                if (valeur < 1 || valeur > 332)
                {
                    Console.WriteLine("Sommet invalide. Veuillez entrer un ID compris entre 1 et 332.");
                }

            }
            return valeur;

        }


    }
}
