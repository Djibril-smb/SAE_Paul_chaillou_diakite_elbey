using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class CategorieEvenement
    {
        private int categorieEvenementId;
        private string categorieEvenementNom;

        public int CategorieEvenementId
        {
            get => categorieEvenementId;
            set => categorieEvenementId = value;
        }
        public string CategorieEvenementNom
        {
            get => categorieEvenementNom;
            set => categorieEvenementNom = value;
        }

        public CategorieEvenement()
        {
            categorieEvenementNom = "";
        }

        //liste des categories d'evenements --> combobox choix d'evenement (creation de commande)
        public static List<CategorieEvenement> Lister()
        {
            List<CategorieEvenement> resultat = new List<CategorieEvenement>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT CATEGORIE_EVENEMENT_ID, CATEGORIE_EVENEMENT_NOM "
                           + "FROM CATEGORIE_EVENEMENT ORDER BY CATEGORIE_EVENEMENT_NOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        CategorieEvenement ce = new CategorieEvenement();
                        ce.CategorieEvenementId = lecteur.GetInt32(0);
                        ce.CategorieEvenementNom = lecteur.GetString(1);
                        resultat.Add(ce);
                    }
                }
            }

            return resultat;
        }
    }
}
