using System;

namespace PROJET_PSI
{
    /// <summary>
    /// Représente un lien entre deux noeuds dans un graphe pondéré.
    /// </summary>
    /// <typeparam name="T">Type de données contenues dans les nœuds.</typeparam>
    public class Lien<T>
    {
        /// <summary>
        /// Nœud de destination du lien.
        /// </summary>
        private Noeud<T> destination;

        /// <summary>
        /// Poids associé au lien.
        /// </summary>
        private double poids;

        /// <summary>
        /// Définit le nœud de destination.
        /// </summary>
        public Noeud<T> Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        /// <summary>
        /// Définit le poids du lien.
        /// </summary>
        public double Poids
        {
            get { return poids; }
            set { poids = value; }
        }

        /// <summary>
        /// Initialise un nouveau lien vers un nœud donné.
        /// </summary>
        /// <param name="dest">Noeud de destination.</param>
        /// <param name="poids">Poids du lien.</param>
        public Lien(Noeud<T> dest, double poids = 1.0)
        {
            this.destination = dest;
            this.poids = poids;
        }
    }
}
