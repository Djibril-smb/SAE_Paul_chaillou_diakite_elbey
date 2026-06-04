using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Produit
    {
        private int produitId;
        private int recetteId;
        private bool estIndisponible;
        private int nbParts;
        private decimal prix;

        // Champs d'affichage uniquement (remplis par JOIN, pas stockes dans PRODUIT).
        private string? recetteNom;
        private string? categorieNom;
        private string? imagePath;

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
        public string? RecetteNom
        {
            get => recetteNom;
            set => recetteNom = value;
        }
        public string? CategorieNom
        {
            get => categorieNom;
            set => categorieNom = value;
        }
        public string? ImagePath
        {
            get => imagePath;
            set => imagePath = value;
        }

        /// <summary>Inverse de EstIndisponible, pour un affichage positif a l'ecran.</summary>
        public bool EstDisponible
        {
            get => !estIndisponible;
        }

        /// <summary>Libelle dynamique du bouton "marquer indispo / rendre dispo".</summary>
        public string LibelleBoutonDispo
        {
            get => estIndisponible ? "Rendre dispo" : "Marquer indispo";
        }

        /// <summary>
        /// Libelle pret a afficher dans un ComboBox.
        /// Ex : "Gateaux - Fraisier (8 parts) - 44.00 €".
        /// </summary>
        public string Libelle
        {
            get => (categorieNom ?? "") + " - " + (recetteNom ?? "") +
                   " (" + nbParts + " parts) - " + prix.ToString("0.00") + " €";
        }

        public Produit()
        {
        }

        /// <summary>
        /// Retourne tous les produits d'une recette (disponibles ET indisponibles).
        /// Utilise par la fiche recette (gestion des tailles).
        /// </summary>
        public static List<Produit> ListerParRecette(int recetteId)
        {
            List<Produit> resultat = new List<Produit>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT PRODUIT_ID, RECETTE_ID, EST_INDISPONIBLE, NB_PARTS, PRIX "
                           + "FROM PRODUIT WHERE RECETTE_ID = @id ORDER BY NB_PARTS";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", recetteId);

                    using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                    {
                        while (lecteur.Read())
                        {
                            Produit p = new Produit();
                            p.ProduitId = lecteur.GetInt32(lecteur.GetOrdinal("PRODUIT_ID"));
                            p.RecetteId = lecteur.GetInt32(lecteur.GetOrdinal("RECETTE_ID"));
                            p.EstIndisponible = lecteur.GetBoolean(lecteur.GetOrdinal("EST_INDISPONIBLE"));
                            p.NbParts = lecteur.GetInt32(lecteur.GetOrdinal("NB_PARTS"));
                            p.Prix = lecteur.GetDecimal(lecteur.GetOrdinal("PRIX"));
                            resultat.Add(p);
                        }
                    }
                }
            }

            return resultat;
        }

        /// <summary>Insere un nouveau produit (taille) pour une recette. Recupere l'id genere.</summary>
        public void Enregistrer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "INSERT INTO PRODUIT (RECETTE_ID, EST_INDISPONIBLE, NB_PARTS, PRIX) "
                           + "VALUES (@rec, @indispo, @parts, @prix) RETURNING PRODUIT_ID";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("rec", recetteId);
                    cmd.Parameters.AddWithValue("indispo", estIndisponible);
                    cmd.Parameters.AddWithValue("parts", nbParts);
                    cmd.Parameters.AddWithValue("prix", prix);
                    produitId = (int)cmd.ExecuteScalar()!;
                }
            }
        }

        /// <summary>
        /// Bascule l'indisponibilite (TRUE/FALSE) du produit.
        /// </summary>
        public void BasculerDisponibilite()
        {
            estIndisponible = !estIndisponible;

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "UPDATE PRODUIT SET EST_INDISPONIBLE = @v WHERE PRODUIT_ID = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("v", estIndisponible);
                    cmd.Parameters.AddWithValue("id", produitId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Supprime ce produit. Echoue (23503) s'il est reference par des LIGNE_COMMANDE.
        /// </summary>
        public void Supprimer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "DELETE FROM PRODUIT WHERE PRODUIT_ID = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", produitId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retourne tous les produits disponibles (non indisponibles),
        /// avec le nom de la recette et de la categorie via JOIN.
        /// </summary>
        public static List<Produit> Lister()
        {
            List<Produit> resultat = new List<Produit>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT P.PRODUIT_ID, P.RECETTE_ID, P.EST_INDISPONIBLE, "
                           + "       P.NB_PARTS, P.PRIX, "
                           + "       R.RECETTE_NOM, R.IMAGE_PATH, C.CATEGORIE_NOM "
                           + "FROM PRODUIT P "
                           + "JOIN RECETTE R ON R.RECETTE_ID = P.RECETTE_ID "
                           + "JOIN CATEGORIE C ON C.CATEGORIE_ID = R.CATEGORIE_ID "
                           + "WHERE P.EST_INDISPONIBLE = FALSE "
                           + "ORDER BY C.CATEGORIE_NOM, R.RECETTE_NOM, P.NB_PARTS";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        Produit p = new Produit();
                        p.ProduitId = lecteur.GetInt32(lecteur.GetOrdinal("PRODUIT_ID"));
                        p.RecetteId = lecteur.GetInt32(lecteur.GetOrdinal("RECETTE_ID"));
                        p.EstIndisponible = lecteur.GetBoolean(lecteur.GetOrdinal("EST_INDISPONIBLE"));
                        p.NbParts = lecteur.GetInt32(lecteur.GetOrdinal("NB_PARTS"));
                        p.Prix = lecteur.GetDecimal(lecteur.GetOrdinal("PRIX"));
                        p.RecetteNom = lecteur.GetString(lecteur.GetOrdinal("RECETTE_NOM"));
                        p.CategorieNom = lecteur.GetString(lecteur.GetOrdinal("CATEGORIE_NOM"));
                        int iImg = lecteur.GetOrdinal("IMAGE_PATH");
                        p.ImagePath = lecteur.IsDBNull(iImg) ? null : lecteur.GetString(iImg);
                        resultat.Add(p);
                    }
                }
            }

            return resultat;
        }
    }
}
