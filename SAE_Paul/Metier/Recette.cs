using System;
using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Recette
    {
        private int recetteId;
        private int categorieId;
        private string recetteNom;
        private string? recetteDescription;
        private string? imagePath;

        //champs d'affichage (remplis par JOIN dans Lister)
        private string? categorieNom;
        private decimal? prixMin;
        private decimal? prixMax;
        private int nbTailles;

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
        public string? ImagePath
        {
            get => imagePath;
            set => imagePath = value;
        }
        public string? CategorieNom
        {
            get => categorieNom;
            set => categorieNom = value;
        }
        public decimal? PrixMin
        {
            get => prixMin;
            set => prixMin = value;
        }
        public decimal? PrixMax
        {
            get => prixMax;
            set => prixMax = value;
        }
        public int NbTailles
        {
            get => nbTailles;
            set => nbTailles = value;
        }

        //texte affiche sur la carte recette
        public string LibellePrix
        {
            get
            {
                if (nbTailles == 0) return "Aucune taille";
                if (prixMin == prixMax) return ((decimal)prixMin!).ToString("0.00") + " €";
                return "de " + ((decimal)prixMin!).ToString("0.00") + " € à "
                     + ((decimal)prixMax!).ToString("0.00") + " €";
            }
        }

        public Recette()
        {
            recetteNom = "";
        }

        // -------------------------------------------------------------------
        //  Lecture
        // -------------------------------------------------------------------

        
        ///liste toutes les recettes avec leur categorie et la fourchette de prix
        ///(calculee par sous-requete sur PRODUIT)
        
        public static List<Recette> Lister()
        {
            List<Recette> resultat = new List<Recette>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT R.RECETTE_ID, R.CATEGORIE_ID, R.RECETTE_NOM, R.RECETTE_DESCRIPTION, "
                           + "       R.IMAGE_PATH, "
                           + "       C.CATEGORIE_NOM, "
                           + "       (SELECT MIN(P.PRIX) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS PRIX_MIN, "
                           + "       (SELECT MAX(P.PRIX) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS PRIX_MAX, "
                           + "       (SELECT COUNT(*) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS NB_TAILLES "
                           + "FROM RECETTE R "
                           + "JOIN CATEGORIE C ON C.CATEGORIE_ID = R.CATEGORIE_ID "
                           + "ORDER BY C.CATEGORIE_NOM, R.RECETTE_NOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        Recette r = new Recette();
                        r.RecetteId = lecteur.GetInt32(lecteur.GetOrdinal("RECETTE_ID"));
                        r.CategorieId = lecteur.GetInt32(lecteur.GetOrdinal("CATEGORIE_ID"));
                        r.RecetteNom = lecteur.GetString(lecteur.GetOrdinal("RECETTE_NOM"));
                        int iDesc = lecteur.GetOrdinal("RECETTE_DESCRIPTION");
                        r.RecetteDescription = lecteur.IsDBNull(iDesc) ? null : lecteur.GetString(iDesc);
                        int iImg = lecteur.GetOrdinal("IMAGE_PATH");
                        r.ImagePath = lecteur.IsDBNull(iImg) ? null : lecteur.GetString(iImg);
                        r.CategorieNom = lecteur.GetString(lecteur.GetOrdinal("CATEGORIE_NOM"));

                        int iMin = lecteur.GetOrdinal("PRIX_MIN");
                        r.PrixMin = lecteur.IsDBNull(iMin) ? (decimal?)null : lecteur.GetDecimal(iMin);
                        int iMax = lecteur.GetOrdinal("PRIX_MAX");
                        r.PrixMax = lecteur.IsDBNull(iMax) ? (decimal?)null : lecteur.GetDecimal(iMax);

                        // COUNT(*) en PostgreSQL : bigint -> Int64.
                        r.NbTailles = (int)lecteur.GetInt64(lecteur.GetOrdinal("NB_TAILLES"));

                        resultat.Add(r);
                    }
                }
            }

            return resultat;
        }

        //retourne les IDs des allergenes lies a cette recette
        public List<int> ListerMesAllergenes()
        {
            List<int> resultat = new List<int>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT ALLERGENE_ID FROM RECETTE_ALLERGENE WHERE RECETTE_ID = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", recetteId);

                    using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                    {
                        while (lecteur.Read())
                            resultat.Add(lecteur.GetInt32(0));
                    }
                }
            }

            return resultat;
        }

        // -------------------------------------------------------------------
        //  Ecriture
        // -------------------------------------------------------------------

        
        //cree (RecetteId == 0) ou met a jour la recette
        
        public void Enregistrer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                if (recetteId == 0)
                {
                    // CREATION
                    string sql = "INSERT INTO RECETTE (CATEGORIE_ID, RECETTE_NOM, RECETTE_DESCRIPTION, IMAGE_PATH) "
                               + "VALUES (@cat, @nom, @desc, @img) RETURNING RECETTE_ID";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("cat", categorieId);
                        cmd.Parameters.AddWithValue("nom", recetteNom);
                        cmd.Parameters.AddWithValue("desc",
                            (object?)recetteDescription ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("img",
                            (object?)imagePath ?? System.DBNull.Value);
                        recetteId = (int)cmd.ExecuteScalar()!;
                    }
                }
                else
                {
                    // MISE A JOUR
                    string sql = "UPDATE RECETTE "
                               + "SET CATEGORIE_ID = @cat, RECETTE_NOM = @nom, "
                               + "    RECETTE_DESCRIPTION = @desc, IMAGE_PATH = @img "
                               + "WHERE RECETTE_ID = @id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("cat", categorieId);
                        cmd.Parameters.AddWithValue("nom", recetteNom);
                        cmd.Parameters.AddWithValue("desc",
                            (object?)recetteDescription ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("img",
                            (object?)imagePath ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("id", recetteId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        
        //remplace les allergenes lies a cette recette par la liste passee
        //DELETE puis INSERT, dans une transaction
        
        public void MettreAJourAllergenes(List<int> nouveauxIds)
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            using (NpgsqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string del = "DELETE FROM RECETTE_ALLERGENE WHERE RECETTE_ID = @id";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(del, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", recetteId);
                        cmd.ExecuteNonQuery();
                    }

                    string ins = "INSERT INTO RECETTE_ALLERGENE (RECETTE_ID, ALLERGENE_ID) "
                               + "VALUES (@rId, @aId)";
                    foreach (int aId in nouveauxIds)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(ins, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("rId", recetteId);
                            cmd.Parameters.AddWithValue("aId", aId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        
        //supprime la recette ET ses liens d'allergenes, dans une transaction
        //echoue (PostgresException 23503) si des PRODUIT existent encore
        
        public void Supprimer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            using (NpgsqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    string delAll = "DELETE FROM RECETTE_ALLERGENE WHERE RECETTE_ID = @id";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(delAll, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", recetteId);
                        cmd.ExecuteNonQuery();
                    }

                    string delRec = "DELETE FROM RECETTE WHERE RECETTE_ID = @id";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(delRec, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", recetteId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
