using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Npgsql;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Liste des clients avec recherche, creation, suppression et acces a l'historique.
    /// </summary>
    public partial class ucListeClients : UserControl
    {
        public ucListeClients()
        {
            InitializeComponent();
            Rafraichir();
        }

        /// <summary>Recharge la grille (tous les clients, ou ceux qui matchent la recherche).</summary>
        private void Rafraichir()
        {
            string texte = TxtRecherche.Text.Trim();

            if (texte == "")
                GrilleClients.ItemsSource = Client.Lister();
            else
                GrilleClients.ItemsSource = Client.Rechercher(texte);
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            Rafraichir();
        }

        private void BtnNouveau_Click(object sender, RoutedEventArgs e)
        {
            FenetreFicheClient fenetre = new FenetreFicheClient();
            bool? resultat = fenetre.ShowDialog();
            if (resultat == true)
                Rafraichir();
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            // 1) Verifier qu'une ligne est selectionnee.
            Client? selectionne = GrilleClients.SelectedItem as Client;
            if (selectionne == null)
            {
                MessageBox.Show("Sélectionnez d'abord un client dans la liste.",
                    "Aucun client sélectionné", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2) Demander confirmation.
            string nomComplet = ((selectionne.Prenom ?? "") + " " + selectionne.Nom).Trim();
            MessageBoxResult choix = MessageBox.Show(
                "Voulez-vous vraiment supprimer le client \"" + nomComplet + "\" ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (choix != MessageBoxResult.Yes)
                return;

            // 3) Suppression + gestion du cas "le client a des commandes" (FK restrict).
            try
            {
                selectionne.Supprimer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                // 23503 = violation de cle etrangere : le client est reference par COMMANDE.
                MessageBox.Show("Impossible de supprimer ce client car il a déjà des commandes.",
                    "Suppression refusée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Rafraichir();
        }

        private void GrilleClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // SelectedItem peut etre null si on double-clique a cote d'une ligne ou sur l'entete.
            Client? selectionne = GrilleClients.SelectedItem as Client;
            if (selectionne == null)
                return;

            FenetreCommandesClient fenetre = new FenetreCommandesClient(selectionne);
            fenetre.Owner = Window.GetWindow(this);   // centre la fenetre sur la MainWindow
            fenetre.ShowDialog();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucPageAccueilVendeur());
        }
    }
}
