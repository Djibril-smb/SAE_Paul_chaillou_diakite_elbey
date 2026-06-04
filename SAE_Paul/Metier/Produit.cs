namespace SAE_Paul.Metier
{
    public class Produit
    {
        private int produitId;
        private int recetteId;
        private bool estIndisponible;
        private int nbParts;
        private decimal prix;

        public int ProduitId
        {
            get => produitId;
            set => produitId = value;
        }
        public int RecetteId
        {
            get => recetteId;
            set => recetteId = value;
        }
        public bool EstIndisponible
        {
            get => estIndisponible;
            set => estIndisponible = value;
        }
        public int NbParts
        {
            get => nbParts;
            set => nbParts = value;
        }
        public decimal Prix
        {
            get => prix;
            set => prix = value;
        }

        public Produit()
        {
        }
    }
}
