using Npgsql;

namespace SAE_Paul.Metier
{
    public class Utilisateur
    {
        private string login;
        private string role;

        public string Login
        {
            get => login;
            set => login = value;
        }
        public string Role
        {
            get => role;
            set => role = value;
        }

        public Utilisateur()
        {
            login = "";
            role = "";
        }

        
        //cherche un utilisateur par son login. Retourne null s'il n'existe pas
        
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
                            Utilisateur u = new Utilisateur();
                            u.Login = lecteur.GetString(0);
                            u.Role = lecteur.GetString(1);
                            return u;
                        }
                    }
                }
            }

            return null;
        }
    }
}
