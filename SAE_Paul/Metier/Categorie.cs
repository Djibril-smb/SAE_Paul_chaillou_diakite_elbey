namespace SAE_Paul.Metier
{
    /// <summary>Categorie de recette (Gateaux, Viennoiseries, Pains). Table CATEGORIE.</summary>
    public class Categorie
    {
        public int CategorieId { get; set; }
        public string CategorieNom { get; set; }

        public Categorie()
        {
            CategorieNom = "";
        }
    }
}
