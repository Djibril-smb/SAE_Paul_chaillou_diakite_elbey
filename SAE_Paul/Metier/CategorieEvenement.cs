namespace SAE_Paul.Metier
{
    public class CategorieEvenement
    {
        private int categorieEvenementId;
        private string categorieEvenementNom;

        public int CategorieEvenementId
        {
            get => categorieEvenementId;
            set => categorieEvenementId = value;
        }
        public string CategorieEvenementNom
        {
            get => categorieEvenementNom;
            set => categorieEvenementNom = value;
        }

        public CategorieEvenement()
        {
            categorieEvenementNom = "";
        }
    }
}
