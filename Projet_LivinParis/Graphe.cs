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
using System.Text.Json;
using System.Xml.Serialization;

namespace PROJET_PSI
{
    /// <summary>
    /// Classe représentant un graphe orienté pondéré pour modéliser un réseau de métro.
    /// </summary>
    /// <typeparam name="T">Type de données que chaque noeud peut contenir.</typeparam>
    public class Graphe<T>
    {
        private Dictionary<int, Noeud<T>> noeuds = new Dictionary<int, Noeud<T>>();
        private Dictionary<int, string> lignesStations = new Dictionary<int, string>();

        /// <summary>
        /// Dictionnaire des noeuds du graphe.
        /// </summary>
        public Dictionary<int, Noeud<T>> Noeuds
        {
            get { return noeuds; }
            set { noeuds = value; }
        }

        /// <summary>
        /// Dictionnaire associant chaque ID de station à sa ligne de métro.
        /// </summary>
        public Dictionary<int, string> LignesStations
        {
            get { return lignesStations; }
            set { lignesStations = value; }
        }


        /// <summary>
        /// Ajoute un nœud au graphe s’il n’existe pas déjà.
        /// </summary>
        public void AjouterNoeud(int id)
        {
            if (noeuds.ContainsKey(id) != true)
            {
                noeuds[id] = new Noeud<T>(id);
            }
        }

        /// <summary>
        /// Vérifie s’il existe un lien entre deux nœuds du graphe.
        /// </summary>
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

        /// <summary>
        /// Ajoute un lien entre deux noeuds avec un poids donné.
        /// </summary>
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

        /// <summary>
        /// Charge les données du graphe à partir d’un fichier Excel.
        /// </summary>
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

                /// Créer tous les noeuds avec coordonnées et ligne
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

                    if (idParStation.ContainsKey(nom) != true)
                    {
                        idParStation[nom] = new List<int>();
                    }
                    idParStation[nom].Add(id);
                }

                /// Créer les liens précédent/suivant
                foreach (DataRow row in tableArcs.Rows)
                {
                    int id = Convert.ToInt32(row["Station Id"]);
                    double precedent = row["Précédent"] != DBNull.Value ? Convert.ToDouble(row["Précédent"]) : -1;
                    double suivant = row["Suivant"] != DBNull.Value ? Convert.ToDouble(row["Suivant"]) : -1;
                    double temps = row["Temps entre 2 stations"] != DBNull.Value ? Convert.ToDouble(row["Temps entre 2 stations"]) : 1.0;

                    if (precedent != -1 && noeuds.ContainsKey((int)precedent) == true && noeuds.ContainsKey(id) == true)
                    {
                        AjouterLien((int)precedent, id, temps);
                    }

                    if (suivant != -1 && noeuds.ContainsKey(id) == true && noeuds.ContainsKey((int)suivant) == true)
                    {
                        AjouterLien(id, (int)suivant, temps);
                    }
                }

                /// Ajouter les correspondances entre stations de même nom
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
                                    if (dejaLie.Contains(cle) != true)
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



        /// <summary>
        /// Calcule le chemin le plus court entre deux stations avec l’algorithme de Dijkstra.
        /// Affiche l’itinéraire et génère une image du trajet.
        /// </summary>
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

            Console.WriteLine("\nItinéraire de livraison :");
            Console.WriteLine();

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
                if (ligneSuivante != ligneActuelle || estDerniereEtape == true)
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

