using System;

namespace PROJET_PSI
{
    public class Lien<T>
    {
        private Noeud<T> destination;
        private double poids;

        public Noeud<T> Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public double Poids
        {
            get { return poids; }
            set { poids = value; }
        }

        public Lien(Noeud<T> dest, double poids = 1.0)
        {
            this.destination = dest;
            this.poids = poids;
        }
    }
}
