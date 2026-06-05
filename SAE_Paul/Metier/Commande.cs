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


        private string? categorieEvenementNom;
        private string? clientNom;
        private string? clientPrenom;

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
        {
            get => dateRetrait;
            set => dateRetrait = value;
        }
        public decimal Acompte
        {
            get => acompte;
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
        public string? ClientNom
        {
            get => clientNom;
            set => clientNom = value;
        }
        public string? ClientPrenom
        {
            get => clientPrenom;
            set => clientPrenom = value;
        }


        //pour l'affichage : "Marie Dupont" (ou "Dupont" si pas de prenom)

        public string ClientNomComplet
        {
            get => ((clientPrenom ?? "") + " " + (clientNom ?? "")).Trim();
        }

        public Commande()
        {
            lignes = new List<LigneCommande>();
        }

        // -------------------------------------------------------------------
        //  Lectures
        // -------------------------------------------------------------------


        //retourne toutes les commandes d'un client, des plus recentes aux plus anciennes
        //le LEFT JOIN ramene le nom de l'evenement pour l'affichage (peut etre NULL)

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
                            Commande c = LireBase(lecteur);
                            int iEv = lecteur.GetOrdinal("CATEGORIE_EVENEMENT_NOM");
                            c.CategorieEvenementNom = lecteur.IsDBNull(iEv) ? null : lecteur.GetString(iEv);
                            resultat.Add(c);
                        }
                    }
                }
            }

            return resultat;
        }

        //toutes les commandes, des plus recentes aux plus anciennes
        public static List<Commande> Lister()
        {
            return ListerAvecFiltre("ORDER BY C.DATE_RETRAIT DESC, CL.NOM");
        }

        //commandes pas encore recuperees (a preparer)
        public static List<Commande> ListerEnCours()
        {
            return ListerAvecFiltre("WHERE C.EST_RECUPEREE = FALSE "
                                  + "ORDER BY C.DATE_RETRAIT, CL.NOM");
        }

        //commandes deja recuperees (terminees)
        public static List<Commande> ListerRecuperees()
        {
            return ListerAvecFiltre("WHERE C.EST_RECUPEREE = TRUE "
                                  + "ORDER BY C.DATE_RETRAIT DESC, CL.NOM");
        }

        //commandes dont la date de retrait est AUJOURD'HUI
        public static List<Commande> ListerDuJour()
        {
            return ListerAvecFiltre("WHERE C.DATE_RETRAIT = CURRENT_DATE "
                                  + "ORDER BY C.EST_RECUPEREE, CL.NOM, CL.PRENOM");
        }


        //pour le patissier : commandes pas encore pretes ET pas encore recuperees
        //triees par date de retrait croissante (les plus urgentes en haut)

        public static List<Commande> ListerAPreparer()
        {
            return ListerAvecFiltre("WHERE C.EST_PRETE = FALSE AND C.EST_RECUPEREE = FALSE "
                                  + "ORDER BY C.DATE_RETRAIT, CL.NOM");
        }


        //helper interne : meme SELECT pour tous les listings (JOIN client + LEFT JOIN evenement)
        //avec une clause WHERE / ORDER BY differente passee en parametre

        private static List<Commande> ListerAvecFiltre(string whereOrderBy)
        {
            List<Commande> resultat = new List<Commande>();

            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "SELECT C.COMMANDE_ID, C.CATEGORIE_EVENEMENT_ID, C.CLIENT_ID, "
                           + "       C.DATE_CREATION, C.DATE_RETRAIT, C.ACOMPTE, "
                           + "       C.EST_PRETE, C.EST_RECUPEREE, C.TOTAL, "
                           + "       C.DATE_EVENEMENT, C.NB_PERSONNE, "
                           + "       E.CATEGORIE_EVENEMENT_NOM, "
                           + "       CL.NOM AS CLIENT_NOM, CL.PRENOM AS CLIENT_PRENOM "
                           + "FROM COMMANDE C "
                           + "JOIN CLIENT CL ON CL.CLIENT_ID = C.CLIENT_ID "
                           + "LEFT JOIN CATEGORIE_EVENEMENT E "
                           + "       ON E.CATEGORIE_EVENEMENT_ID = C.CATEGORIE_EVENEMENT_ID "
                           + whereOrderBy;

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        Commande c = LireBase(lecteur);

                        int iEv = lecteur.GetOrdinal("CATEGORIE_EVENEMENT_NOM");
                        c.CategorieEvenementNom = lecteur.IsDBNull(iEv) ? null : lecteur.GetString(iEv);

                        c.ClientNom = lecteur.GetString(lecteur.GetOrdinal("CLIENT_NOM"));
                        int iPrenom = lecteur.GetOrdinal("CLIENT_PRENOM");
                        c.ClientPrenom = lecteur.IsDBNull(iPrenom) ? null : lecteur.GetString(iPrenom);

                        resultat.Add(c);
                    }
                }
            }

            return resultat;
        }

        // -------------------------------------------------------------------
        //  Ecritures
        // -------------------------------------------------------------------


        //insere la commande ET toutes ses lignes dans une TRANSACTION :
        //si l'insert d'une ligne echoue, la commande entiere est annulee
        //le CommandeId genere est ensuite reaffecte aux lignes

        public void Enregistrer()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            using (NpgsqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    // 1) Insert de l'en-tete COMMANDE
                    string sqlCmd = "INSERT INTO COMMANDE "
                                  + "  (CATEGORIE_EVENEMENT_ID, CLIENT_ID, DATE_CREATION, DATE_RETRAIT, "
                                  + "   ACOMPTE, TOTAL, DATE_EVENEMENT, NB_PERSONNE) "
                                  + "VALUES (@catEv, @client, @dateCrea, @dateRet, "
                                  + "        @acompte, @total, @dateEv, @nbPers) "
                                  + "RETURNING COMMANDE_ID";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCmd, cn, tx))
                    {
                        cmd.Parameters.AddWithValue("catEv", (object?)categorieEvenementId ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("client", clientId);
                        cmd.Parameters.AddWithValue("dateCrea", dateCreation);
                        cmd.Parameters.AddWithValue("dateRet", dateRetrait);
                        cmd.Parameters.AddWithValue("acompte", acompte);
                        cmd.Parameters.AddWithValue("total", total);
                        cmd.Parameters.AddWithValue("dateEv", (object?)dateEvenement ?? System.DBNull.Value);
                        cmd.Parameters.AddWithValue("nbPers", (object?)nbPersonne ?? System.DBNull.Value);

                        commandeId = (int)cmd.ExecuteScalar()!;
                    }

                    // 2) Insert de chaque LIGNE_COMMANDE
                    string sqlLn = "INSERT INTO LIGNE_COMMANDE "
                                 + "  (COMMANDE_ID, PRODUIT_ID, QUANTITE, EST_DECOUPE) "
                                 + "VALUES (@cmdId, @prodId, @qte, @decoupe)";

                    foreach (LigneCommande ligne in lignes)
                    {
                        ligne.CommandeId = commandeId;

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlLn, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("cmdId", ligne.CommandeId);
                            cmd.Parameters.AddWithValue("prodId", ligne.ProduitId);
                            cmd.Parameters.AddWithValue("qte", ligne.Quantite);
                            cmd.Parameters.AddWithValue("decoupe", ligne.EstDecoupe);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3) On valide la transaction : tout est ecrit en base
                    tx.Commit();
                }
                catch
                {
                    // Echec : on annule TOUT (l'en-tete et les lignes deja inserees)
                    tx.Rollback();
                    throw;
                }
            }
        }


        //marque la commande comme prete (cote patissier)

        public void MarquerPrete()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "UPDATE COMMANDE SET EST_PRETE = TRUE WHERE COMMANDE_ID = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", commandeId);
                    cmd.ExecuteNonQuery();
                }
            }

            estPrete = true;
        }

        
        //marque la commande comme recuperee par le client
        
        public void MarquerRecuperee()
        {
            using (NpgsqlConnection cn = Connexion.Ouvrir())
            {
                string sql = "UPDATE COMMANDE SET EST_RECUPEREE = TRUE WHERE COMMANDE_ID = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", commandeId);
                    cmd.ExecuteNonQuery();
                }
            }

            //on reflete l'etat en memoire pour ne pas avoir a recharger toute la liste
            estRecuperee = true;
        }

        // -------------------------------------------------------------------
        //  Helper de lecture
        // -------------------------------------------------------------------


        //lit les colonnes de la table COMMANDE depuis la ligne courante du lecteur
        //les colonnes issues de JOIN (nom client, nom evenement) sont lues a part
        //par la methode appelante car elles ne sont pas toujours dans le SELECT

        private static Commande LireBase(NpgsqlDataReader lecteur)
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
            return c;
        }
    }
}
