using System;
using System.Collections.Generic;
using Npgsql;

namespace SAE_Paul.Metier
{
    public class Commande
    {
        private int commandeId;
        private int? categorieEvenementId;
        private int clientId;
        private DateTime dateCreation;
        private DateTime dateRetrait;
        private decimal acompte;
        private bool estPrete;
        private bool estRecuperee;
        private decimal total;
        private DateTime? dateEvenement;
        private int? nbPersonne;
        private List<LigneCommande> lignes;

        // Champ d'affichage uniquement (rempli par JOIN, pas stocke dans COMMANDE).
        private string? categorieEvenementNom;

        public int CommandeId 
        { 
            get => commandeId; 
            set => commandeId = value; 
        }
        public int? CategorieEvenementId 
        { 
            get => categorieEvenementId;
            set => categorieEvenementId = value; 
        }
        
        public int ClientId 
        { 
            get => clientId;
            set => clientId = value; 
        }
        public DateTime DateCreation 
        { 
            get => dateCreation;
            set => dateCreation = value; 
        }
        public DateTime DateRetrait 
        {   get => dateRetrait;
            set => dateRetrait = value; 
        }
        public decimal Acompte 
        {   get => acompte;
            set => acompte = value; 
        }
        public bool EstPrete 
        { 
            get => estPrete;
            set => estPrete = value; 
        }
        public bool EstRecuperee 
        { 
            get => estRecuperee;
            set => estRecuperee = value; 
        }
        public decimal Total 
        { 
            get => total; 
            set => total = value; 
        }
        public DateTime? DateEvenement 
        { 
            get => dateEvenement;
            set => dateEvenement = value; 
        }
        public int? NbPersonne 
        { 
            get => nbPersonne;
            set => nbPersonne = value; 
        }
        public List<LigneCommande> Lignes 
        {
            get => lignes;
            set => lignes = value; 
        }


        public string? CategorieEvenementNom
        {
            get => categorieEvenementNom;
            set => categorieEvenementNom = value;
        }

        public Commande()
        {
            lignes = new List<LigneCommande>();
        }

        /// <summary>
        /// Retourne toutes les commandes d'un client, des plus recentes aux plus anciennes.
        /// Le LEFT JOIN ramene le nom de l'evenement pour l'affichage (peut etre NULL).
        /// </summary>
        public static List<Commande> ListerParClient(int clientId)
        {
            List<Commande> resultat = new List<Commande>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT C.COMMANDE_ID, C.CATEGORIE_EVENEMENT_ID, C.CLIENT_ID, "
                           + "       C.DATE_CREATION, C.DATE_RETRAIT, C.ACOMPTE, "
                           + "       C.EST_PRETE, C.EST_RECUPEREE, C.TOTAL, "
                           + "       C.DATE_EVENEMENT, C.NB_PERSONNE, "
                           + "       E.CATEGORIE_EVENEMENT_NOM "
                           + "FROM COMMANDE C "
                           + "LEFT JOIN CATEGORIE_EVENEMENT E "
                           + "       ON E.CATEGORIE_EVENEMENT_ID = C.CATEGORIE_EVENEMENT_ID "
                           + "WHERE C.CLIENT_ID = @clientId "
                           + "ORDER BY C.DATE_CREATION DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("clientId", clientId);

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
        /// Fabrique un objet Commande a partir de la ligne courante du lecteur.
        /// </summary>
        private static Commande LireDepuis(NpgsqlDataReader lecteur)
        {
            Commande c = new Commande();
            c.CommandeId = lecteur.GetInt32(lecteur.GetOrdinal("COMMANDE_ID"));
            c.CategorieEvenementId = lecteur.IsDBNull(lecteur.GetOrdinal("CATEGORIE_EVENEMENT_ID"))
                            ? (int?)null : lecteur.GetInt32(lecteur.GetOrdinal("CATEGORIE_EVENEMENT_ID"));
            c.ClientId = lecteur.GetInt32(lecteur.GetOrdinal("CLIENT_ID"));
            c.DateCreation = lecteur.GetDateTime(lecteur.GetOrdinal("DATE_CREATION"));
            c.DateRetrait = lecteur.GetDateTime(lecteur.GetOrdinal("DATE_RETRAIT"));
            c.Acompte = lecteur.GetDecimal(lecteur.GetOrdinal("ACOMPTE"));
            c.EstPrete = lecteur.GetBoolean(lecteur.GetOrdinal("EST_PRETE"));
            c.EstRecuperee = lecteur.GetBoolean(lecteur.GetOrdinal("EST_RECUPEREE"));
            c.Total = lecteur.GetDecimal(lecteur.GetOrdinal("TOTAL"));
            c.DateEvenement = lecteur.IsDBNull(lecteur.GetOrdinal("DATE_EVENEMENT"))
                            ? (DateTime?)null : lecteur.GetDateTime(lecteur.GetOrdinal("DATE_EVENEMENT"));
            c.NbPersonne = lecteur.IsDBNull(lecteur.GetOrdinal("NB_PERSONNE"))
                            ? (int?)null : lecteur.GetInt32(lecteur.GetOrdinal("NB_PERSONNE"));

            // Colonne du JOIN : presente seulement si la requete la SELECTionne.
            int iNom = lecteur.GetOrdinal("CATEGORIE_EVENEMENT_NOM");
            c.CategorieEvenementNom = lecteur.IsDBNull(iNom) ? null : lecteur.GetString(iNom);

            return c;
        }
    }
}
