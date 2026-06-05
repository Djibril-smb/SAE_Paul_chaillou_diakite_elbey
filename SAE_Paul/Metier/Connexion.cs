using Npgsql;

namespace SAE_Paul.Metier
{
    public static class Connexion
    {
        private const string Hote = "srv-peda-new";
        private const int Port = 5433;
        private const string Base = "sae_paul";
        private const string Schema = "sae_paul";
        private const string LoginBd = "elbeyi"; // login BD commun, ecrit en dur

        // Mot de passe saisi au login.
        private static string? _motDePasse;
        public static void DefinirMotDePasse(string motDePasse)
        {
            _motDePasse = motDePasse;
        }

        // Role de l'utilisateur courant (VENDEUR ou PATISSIER), defini au login.
        // Sert a router le bouton "retour" des ecrans partages (ex. Produits).
        private static string? _role;
        public static string? Role
        {
            get => _role;
        }
        public static void DefinirRole(string role)
        {
            _role = role;
        }

        public static NpgsqlConnection Ouvrir()
        {
            string chaine =
                $"Host={Hote};Port={Port};Database={Base};Username={LoginBd};Password={_motDePasse};Options='-c search_path={Schema}'";
            NpgsqlConnection cn = new NpgsqlConnection(chaine);
            cn.Open();
            return cn;
        }
    }
}
