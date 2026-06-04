using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Allergene
    {
        private int allergeneId;
        private string allergeneNom;

        public int AllergeneId
        {
            get => allergeneId;
            set => allergeneId = value;
        }
        public string AllergeneNom
        {
            get => allergeneNom;
            set => allergeneNom = value;
        }

        public Allergene()
        {
            allergeneNom = "";
        }

        /// <summary>Liste tous les allergenes tries par nom. Sert aux cases a cocher de la fiche recette.</summary>
        public static List<Allergene> Lister()
        {
            List<Allergene> resultat = new List<Allergene>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT ALLERGENE_ID, ALLERGENE_NOM FROM ALLERGENE ORDER BY ALLERGENE_NOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        Allergene a = new Allergene();
                        a.AllergeneId = lecteur.GetInt32(0);
                        a.AllergeneNom = lecteur.GetString(1);
                        resultat.Add(a);
                    }
                }
            }

            return resultat;
        }
    }
}
