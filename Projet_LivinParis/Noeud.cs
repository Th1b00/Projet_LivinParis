using System;
using System.Collections.Generic;

namespace PROJET_PSI
{
    public class Noeud<T>
    {
        private int id;
        private string nom;
        private List<Lien<T>> liens = new List<Lien<T>>();
        private float x;
        private float y;

        public Noeud(int id)
        {
            this.id = id;
        }

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

        public List<Lien<T>> Liens
        {
            get { return liens; }
            set { liens = value; }
        }
        public float X
        {
            get { return x; }
            set { x = value; }
        }
        public float Y
        {
            get { return y; }
            set { y = value; }
        }
    }
}
