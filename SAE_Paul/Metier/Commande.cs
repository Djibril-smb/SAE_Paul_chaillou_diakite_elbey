using System;
using System.Collections.Generic;

namespace SAE_Paul.Metier
{
    /// <summary>
    /// Une commande passee par un client pour un evenement. Table COMMANDE.
    /// Regle metier du sujet : l'acompte vaut le quart du total (Acompte = Total / 4).
    /// </summary>
    public class Commande
    {
        public int CommandeId { get; set; }
        public int? CategorieEvenementId { get; set; }
        public int ClientId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateRetrait { get; set; }
        public decimal Acompte { get; set; }
        public bool EstPrete { get; set; }
        public bool EstRecuperee { get; set; }
        public decimal Total { get; set; }
        public DateTime? DateEvenement { get; set; }
        public int? NbPersonne { get; set; }

        // Lignes de la commande (en memoire : remplies/lues a part dans LIGNE_COMMANDE).
        public List<LigneCommande> Lignes { get; set; }

        public Commande()
        {
            Lignes = new List<LigneCommande>();
        }
    }
}
