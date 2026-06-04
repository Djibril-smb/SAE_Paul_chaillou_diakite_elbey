namespace SAE_Paul.Metier
{
    /// <summary>
    /// Recette (le "modele" d'un produit : un nom, une categorie, une description).
    /// Table RECETTE. Une recette peut etre declinee en plusieurs Produits (tailles/parts).
    /// </summary>
    public class Recette
    {
        public int RecetteId { get; set; }
        public int CategorieId { get; set; }
        public string RecetteNom { get; set; }
        public string? RecetteDescription { get; set; }

        public Recette()
        {
            RecetteNom = "";
        }
    }
}
