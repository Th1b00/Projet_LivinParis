using System;

namespace PROJET_PSI
{
    /// <summary>
    /// Représente un lien entre deux nœuds avec un poids associé.
    /// </summary>
    /// <typeparam name="T">Type des données associées au lien.</typeparam>
    public class Lien<T>
    {
        /// <summary>
        /// Nœud de destination de ce lien.
        /// </summary>
        private Noeud<T> destination;

        /// <summary>
        /// Poids du lien (ex : durée du trajet entre deux stations).
        /// </summary>
        private double poids;

        /// <summary>
        /// Informations du lien.
        /// </summary>
        private T info;

        /// <summary>
        /// Crée un nouveau lien vers une destination, avec un poids et une information associée.
        /// </summary>
        /// <param name="destination">Nœud de destination.</param>
        /// <param name="poids">Poids du lien.</param>
        /// <param name="info">Informations du lien.</param>
        public Lien(Noeud<T> destination, double poids, T info)
        {
            this.destination = destination;
            this.poids = poids;
            this.info = info;
        }

        /// <summary>
        /// Obtient ou modifie le nœud de destination du lien.
        /// </summary>
        public Noeud<T> Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        /// <summary>
        /// Obtient ou modifie le poids du lien.
        /// </summary>
        public double Poids
        {
            get { return poids; }
            set { poids = value; }
        }

        /// <summary>
        /// Obtient ou modifie l'information associée au lien.
        /// </summary>
        public T Info
        {
            get { return info; }
            set { info = value; }
        }
    }
}

