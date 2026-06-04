using Npgsql;

namespace SAE_Paul.Metier
{
   
    public class Utilisateur
    {
        public string Login { get; set; }
        public string Role { get; set; }

        public Utilisateur()
        {
            Login = "";
            Role = "";
        }

        public static Utilisateur? Trouver(string login)
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT LOGIN, ROLE FROM UTILISATEUR WHERE LOGIN = @login";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("login", login);

                    using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                    {
                        if (lecteur.Read())
                        {
                            return new Utilisateur
                            {
                                Login = lecteur.GetString(0),
                                Role = lecteur.GetString(1)
                            };
                        }
                    }
                }
            }

            return null; 
        }
    }
}
