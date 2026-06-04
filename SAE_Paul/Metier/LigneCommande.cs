namespace SAE_Paul.Metier
{
    public class LigneCommande
    {
        private int commandeId;
        private int produitId;
        private int quantite;
        private bool estDecoupe;

        public int CommandeId
        {
            get => commandeId;
            set => commandeId = value;
        }
        public int ProduitId
        {
            get => produitId;
            set => produitId = value;
        }
        public int Quantite
        {
            get => quantite;
            set => quantite = value;
        }
        public bool EstDecoupe
        {
            get => estDecoupe;
            set => estDecoupe = value;
        }

        public LigneCommande()
        {
        }
    }
}
