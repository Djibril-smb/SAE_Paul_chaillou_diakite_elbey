using Npgsql;

namespace SAE_Paul.Metier
{
    public static class Connexion
    {
        private const string Hote = "localhost";
        private const int Port = 5432;
        private const string Base = "boulangerie_paul";
        private const string LoginBd = "postgres"; // login BD commun, ecrit en dur

        // Mot de passe saisi au login.
        private static string? _motDePasse;
        public static void DefinirMotDePasse(string motDePasse)
        {
            _motDePasse = motDePasse;
        }

        public static NpgsqlConnection Ouvrir()
        {
            string chaine =
                $"Host={Hote};Port={Port};Database={Base};Username={LoginBd};Password={_motDePasse}";
            NpgsqlConnection cn = new NpgsqlConnection(chaine);
            cn.Open();
            return cn;
        }
    }
}
