using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class LigneCommande
    {
        private int commandeId;
        private int produitId;
        private int quantite;
        private bool estDecoupe;

        // Champs d'affichage uniquement (remplis par JOIN, pas stockes dans LIGNE_COMMANDE).
        private string? libelle;
        private decimal prixUnitaire;

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
        public string? Libelle
        {
            get => libelle;
            set => libelle = value;
        }
        public decimal PrixUnitaire
        {
            get => prixUnitaire;
            set => prixUnitaire = value;
        }
        public decimal SousTotal
        {
            get => prixUnitaire * quantite;
        }

        public LigneCommande()
        {
        }

        /// <summary>
        /// Retourne toutes les lignes d'une commande, avec le libelle du produit
        /// (categorie + nom + nb parts) et son prix unitaire actuel.
        /// </summary>
        public static List<LigneCommande> ListerParCommande(int commandeId)
        {
            List<LigneCommande> resultat = new List<LigneCommande>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT L.COMMANDE_ID, L.PRODUIT_ID, L.QUANTITE, L.EST_DECOUPE, "
                           + "       P.PRIX, P.NB_PARTS, R.RECETTE_NOM, C.CATEGORIE_NOM "
                           + "FROM LIGNE_COMMANDE L "
                           + "JOIN PRODUIT P ON P.PRODUIT_ID = L.PRODUIT_ID "
                           + "JOIN RECETTE R ON R.RECETTE_ID = P.RECETTE_ID "
                           + "JOIN CATEGORIE C ON C.CATEGORIE_ID = R.CATEGORIE_ID "
                           + "WHERE L.COMMANDE_ID = @id "
                           + "ORDER BY C.CATEGORIE_NOM, R.RECETTE_NOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", commandeId);

                    using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                    {
                        while (lecteur.Read())
                        {
                            LigneCommande l = new LigneCommande();
                            l.CommandeId = lecteur.GetInt32(lecteur.GetOrdinal("COMMANDE_ID"));
                            l.ProduitId = lecteur.GetInt32(lecteur.GetOrdinal("PRODUIT_ID"));
                            l.Quantite = lecteur.GetInt32(lecteur.GetOrdinal("QUANTITE"));
                            l.EstDecoupe = lecteur.GetBoolean(lecteur.GetOrdinal("EST_DECOUPE"));
                            l.PrixUnitaire = lecteur.GetDecimal(lecteur.GetOrdinal("PRIX"));
                            l.Libelle = lecteur.GetString(lecteur.GetOrdinal("CATEGORIE_NOM"))
                                      + " - " + lecteur.GetString(lecteur.GetOrdinal("RECETTE_NOM"))
                                      + " (" + lecteur.GetInt32(lecteur.GetOrdinal("NB_PARTS")) + " parts)";
                            resultat.Add(l);
                        }
                    }
                }
            }

            return resultat;
        }
    }
}
