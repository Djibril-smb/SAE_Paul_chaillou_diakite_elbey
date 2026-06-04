namespace SAE_Paul.Metier
{
    /// <summary>
    /// Produit vendable = une recette declinee en un nombre de parts et un prix.
    /// Table PRODUIT. C'est ce que l'on ajoute dans une ligne de commande.
    /// (Gere par un coequipier ; cote Commandes on ne fait que LIRE les produits.)
    /// </summary>
    public class Produit
    {
        public int ProduitId { get; set; }
        public int RecetteId { get; set; }
        public bool EstIndisponible { get; set; }
        public int NbParts { get; set; }
        public decimal Prix { get; set; }

        public Produit()
        {
        }
    }
}