            return distances[arrivee];
        }

        /// <summary>
        /// Calcule les plus courtes distances entre toutes les paires avec l’algorithme de Floyd-Warshall.
        /// </summary>
        public double FloydWarshall(int source, int cible)
        {
            var idList = noeuds.Keys.ToList();
            var idVersIndice = new Dictionary<int, int>();
            var indiceVersId = new Dictionary<int, int>();

            for (int i = 0; i < idList.Count; i++)
            {
                idVersIndice[idList[i]] = i;
                indiceVersId[i] = idList[i];
            }

            int n = idList.Count;
            double[,] dist = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        dist[i, j] = 0;
                    }
                    else
                    {
                        dist[i, j] = double.MaxValue;
                    }
                }
            }

            foreach (var noeud in noeuds)
            {
                int i = idVersIndice[noeud.Key];
                foreach (var lien in noeud.Value.Liens)
                {
                    int j = idVersIndice[lien.Destination.Id];
                    dist[i, j] = lien.Poids;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != double.MaxValue && dist[k, j] != double.MaxValue)
                        {
                            double nouvelleDistance = dist[i, k] + dist[k, j];
                            if (nouvelleDistance < dist[i, j])
                            {
                                dist[i, j] = nouvelleDistance;
                            }
                        }
                    }
                }
            }

            if (idVersIndice.ContainsKey(source) != true || idVersIndice.ContainsKey(cible) != true)
            {
                return -1;
            }

            double resultat = dist[idVersIndice[source], idVersIndice[cible]];

            if (resultat == double.MaxValue)
            {
                return -1;
            }
            else
            {
                return resultat;
            }
        }

        /// <summary>
        /// Affiche une carte géographique du graphe avec des lignes colorées.
        /// </summary>
        public void AfficherGrapheGeo(string cheminFichier = "plan_metro_geo.png")
        {
            const int largeur = 3000;
            const int hauteur = 2250;
            const int rayon = 8;

            float minX = noeuds.Values.Min(n => n.X);
            float maxX = noeuds.Values.Max(n => n.X);
            float minY = noeuds.Values.Min(n => n.Y);
            float maxY = noeuds.Values.Max(n => n.Y);

            float centreX = (minX + maxX) / 2;
            float centreY = (minY + maxY) / 2;

            float zoomX = 15000f;
            float zoomY = 20000f;

            var bitmap = new Bitmap(largeur, hauteur);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var police = new Font("Arial", 12, FontStyle.Bold);

            var couleursLignes = new Dictionary<string, Color>();
            var palette = new Color[]
            {
        Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
        Color.Brown, Color.Teal, Color.DarkCyan, Color.DeepPink,
        Color.Gold, Color.DarkGreen, Color.Black, Color.Magenta
            };

            int index = 0;
            foreach (var ligne in lignesStations.Values.Distinct())
            {
                couleursLignes[ligne] = palette[index % palette.Length];
                index++;
            }

            var positions = new Dictionary<int, PointF>();
            foreach (var n in noeuds.Values)
            {
                float x = largeur / 2 + (n.X - centreX) * zoomX;
                float y = hauteur / 2 - (n.Y - centreY) * zoomY;
                positions[n.Id] = new PointF(x, y);
            }

            foreach (var noeud in noeuds.Values)
            {
                PointF p1 = positions[noeud.Id];
                foreach (var lien in noeud.Liens)
                {
                    PointF p2 = positions[lien.Destination.Id];
                    string ligne = lignesStations.ContainsKey(noeud.Id) ? lignesStations[noeud.Id] : "Inconnue";
                    Color couleur = couleursLignes.ContainsKey(ligne) ? couleursLignes[ligne] : Color.Gray;

                    using (var stylo = new Pen(couleur, 5))
                    {
                        g.DrawLine(stylo, p1, p2);
                    }
                }
            }

            foreach (var kvp in positions)
            {
                PointF p = kvp.Value;
                g.FillEllipse(Brushes.Black, p.X - rayon, p.Y - rayon, rayon * 2, rayon * 2);

                string nom = noeuds[kvp.Key].Nom;
                if (string.IsNullOrWhiteSpace(nom) != true)
                {
                    g.DrawString(nom, police, Brushes.Black, p.X + 6, p.Y - 6);
                }
            }

            int legendeX = largeur - 300;
            int legendeY = 80;
            Font policeLegende = new Font("Arial", 11);

            g.FillRectangle(Brushes.White, legendeX - 10, legendeY - 30, 280, 30 + 22 * couleursLignes.Count);
            g.DrawRectangle(Pens.Black, legendeX - 10, legendeY - 30, 280, 30 + 22 * couleursLignes.Count);
            g.DrawString("Lignes du métro parisien :", new Font("Arial", 13, FontStyle.Bold), Brushes.Black, legendeX, legendeY - 30);

            int dy = 0;
            foreach (var kvp in couleursLignes)
            {
                using (var pinceau = new SolidBrush(kvp.Value))
                {
                    g.FillRectangle(pinceau, legendeX, legendeY + dy, 25, 12);
                }
                g.DrawString(kvp.Key, policeLegende, Brushes.Black, legendeX + 35, legendeY + dy - 2);
                dy += 22;
            }

            bitmap.Save(cheminFichier, ImageFormat.Png);
            Console.WriteLine("Plan du métro géographique enregistré sous : " + cheminFichier);

            if (File.Exists(cheminFichier))
            {
                var fenetre = new Form();
                fenetre.Text = "Plan du métro parisien (projection GPS)";
                fenetre.Width = largeur + 50;
                fenetre.Height = hauteur + 50;
                fenetre.StartPosition = FormStartPosition.CenterScreen;

                var box = new PictureBox();
                box.Image = Image.FromFile(cheminFichier);
                box.SizeMode = PictureBoxSizeMode.Zoom;
                box.Dock = DockStyle.Fill;

                fenetre.Controls.Add(box);
                Application.Run(fenetre);
            }
        }

        /// <summary>
        /// Dessine une image illustrant un chemin entre plusieurs stations.
        /// </summary>
        public void DessinerChemin(List<int> chemin, string cheminFichier)
        {
            if (chemin == null || chemin.Count == 0)
            {
                return;
            }

            const int largeur = 1200;
            const int hauteur = 800;
            const int rayon = 10;
            const int marge = 50;

            using (var bitmap = new Bitmap(largeur, hauteur))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                var positions = new Dictionary<int, PointF>();
                float stepX = (largeur - 2 * marge) / (float)(chemin.Count - 1);

                for (int i = 0; i < chemin.Count; i++)
                {
                    float x = marge + i * stepX;
                    float y = hauteur / 2;
                    positions[chemin[i]] = new PointF(x, y);
                }

                using (var stylo = new Pen(Color.Blue, 3))
                {
                    for (int i = 0; i < chemin.Count - 1; i++)
                    {
                        int id1 = chemin[i];
                        int id2 = chemin[i + 1];

                        g.DrawLine(stylo, positions[id1], positions[id2]);

                        var lien = noeuds[id1].Liens.First(l => l.Destination.Id == id2);
                        PointF milieu = new PointF(
                            (positions[id1].X + positions[id2].X) / 2,
                            (positions[id1].Y + positions[id2].Y) / 2 - 15
                        );

                        g.DrawString(lien.Poids + " min", new Font("Arial", 8), Brushes.Black, milieu);
                    }
                }

                using (var pinceau = new SolidBrush(Color.Red))
                using (var pinceauTexte = new SolidBrush(Color.Black))
                {
                    List<RectangleF> zones = new List<RectangleF>();
                    var fontNom = new Font("Arial", 8);
                    var fontLigne = new Font("Arial", 7, FontStyle.Bold);

                    foreach (var kvp in positions)
                    {
                        PointF pos = kvp.Value;
                        g.FillEllipse(pinceau, pos.X - rayon, pos.Y - rayon, rayon * 2, rayon * 2);

                        string nom = noeuds[kvp.Key].Nom;
                        string ligne = lignesStations[kvp.Key];

                        float labelX = pos.X - rayon;
                        float labelY = pos.Y + rayon + 5;
                        RectangleF zone = new RectangleF(labelX, labelY, 100, 14);

                        int decalage = 0;
                        while (zones.Any(z => z.IntersectsWith(zone)) == true)
                        {
                            decalage += 12;
                            zone.Y += 12;
                        }

                        zones.Add(zone);

                        g.DrawString(nom, fontNom, pinceauTexte, labelX, zone.Y);
                        g.DrawString(ligne, fontLigne, Brushes.Green, labelX, zone.Y + 12);
                    }
                }

                bitmap.Save(cheminFichier, ImageFormat.Png);
                Console.WriteLine("\nGraphe du chemin généré : " + cheminFichier);
            }
        }

        /// <summary>
        /// Affiche une image du chemin généré dans une fenêtre.
        /// </summary>
        public void AfficherCheminDansFenetre(string cheminImage)
        {
            if (File.Exists(cheminImage) != true)
            {
                Console.WriteLine("Le fichier image '" + cheminImage + "' est introuvable.");
                return;
            }

            Form fenetre = new Form();
            fenetre.Text = "Itinéraire du chemin le plus court";
            fenetre.Width = 1300;
            fenetre.Height = 900;
            fenetre.StartPosition = FormStartPosition.CenterScreen;

            PictureBox imageBox = new PictureBox();
            imageBox.Image = Image.FromFile(cheminImage);
            imageBox.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox.Dock = DockStyle.Fill;

            fenetre.Controls.Add(imageBox);

            Application.Run(fenetre);
        }

        /// <summary>
        /// Demande à l’utilisateur de saisir un ID de station valide (entre 1 et 332).
        /// </summary>
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

        /// <summary>
        /// Recherche l'ID d'un sommet à partir de son nom.
        /// </summary>
        public int GetIdSommetParNom(string nomStation)
        {
            foreach (var kvp in noeuds)
            {
                string nomNoeud = kvp.Value.Nom.Trim().ToLowerInvariant();
                string nomRecherche = nomStation.Trim().ToLowerInvariant();

                if (nomNoeud == nomRecherche)
                {
                    return kvp.Key;
                }

                if (nomNoeud.Replace("é", "e").Replace("è", "e").Replace("ê", "e") ==
                    nomRecherche.Replace("é", "e").Replace("è", "e").Replace("ê", "e"))
                {
                    return kvp.Key;
                }

                if (nomNoeud.Contains(nomRecherche) == true || nomRecherche.Contains(nomNoeud) == true)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        /// <summary>
        /// Coloration Welsh-Powell.
        /// </summary>
        public Dictionary<int, int> ColorationWelshPowell()
        {
            Dictionary<string, List<int>> groupesStations = new Dictionary<string, List<int>>();
            foreach (KeyValuePair<int, Noeud<T>> station in noeuds)
            {
                string nomNormalise = station.Value.Nom.Trim().ToLowerInvariant();

                if (groupesStations.ContainsKey(nomNormalise) != true)
                {
                    groupesStations[nomNormalise] = new List<int>();
                }

                groupesStations[nomNormalise].Add(station.Key);
            }

            Dictionary<string, HashSet<string>> voisinsFusionnes = new Dictionary<string, HashSet<string>>();
            foreach (KeyValuePair<string, List<int>> groupe in groupesStations)
            {
                voisinsFusionnes[groupe.Key] = new HashSet<string>();

                foreach (int id in groupe.Value)
                {
                    foreach (Lien<T> lien in noeuds[id].Liens)
                    {
                        string nomVoisin = lien.Destination.Nom.Trim().ToLowerInvariant();

                        if (nomVoisin != groupe.Key)
                        {
                            voisinsFusionnes[groupe.Key].Add(nomVoisin);
                        }
                    }
                }
            }

            Dictionary<string, int> couleursParNom = new Dictionary<string, int>();
            List<string> nomsTries = new List<string>(voisinsFusionnes.Keys);
            nomsTries.Sort(delegate (string a, string b)
            {
                return voisinsFusionnes[b].Count.CompareTo(voisinsFusionnes[a].Count);
            });

            foreach (string nom in nomsTries)
            {
                HashSet<int> couleursInterdites = new HashSet<int>();

                foreach (string voisin in voisinsFusionnes[nom])
                {
                    if (couleursParNom.ContainsKey(voisin) == true)
                    {
                        couleursInterdites.Add(couleursParNom[voisin]);
                    }
                }

                int couleur = 0;
                while (couleursInterdites.Contains(couleur) == true)
                {
                    couleur = couleur + 1;
                }

                couleursParNom[nom] = couleur;
            }

            Dictionary<int, int> couleursFinales = new Dictionary<int, int>();
            foreach (KeyValuePair<string, List<int>> groupe in groupesStations)
            {
                foreach (int id in groupe.Value)
                {
                    couleursFinales[id] = couleursParNom[groupe.Key];
                }
            }

            return couleursFinales;
        }

        /// <summary>
        /// Vérifie si le graphe est biparti à partir de la coloration Welsh-Powell.
        /// </summary>
        public bool EstBiparti()
        {
            Dictionary<int, int> couleurs = ColorationWelshPowell();

            List<int> couleursUtilisées = new List<int>();

            foreach (int c in couleurs.Values)
            {
                if (couleursUtilisées.Contains(c) != true)
                {
                    couleursUtilisées.Add(c);
                }
            }

            if (couleursUtilisées.Count < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie si le graphe est planaire.
        /// </summary>
        public bool EstPlanaire()
        {
            int n = noeuds.Count;
            int totalDegres = 0;

            foreach (var noeud in noeuds.Values)
            {
                totalDegres = totalDegres + noeud.Liens.Count;
            }

            int m = totalDegres / 2;

            if (m <= 3 * n - 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Exporte le graphe au format JSON dans le fichier spécifié.
        /// </summary>
        public void ExportJson(string cheminFichier)
        {
            var donnees = new List<Dictionary<string, object>>();

            foreach (var station in noeuds.Values)
            {
                var listeLiens = new List<Dictionary<string, object>>();

                foreach (var lien in station.Liens)
                {
                    var dicoLien = new Dictionary<string, object>();
                    dicoLien["Destination"] = lien.Destination.Id;
                    dicoLien["Poids"] = lien.Poids;
                    listeLiens.Add(dicoLien);
                }

                var info = new Dictionary<string, object>();
                info["Id"] = station.Id;
                info["Nom"] = station.Nom;
                info["X"] = station.X;
                info["Y"] = station.Y;

                if (lignesStations.ContainsKey(station.Id) == true)
                {
                    info["Ligne"] = lignesStations[station.Id];
                }
                else
                {
                    info["Ligne"] = null;
                }

                info["Liens"] = listeLiens;

                donnees.Add(info);
            }

            var options = new JsonSerializerOptions();
            options.WriteIndented = true;

            string contenu = JsonSerializer.Serialize(donnees, options);
            File.WriteAllText(cheminFichier, contenu);
        }

        /// <summary>
        /// Exporte le graphe au format XML dans le fichier spécifié.
        /// </summary>
        public void ExportXml(string cheminFichier)
        {
            var export = new ListeNoeuds();
            export.Noeuds = new List<NoeudDto>();

            foreach (var n in noeuds.Values)
            {
                var noeudDto = new NoeudDto();
                noeudDto.Identifiant = n.Id;
                noeudDto.Nom = n.Nom;
                noeudDto.X = n.X;
                noeudDto.Y = n.Y;

                if (lignesStations.ContainsKey(n.Id) == true)
                {
                    noeudDto.Ligne = lignesStations[n.Id];
                }
                else
                {
                    noeudDto.Ligne = null;
                }

                noeudDto.Liens = new List<LienDto>();

                foreach (var lien in n.Liens)
                {
                    var lienDto = new LienDto();
                    lienDto.Destination = lien.Destination.Id;
                    lienDto.Poids = lien.Poids;
                    noeudDto.Liens.Add(lienDto);
                }

                export.Noeuds.Add(noeudDto);
            }

            var serializer = new XmlSerializer(typeof(ListeNoeuds));
            using (var writer = new StreamWriter(cheminFichier))
            {
                serializer.Serialize(writer, export);
            }
        }

        /// <summary>
        /// Importe un graphe depuis un fichier JSON.
        /// </summary>
        public void ImportJson(string cheminFichier)
        {
            var contenu = File.ReadAllText(cheminFichier);
            var stations = JsonSerializer.Deserialize<List<NoeudJson>>(contenu);

            noeuds.Clear();
            lignesStations.Clear();

            foreach (var s in stations)
            {
                var noeud = new Noeud<T>(s.Id);
                noeud.Nom = s.Nom;
                noeud.X = (float)s.X;
                noeud.Y = (float)s.Y;
                noeuds[s.Id] = noeud;

                if (s.Ligne != null)
                {
                    lignesStations[s.Id] = s.Ligne;
                }
            }

            foreach (var s in stations)
            {
                foreach (var lien in s.Liens)
                {
                    var destination = noeuds.ContainsKey(lien.Destination) == true ? noeuds[lien.Destination] : null;
                    if (destination != null)
                    {
                        noeuds[s.Id].Liens.Add(new Lien<T>(destination, lien.Poids));
                    }
                }
            }
        }

        /// <summary>
        /// Importe un graphe depuis un fichier XML.
        /// </summary>
        public void ImportXml(string cheminFichier)
        {
            var serializer = new XmlSerializer(typeof(ListeNoeuds));

            using (var reader = new StreamReader(cheminFichier))
            {
                var data = (ListeNoeuds)serializer.Deserialize(reader);

                noeuds.Clear();
                lignesStations.Clear();

                foreach (var noeud in data.Noeuds)
                {
                    var n = new Noeud<T>(noeud.Identifiant);
                    n.Nom = noeud.Nom;
                    n.X = (float)noeud.X;
                    n.Y = (float)noeud.Y;
                    noeuds[noeud.Identifiant] = n;

                    if (noeud.Ligne != null)
                    {
                        lignesStations[noeud.Identifiant] = noeud.Ligne;
                    }
                }

                foreach (var noeud in data.Noeuds)
                {
                    foreach (var lien in noeud.Liens)
                    {
                        if (noeuds.ContainsKey(lien.Destination) == true)
                        {
                            noeuds[noeud.Identifiant].Liens.Add(new Lien<T>(noeuds[lien.Destination], lien.Poids));
                        }
                    }
                }
            }
        }

        private class NoeudJson
        {
            private int id;
            private string nom;
            private double x;
            private double y;
            private string ligne;
            private List<LienJson> liens;

            public int Id
            {
                get { return id; }
                set { id = value; }
            }

            public string Nom
            {
                get { return nom; }
                set { nom = value; }
            }

            public double X
            {
                get { return x; }
                set { x = value; }
            }

            public double Y
            {
                get { return y; }
                set { y = value; }
            }

            public string Ligne
            {
                get { return ligne; }
                set { ligne = value; }
            }

            public List<LienJson> Liens
            {
                get { return liens; }
                set { liens = value; }
            }
        }

        private class LienJson
        {
            private int destination;
            private double poids;

            public int Destination
            {
                get { return destination; }
                set { destination = value; }
            }

            public double Poids
            {
                get { return poids; }
                set { poids = value; }
            }
        }

        [XmlRoot("Noeuds")]
        public class ListeNoeuds
        {
            private List<NoeudDto> noeuds;

            [XmlElement("Noeud")]
            public List<NoeudDto> Noeuds
            {
                get { return noeuds; }
                set { noeuds = value; }
            }
        }

        public class NoeudDto
        {
            private int identifiant;
            private string nom;
            private double x;
            private double y;
            private string ligne;
            private List<LienDto> liens;

            [XmlAttribute]
            public int Identifiant
            {
                get { return identifiant; }
                set { identifiant = value; }
            }

            [XmlAttribute]
            public string Nom
            {
                get { return nom; }
                set { nom = value; }
            }

            [XmlAttribute]
            public double X
            {
                get { return x; }
                set { x = value; }
            }

            [XmlAttribute]
            public double Y
            {
                get { return y; }
                set { y = value; }
            }

            [XmlAttribute]
            public string Ligne
            {
                get { return ligne; }
                set { ligne = value; }
            }

            [XmlArray("Liens")]
            [XmlArrayItem("Lien")]
            public List<LienDto> Liens
            {
                get { return liens; }
                set { liens = value; }
            }
        }

        public class LienDto
        {
            private int destination;
            private double poids;

            [XmlAttribute]
            public int Destination
            {
                get { return destination; }
                set { destination = value; }
            }

            [XmlAttribute]
            public double Poids
            {
                get { return poids; }
                set { poids = value; }
            }
        }

        /// <summary>
        /// Affiche un graphe des commandes entre clients et cuisiniers.
        /// Chaque utilisateur est affiché à la station indiquée, en bleu pour les clients et rouge pour les cuisiniers.
        /// Les arêtes du graphe sont les connexions du métro.
        /// </summary>
        /// <param name="commandes">Liste des paires (idClient, idCuisinier) représentant les commandes effectuées.</param>
        /// <param name="roles">Dictionnaire associant chaque identifiant utilisateur à son rôle ("Client" ou "Cuisinier").</param>
        /// <param name="noms">Dictionnaire associant chaque identifiant utilisateur au nom de sa station de métro.</param>
        /// <param name="cheminFichier">Chemin du fichier PNG dans lequel sauvegarder l’image du graphe.</param>

        public void AfficherGrapheCommandesAvecRoles(List<(int idClient, int idCuisinier)> commandes, Dictionary<int, string> roles, Dictionary<int, string> noms, string cheminFichier = "graphe_commandes_roles.png")
        {
            const int largeur = 3000;
            const int hauteur = 2250;
            const int rayon = 10;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var noeud in noeuds.Values)
            {
                if (noeud.X < minX) { minX = noeud.X; }
                if (noeud.X > maxX) { maxX = noeud.X; }
                if (noeud.Y < minY) { minY = noeud.Y; }
                if (noeud.Y > maxY) { maxY = noeud.Y; }
            }

            float centreX = (minX + maxX) / 2;
            float centreY = (minY + maxY) / 2;
            float zoomX = 15000f;
            float zoomY = 20000f;

            var bitmap = new Bitmap(largeur, hauteur);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var positions = new Dictionary<int, PointF>();
            foreach (var kvp in noeuds)
            {
                float x = largeur / 2 + (kvp.Value.X - centreX) * zoomX;
                float y = hauteur / 2 - (kvp.Value.Y - centreY) * zoomY;
                positions[kvp.Key] = new PointF(x, y);
            }

            var font = new Font("Arial", 9);
            var brushClient = new SolidBrush(Color.Blue);
            var brushCuisinier = new SolidBrush(Color.Red);

            foreach (var noeud in noeuds.Values)
            {
                var depart = positions[noeud.Id];
                foreach (var lien in noeud.Liens)
                {
                    var arrivee = positions[lien.Destination.Id];
                    g.DrawLine(new Pen(Color.LightGray, 2), depart, arrivee);
                }
            }

            foreach (var kvp in noeuds)
            {
                var pos = positions[kvp.Key];
                g.FillEllipse(Brushes.Black, pos.X - rayon, pos.Y - rayon, rayon * 2, rayon * 2);
            }

            foreach (var commande in commandes)
            {
                int idDepart = GetIdSommetParNom(noms[commande.idCuisinier]);
                int idArrivee = GetIdSommetParNom(noms[commande.idClient]);
                if (idDepart != -1 && idArrivee != -1)
                {
                    Dijkstra(idDepart, idArrivee);
                }
            }

            foreach (var id in roles.Keys)
            {
                string station = noms[id];
                int idNoeud = GetIdSommetParNom(station);
                if (idNoeud != -1)
                {
                    var pos = positions[idNoeud];
                    Brush b = roles[id] == "Client" ? brushClient : brushCuisinier;
                    g.FillEllipse(b, pos.X - rayon, pos.Y - rayon, rayon * 2, rayon * 2);
                    g.DrawString(id + " - " + roles[id], font, Brushes.Black, pos.X + 10, pos.Y);
                }
            }

            g.FillRectangle(Brushes.White, 20, 20, 180, 60);
            g.DrawRectangle(Pens.Black, 20, 20, 180, 60);
            g.FillEllipse(brushClient, 30, 30, 15, 15);
            g.DrawString("Client", font, Brushes.Black, 50, 30);
            g.FillEllipse(brushCuisinier, 30, 55, 15, 15);
            g.DrawString("Cuisinier", font, Brushes.Black, 50, 55);

            bitmap.Save(cheminFichier, ImageFormat.Png);
            Console.WriteLine("Graphe enregistré : " + cheminFichier);

            if (File.Exists(cheminFichier) == true)
            {
                var form = new Form();
                form.Text = "Graphe Commandes";
                form.Width = largeur + 50;
                form.Height = hauteur + 50;
                form.StartPosition = FormStartPosition.CenterScreen;

                var box = new PictureBox();
                box.Image = Image.FromFile(cheminFichier);
                box.Dock = DockStyle.Fill;
                box.SizeMode = PictureBoxSizeMode.Zoom;

                form.Controls.Add(box);
                Application.Run(form);
            }
        }

        /// <summary>
        /// Affiche toutes les stations du graphe avec une coloration obtenue par l'algorithme Welsh-Powell.
        /// Chaque station est affichée sous forme de point coloré, avec son nom à côté.
        /// Les couleurs attribuées permettent de distinguer les groupes non-adjacents.
        /// </summary>
        /// <param name="cheminFichier">Chemin de sauvegarde du fichier image. Si non spécifié, le fichier sera nommé "stations_colorees.png".</param>

        public void AfficherStationsAvecColoration(string cheminFichier = "stations_colorees.png")
        {
            var paletteCouleurs = new Color[] {
        Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
        Color.Brown, Color.Teal, Color.Magenta, Color.Gold, Color.Cyan,
        Color.Lime, Color.DarkBlue, Color.DarkRed, Color.LightGreen
            };

            var coloration = ColorationWelshPowell();

            const int largeur = 3000;
            const int hauteur = 2250;
            const int rayon = 10;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var noeud in noeuds.Values)
            {
                if (noeud.X < minX) { minX = noeud.X; }
                if (noeud.X > maxX) { maxX = noeud.X; }
                if (noeud.Y < minY) { minY = noeud.Y; }
                if (noeud.Y > maxY) { maxY = noeud.Y; }
            }

            float centreX = (minX + maxX) / 2;
            float centreY = (minY + maxY) / 2;
            float zoomX = 15000f;
            float zoomY = 20000f;

            var bitmap = new Bitmap(largeur, hauteur);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var police = new Font("Arial", 9);

            var positions = new Dictionary<int, PointF>();
            foreach (var kvp in noeuds)
            {
                float x = largeur / 2 + (kvp.Value.X - centreX) * zoomX;
                float y = hauteur / 2 - (kvp.Value.Y - centreY) * zoomY;
                positions[kvp.Key] = new PointF(x, y);
            }

            foreach (var noeud in noeuds.Values)
            {
                var posA = positions[noeud.Id];
                foreach (var lien in noeud.Liens)
                {
                    var posB = positions[lien.Destination.Id];
                    g.DrawLine(new Pen(Color.LightGray, 2), posA, posB);
                }
            }

            foreach (var kvp in noeuds)
            {
                var pos = positions[kvp.Key];
                int indexCouleur = coloration[kvp.Key] % paletteCouleurs.Length;
                var pinceau = new SolidBrush(paletteCouleurs[indexCouleur]);
                g.FillEllipse(pinceau, pos.X - rayon, pos.Y - rayon, rayon * 2, rayon * 2);
                g.DrawString(kvp.Value.Nom, police, Brushes.Black, pos.X + 8, pos.Y + 8);
            }

            bitmap.Save(cheminFichier, ImageFormat.Png);
            Console.WriteLine("Carte colorée enregistrée : " + cheminFichier);

            if (File.Exists(cheminFichier) == true)
            {
                var fenetre = new Form();
                fenetre.Text = "Stations colorées (Welsh-Powell)";
                fenetre.Width = largeur + 50;
                fenetre.Height = hauteur + 50;
                fenetre.StartPosition = FormStartPosition.CenterScreen;

                var imageBox = new PictureBox();
                imageBox.Image = Image.FromFile(cheminFichier);
                imageBox.SizeMode = PictureBoxSizeMode.Zoom;
                imageBox.Dock = DockStyle.Fill;

                fenetre.Controls.Add(imageBox);
                Application.Run(fenetre);
            }
        }

    }
}
