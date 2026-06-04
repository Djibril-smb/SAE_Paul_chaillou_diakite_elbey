using System.Windows;
using System.Windows.Controls;
using Npgsql;
using SAE_Paul.Metier;

namespace SAE_Paul
{
    /// <summary>
    /// Ecran de connexion.
    /// Mecanisme : le mot de passe saisi est celui de la base PostgreSQL (commun aux 2 acteurs).
    /// L'identifiant saisi est cherche dans la table UTILISATEUR pour connaitre le ROLE,
    /// puis on affiche l'accueil correspondant (vendeur ou patissier).
    /// </summary>
    public partial class PageLogin : UserControl
    {
        public PageLogin()
        {
            InitializeComponent();
        }

        private void BtnConnexion_Click(object sender, RoutedEventArgs e)
        {
            string identifiant = TxtIdentifiant.Text.Trim();
            string motDePasse = TxtMotDePasse.Text;

            if (identifiant == "" || motDePasse == "")
            {
                MessageBox.Show("Veuillez saisir un identifiant et un mot de passe.",
                    "Champs manquants", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) On memorise le mot de passe : il servira a ouvrir la connexion PostgreSQL.
            Connexion.DefinirMotDePasse(motDePasse);

            Utilisateur? utilisateur;
            try
            {
                // 2) Premiere ouverture = validation du mot de passe + lecture du role.
                utilisateur = Utilisateur.Trouver(identifiant);
            }
            catch (NpgsqlException)
            {
                // Connexion impossible : mot de passe BD errone (ou serveur injoignable).
                MessageBox.Show("Mot de passe incorrect ou base de données injoignable.",
                    "Connexion refusée", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3) Le mot de passe etait bon mais l'identifiant n'existe pas dans UTILISATEUR.
            if (utilisateur == null)
            {
                MessageBox.Show("Identifiant inconnu.",
                    "Connexion refusée", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4) Routage vers le bon ecran d'accueil selon le role.
            MainWindow fenetre = (MainWindow)Application.Current.MainWindow;
            if (utilisateur.Role == "VENDEUR")
                fenetre.Naviguer(new uc.ucPageAccueilVendeur());
            else
                fenetre.Naviguer(new uc.ucPageAccueilChef());
        }
    }
}
