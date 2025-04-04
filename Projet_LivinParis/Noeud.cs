using System;
using System.Collections.Generic;

namespace PROJET_PSI
{
    /// <summary>
    /// Représente un nœud dans un graphe, pouvant être utilisé pour modéliser une station de métro.
    /// </summary>
    /// <typeparam name="T"> Liens sortants du nœud.</typeparam>
    public class Noeud<T>
    {
        /// <summary>
        /// Id unique du noeud.
        /// </summary>
        private int id;

        /// <summary>
        /// Nom du nœud (ici, ça sera le nom d'une station).
        /// </summary>
        private string nom;

        /// <summary>
        /// Liste des liens sortants du nœud.
        /// </summary>
        private List<Lien<T>> liens = new List<Lien<T>>();

        /// <summary>
        /// Coordonnée X pour l'affichage du graphe.
        /// </summary>
        private float x;

        /// <summary>
        /// Coordonnée Y pour l'affichage du graphe.
        /// </summary>
        private float y;

        /// <summary>
        /// Crée un nouveau nœud avec un identifiant donné.
        /// </summary>
        /// <param name="id">Id unique du nœud.</param>
        public Noeud(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Obtient ou modifie l'id du nœud.
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Obtient ou modifie le nom du nœud.
        /// </summary>
        public string Nom
        {
            get { return nom; }
            set { nom = value; }
        }

        /// <summary>
        /// Obtient ou modifie la liste des liens sortants du nœud.
        /// </summary>
        public List<Lien<T>> Liens
        {
            get { return liens; }
            set { liens = value; }
        }

        /// <summary>
        /// Obtient ou modifie la coordonnée X du nœud (pour l'affichage).
        /// </summary>
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Obtient ou modifie la coordonnée Y du nœud (pour l'affichage).
        /// </summary>
        public float Y
        {
            get { return y; }
            set { y = value; }
        }
    }
}

