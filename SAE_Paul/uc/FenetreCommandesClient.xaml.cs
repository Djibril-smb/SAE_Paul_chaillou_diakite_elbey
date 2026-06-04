using System.Collections.Generic;
using System.Windows;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Fenetre modale qui affiche l'historique des commandes d'un client donne.
    /// On lui passe le client en parametre via le constructeur.
    /// </summary>
    public partial class FenetreCommandesClient : Window
    {
        public FenetreCommandesClient(Client client)
        {
            InitializeComponent();

            // Titre dynamique : "Commandes de Marie Dupont"
            string prenom = client.Prenom ?? "";
            LblTitre.Text = "Commandes de " + (prenom + " " + client.Nom).Trim();

            // Chargement des commandes du client.
            List<Commande> commandes = Commande.ListerParClient(client.ClientId);
            GrilleCommandes.ItemsSource = commandes;

            // Si aucune commande : on cache la grille, on affiche un message a la place.
            if (commandes.Count == 0)
            {
                GrilleCommandes.Visibility = Visibility.Collapsed;
                LblVide.Visibility = Visibility.Visible;
            }
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
