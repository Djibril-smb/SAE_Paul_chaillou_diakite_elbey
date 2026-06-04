namespace SAE_Paul.Metier
{
    public class Categorie
    {
        private int categorieId;
        private string categorieNom;

        public int CategorieId
        {
            get => categorieId;
            set => categorieId = value;
        }
        public string CategorieNom
        {
            get => categorieNom;
            set => categorieNom = value;
        }

        public Categorie()
        {
            categorieNom = "";
        }
    }
}
