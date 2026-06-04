namespace SAE_Paul.Metier
{
    /// <summary>
    /// Une ligne d'une commande : un produit, une quantite, et s'il doit etre decoupe.
    /// Table LIGNE_COMMANDE (cle primaire = COMMANDE_ID + PRODUIT_ID).
    /// </summary>
    public class LigneCommande
    {
        public int CommandeId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }
        public bool EstDecoupe { get; set; }

        public LigneCommande()
        {
        }
    }
}
