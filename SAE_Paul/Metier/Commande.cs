using System;
using System.Collections.Generic;

namespace SAE_Paul.Metier
{
    public class Commande
    {
        private int commandeId;
        private int? categorieEvenementId;
        private int clientId;
        private DateTime dateCreation;
        private DateTime dateRetrait;
        private decimal acompte;
        private bool estPrete;
        private bool estRecuperee;
        private decimal total;
        private DateTime? dateEvenement;
        private int? nbPersonne;
        private List<LigneCommande> lignes;

        public int CommandeId 
        { 
            get => commandeId; 
            set => commandeId = value; 
        }
        public int? CategorieEvenementId 
        { 
            get => categorieEvenementId;
            set => categorieEvenementId = value; 
        }
        
        public int ClientId 
        { 
            get => clientId;
            set => clientId = value; 
        }
        public DateTime DateCreation 
        { 
            get => dateCreation;
            set => dateCreation = value; 
        }
        public DateTime DateRetrait 
        {   get => dateRetrait;
            set => dateRetrait = value; 
        }
        public decimal Acompte 
        {   get => acompte;
            set => acompte = value; 
        }
        public bool EstPrete 
        { 
            get => estPrete;
            set => estPrete = value; 
        }
        public bool EstRecuperee 
        { 
            get => estRecuperee;
            set => estRecuperee = value; 
        }
        public decimal Total 
        { 
            get => total; 
            set => total = value; 
        }
        public DateTime? DateEvenement 
        { 
            get => dateEvenement;
            set => dateEvenement = value; 
        }
        public int? NbPersonne 
        { 
            get => nbPersonne;
            set => nbPersonne = value; 
        }
        public List<LigneCommande> Lignes 
        {
            get => lignes;
            set => lignes = value; 
        }


        public Commande()
        {
            lignes = new List<LigneCommande>();
        }
    }
}
