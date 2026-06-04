namespace SAE_Paul.Metier
{
    /// <summary>Type d'evenement associe a une commande (Mariage, Anniversaire...). Table CATEGORIE_EVENEMENT.</summary>
    public class CategorieEvenement
    {
        public int CategorieEvenementId { get; set; }
        public string CategorieEvenementNom { get; set; }

        public CategorieEvenement()
        {
            CategorieEvenementNom = "";
        }
    }
}
