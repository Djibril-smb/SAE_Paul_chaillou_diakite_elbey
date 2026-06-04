using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{

    public class Client
    {
        public int ClientId { get; set; }
        public string Nom { get; set; }
        public string? Prenom { get; set; }
        public string Telephone { get; set; }
        public string? Mail { get; set; }

        public Client()
        {
            Nom = "";
            Telephone = "";
        }

        public static List<Client> Lister()
        {
            List<Client> resultat = new List<Client>();

            // 'using' = la connexion est fermee automatiquement a la fin du bloc.
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT CLIENT_ID, NOM, PRENOM, TELEPHONE, MAIL "
                           + "FROM CLIENT ORDER BY NOM, PRENOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    // On avance ligne par ligne dans le resultat du SELECT.
                    while (lecteur.Read())
                    {
                        resultat.Add(LireDepuis(lecteur));
                    }
                }
            }

            return resultat;
        }

        /// <summary>
        /// Recherche les clients dont le nom, prenom ou telephone contient 'texte'.
        /// </summary>
        public static List<Client> Rechercher(string texte)
        {
            List<Client> resultat = new List<Client>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT CLIENT_ID, NOM, PRENOM, TELEPHONE, MAIL FROM CLIENT "
                           + "WHERE NOM ILIKE @motif OR PRENOM ILIKE @motif OR TELEPHONE ILIKE @motif "
                           + "ORDER BY NOM, PRENOM";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    // Parametre => protege contre l'injection SQL. Les % = "contient".
                    cmd.Parameters.AddWithValue("motif", "%" + texte + "%");

                    using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                    {
                        while (lecteur.Read())
                        {
                            resultat.Add(LireDepuis(lecteur));
                        }
                    }
                }
            }

            return resultat;
        }

        /// <summary>
        /// Insere le client courant dans la base et recupere l'identifiant genere.
        /// </summary>
        public void Enregistrer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                // RETURNING CLIENT_ID : PostgreSQL nous renvoie l'id auto-genere (SERIAL).
                string sql = "INSERT INTO CLIENT (NOM, PRENOM, TELEPHONE, MAIL) "
                           + "VALUES (@nom, @prenom, @telephone, @mail) RETURNING CLIENT_ID";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("nom", Nom);
                    cmd.Parameters.AddWithValue("prenom", (object?)Prenom ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("telephone", Telephone);
                    cmd.Parameters.AddWithValue("mail", (object?)Mail ?? System.DBNull.Value);

                    // ExecuteScalar = recupere la 1re colonne de la 1re ligne (ici CLIENT_ID).
                    ClientId = (int)cmd.ExecuteScalar()!;
                }
            }
        }

        /// <summary>
        /// Fabrique un objet Client a partir de la ligne courante du lecteur.
        /// (Evite de repeter le meme code dans Lister et Rechercher.)
        /// </summary>
        private static Client LireDepuis(NpgsqlDataReader lecteur)
        {
            return new Client
            {
                ClientId = lecteur.GetInt32(lecteur.GetOrdinal("CLIENT_ID")),
                Nom = lecteur.GetString(lecteur.GetOrdinal("NOM")),
                Prenom = lecteur.IsDBNull(lecteur.GetOrdinal("PRENOM"))
                            ? null : lecteur.GetString(lecteur.GetOrdinal("PRENOM")),
                Telephone = lecteur.GetString(lecteur.GetOrdinal("TELEPHONE")),
                Mail = lecteur.IsDBNull(lecteur.GetOrdinal("MAIL"))
                            ? null : lecteur.GetString(lecteur.GetOrdinal("MAIL"))
            };
        }
    }
}
