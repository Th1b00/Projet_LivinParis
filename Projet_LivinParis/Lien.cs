using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_LivinParis
{
    internal class Lien
    {
        /// <summary>
        /// Noeud de destination du lien.
        /// </summary>
        private Noeud destination;

        /// <summary>
        /// Définit le noeud de destination du lien.
        /// </summary>
        public Noeud Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        /// <summary>
        /// Initialise une nouvelle instance de la classe Lien.
        /// </summary>
        /// <param name="dest">Noeud de destination du lien</param>
        public Lien(Noeud dest)
        {
            this.destination = dest;
        }
    }
}
