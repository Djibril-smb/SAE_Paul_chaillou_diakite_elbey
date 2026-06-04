namespace SAE_Paul.Metier
{
    public class Recette
    {
        private int recetteId;
        private int categorieId;
        private string recetteNom;
        private string? recetteDescription;

        public int RecetteId
        {
            get => recetteId;
            set => recetteId = value;
        }
        public int CategorieId
        {
            get => categorieId;
            set => categorieId = value;
        }
        public string RecetteNom
        {
            get => recetteNom;
            set => recetteNom = value;
        }
        public string? RecetteDescription
        {
            get => recetteDescription;
            set => recetteDescription = value;
        }

        public Recette()
        {
            recetteNom = "";
        }
    }
}
