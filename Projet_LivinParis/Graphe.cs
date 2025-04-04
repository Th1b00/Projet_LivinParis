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
using System.Drawing.Drawing2D;

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

                // 1. Créer tous les noeuds avec coordonnées et ligne
                foreach (DataRow row in tableNoeuds.Rows)
                {
                    int id = Convert.ToInt32(row["ID Station"]);
                    string nom = row["Libelle station"].ToString().Trim();
                    float longitude = float.Parse(row["Longitude"].ToString());
                    float latitude = float.Parse(row["Latitude"].ToString());
                    string ligne = row["Libelle Line"].ToString().Trim();

                    var noeud = new Noeud<T>(id)
                    {
                        Nom = nom,
                        X = longitude,
                        Y = latitude
                    };
                    noeuds[id] = noeud;
                    lignesStations[id] = ligne;
                    



                    if (idParStation.ContainsKey(nom) == false)
                    {
                        idParStation[nom] = new List<int>();
                        
                    }
                    idParStation[nom].Add(id);

                }

                
                foreach (DataRow row in tableArcs.Rows)
                {
                    int id = Convert.ToInt32(row["Station Id"]);
                    double precedent = row["Précédent"] != DBNull.Value ? Convert.ToDouble(row["Précédent"]) : -1;
                    double suivant = row["Suivant"] != DBNull.Value ? Convert.ToDouble(row["Suivant"]) : -1;
                    double temps = row["Temps entre 2 stations"] != DBNull.Value ? Convert.ToDouble(row["Temps entre 2 stations"]) : 1.0;

                    if (precedent != -1 && noeuds.ContainsKey((int)precedent)==true && noeuds.ContainsKey(id)==true)
                    {
                        AjouterLien((int)precedent, id, temps);
                    }
                        
                    if (suivant != -1 && noeuds.ContainsKey(id) == true && noeuds.ContainsKey((int)suivant) == true)
                    {
                        AjouterLien(id, (int)suivant, temps);
                    }
                        
                }

                // 3. Ajouter les correspondances
                var dejaLie = new HashSet<(int, int)>();
                foreach (DataRow row in tableArcs.Rows)
                {
                    int id = Convert.ToInt32(row["Station Id"]);
                    string nom = row["Station"].ToString().Trim();

                    if (row["Temps de Changement"] != DBNull.Value)
                    {
                        double temps = Convert.ToDouble(row["Temps de Changement"]);

                        if (idParStation.ContainsKey(nom) == true)
                        {
                            foreach (int autreId in idParStation[nom])
                            {
                                if (autreId != id)
                                {
                                    var cle = (Math.Min(id, autreId), Math.Max(id, autreId));
                                    if (dejaLie.Contains(cle) == false)
                                    {
                                        AjouterLien(id, autreId, temps);
                                        dejaLie.Add(cle);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        

        public double Dijkstra(int depart, int arrivee)
        {
            var distances = new Dictionary<int, double>();
            var precedent = new Dictionary<int, int>();
            var visites = new HashSet<int>();

            foreach (var identifiant in noeuds.Keys)
            {
                distances[identifiant] = double.MaxValue;
            }
            distances[depart] = 0;

            while (visites.Count < noeuds.Count)
            {
                int noeudActuel = -1;
                double distanceMinimale = double.MaxValue;

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




       




        public double BellmanFord(int source, int cible)
        {
            var distances = new Dictionary<int, double>();

            // Initialisation : toutes les distances à l'infini, sauf la source à 0
            foreach (var noeud in noeuds.Keys)
            {
                distances[noeud] = double.MaxValue;
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

        public void AfficherGrapheGeo(string outputPath = "plan_metro_geo.png")
        {
            const int imageWidth = 3000;
            const int imageHeight = 2250;
            const int nodeRadius = 8;
            const int margin = 60;

            float minX = noeuds.Values.Min(n => n.X);
            float maxX = noeuds.Values.Max(n => n.X);
            float minY = noeuds.Values.Min(n => n.Y);
            float maxY = noeuds.Values.Max(n => n.Y);

            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;

            float zoomX = 15000f;
            float zoomY = 20000f;

            Bitmap bitmap = new Bitmap(imageWidth, imageHeight);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font font = new Font("Arial", 12, FontStyle.Bold);

            Dictionary<string, Color> couleursLignes = new Dictionary<string, Color>();
            Color[] palette = new Color[] {
        Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
        Color.Brown, Color.Teal, Color.DarkCyan, Color.DeepPink,
        Color.Gold, Color.DarkGreen, Color.Black, Color.Magenta
    };
            int index = 0;
            foreach (string ligne in lignesStations.Values.Distinct())
            {
                couleursLignes[ligne] = palette[index % palette.Length];
                index++;
            }

            Dictionary<int, PointF> positions = new Dictionary<int, PointF>();
            foreach (var n in noeuds.Values)
            {
                float x = imageWidth / 2 + (n.X - centerX) * zoomX;
                float y = imageHeight / 2 - (n.Y - centerY) * zoomY;
                positions[n.Id] = new PointF(x, y);
            }

            foreach (var noeud in noeuds.Values)
            {
                PointF start = positions[noeud.Id];
                foreach (var lien in noeud.Liens)
                {
                    PointF end = positions[lien.Destination.Id];
                    string ligne = lignesStations.ContainsKey(noeud.Id) ? lignesStations[noeud.Id] : "Inconnue";
                    Color couleur = couleursLignes.ContainsKey(ligne) ? couleursLignes[ligne] : Color.Gray;
                    using (Pen pen = new Pen(couleur, 5))
                    {
                        g.DrawLine(pen, start, end);
                    }
                }
            }

            foreach (var kvp in positions)
            {
                PointF pos = kvp.Value;
                g.FillEllipse(Brushes.Black, pos.X - nodeRadius, pos.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                string nom = noeuds[kvp.Key].Nom;
                if (!string.IsNullOrWhiteSpace(nom))
                {
                    g.DrawString(nom, font, Brushes.Black, pos.X + 6, pos.Y - 6);
                }
            }

            // Légende
            int legX = imageWidth - 300;
            int legY = 80;
            Font legFont = new Font("Arial", 11);
            g.FillRectangle(Brushes.White, legX - 10, legY - 30, 280, 30 + 22 * couleursLignes.Count);
            g.DrawRectangle(Pens.Black, legX - 10, legY - 30, 280, 30 + 22 * couleursLignes.Count);
            g.DrawString("Lignes du métro parisien :", new Font("Arial", 13, FontStyle.Bold), Brushes.Black, legX, legY - 30);

            int dy = 0;
            foreach (var kvp in couleursLignes)
            {
                using (Brush b = new SolidBrush(kvp.Value))
                {
                    g.FillRectangle(b, legX, legY + dy, 25, 12);
                }
                g.DrawString(kvp.Key, legFont, Brushes.Black, legX + 35, legY + dy - 2);
                dy += 22;
            }

            bitmap.Save(outputPath, ImageFormat.Png);
            Console.WriteLine("✅ Plan du métro géographique enregistré sous : " + outputPath);

            if (File.Exists(outputPath))
            {
                Form fenetre = new Form();
                fenetre.Text = "Plan du métro parisien (projection GPS)";
                fenetre.Width = imageWidth + 50;
                fenetre.Height = imageHeight + 50;
                fenetre.StartPosition = FormStartPosition.CenterScreen;

                PictureBox imageBox = new PictureBox();
                imageBox.Image = Image.FromFile(outputPath);
                imageBox.SizeMode = PictureBoxSizeMode.Zoom;
                imageBox.Dock = DockStyle.Fill;

                fenetre.Controls.Add(imageBox);
                Application.Run(fenetre);
            }
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

                // Dessin des nœuds avec décalage dynamique des labels
                using (var nodeBrush = new SolidBrush(Color.Red))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    List<RectangleF> zonesTextes = new List<RectangleF>();
                    Font nomFont = new Font("Arial", 8);
                    Font ligneFont = new Font("Arial", 7, FontStyle.Bold);

                    foreach (var kvp in nodePositions)
                    {
                        PointF pos = kvp.Value;
                        graphics.FillEllipse(nodeBrush, pos.X - nodeRadius, pos.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                        string nom = noeuds[kvp.Key].Nom;
                        string ligne = LignesStations[kvp.Key];

                        // Calcule une position verticale disponible
                        float labelY = pos.Y + nodeRadius + 5;
                        float labelX = pos.X - nodeRadius;

                        // Évite le chevauchement
                        RectangleF zoneTexte = new RectangleF(labelX, labelY, 100, 14);
                        int decalage = 0;
                        while (zonesTextes.Any(z => z.IntersectsWith(zoneTexte)))
                        {
                            decalage += 12;
                            zoneTexte.Y += 12;
                        }
                        zonesTextes.Add(zoneTexte);

                        // Affiche le nom et la ligne sans chevauchement
                        graphics.DrawString(nom, nomFont, textBrush, labelX, zoneTexte.Y);
                        graphics.DrawString(ligne, ligneFont, Brushes.Green, labelX, zoneTexte.Y + 12);
                    }
                }

                bitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine($"\n✅ Graphe du chemin généré : {outputPath}");
            }
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
