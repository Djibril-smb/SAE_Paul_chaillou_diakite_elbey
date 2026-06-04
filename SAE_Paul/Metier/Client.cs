using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Client
    {
        private int clientId;
        private string nom;
        private string? prenom;
        private string telephone;
        private string? mail;

        public int ClientId
        {
            get => clientId;
            set => clientId = value;
        }
        public string Nom
        {
            get => nom;
            set => nom = value;
        }
        public string? Prenom
        {
            get => prenom;
            set => prenom = value;
        }
        public string Telephone
        {
            get => telephone;
            set => telephone = value;
        }
        public string? Mail
        {
            get => mail;
            set => mail = value;
        }

        public Client()
        {
            nom = "";
            telephone = "";
        }

        /// <summary>
        /// Retourne tous les clients, tries par nom puis prenom.
        /// </summary>
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
                    cmd.Parameters.AddWithValue("nom", nom);
                    cmd.Parameters.AddWithValue("prenom", (object?)prenom ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("telephone", telephone);
                    cmd.Parameters.AddWithValue("mail", (object?)mail ?? System.DBNull.Value);

                    // ExecuteScalar = recupere la 1re colonne de la 1re ligne (ici CLIENT_ID).
                    clientId = (int)cmd.ExecuteScalar()!;
                }
            }
        }

        /// <summary>
        /// Fabrique un objet Client a partir de la ligne courante du lecteur.
        /// </summary>
        private static Client LireDepuis(NpgsqlDataReader lecteur)
        {
            Client c = new Client();
            c.ClientId = lecteur.GetInt32(lecteur.GetOrdinal("CLIENT_ID"));
            c.Nom = lecteur.GetString(lecteur.GetOrdinal("NOM"));
            c.Prenom = lecteur.IsDBNull(lecteur.GetOrdinal("PRENOM"))
                            ? null : lecteur.GetString(lecteur.GetOrdinal("PRENOM"));
            c.Telephone = lecteur.GetString(lecteur.GetOrdinal("TELEPHONE"));
            c.Mail = lecteur.IsDBNull(lecteur.GetOrdinal("MAIL"))
                            ? null : lecteur.GetString(lecteur.GetOrdinal("MAIL"));
            return c;
        }
    }
}
