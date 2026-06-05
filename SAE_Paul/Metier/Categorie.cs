using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Categorie
    {
        private int categorieId;
        private string categorieNom;

        public int CategorieId
        {
            get => categorieId;
            set => categorieId = value;
        }
        public string CategorieNom
        {
            get => categorieNom;
            set => categorieNom = value;
        }

        public Categorie()
        {
            categorieNom = "";
        }

        //liste des categories triées par nom --> combobox de choix de catégorie
        public static List<Categorie> Lister()
        {
            List<Categorie> resultat = new List<Categorie>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT CATEGORIE_ID, CATEGORIE_NOM FROM CATEGORIE ORDER BY CATEGORIE_NOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        Categorie c = new Categorie();
                        c.CategorieId = lecteur.GetInt32(0);
                        c.CategorieNom = lecteur.GetString(1);
                        resultat.Add(c);
                    }
                }
            }

            return resultat;
        }
    }
}
