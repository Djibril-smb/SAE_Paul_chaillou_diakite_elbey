using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Vue cote patissier : liste des commandes a preparer (pas pretes, pas recuperees).
    /// Action : marquer une commande comme prete. Double-clic = detail.
    /// </summary>
    public partial class ucListeCommandesChef : UserControl
    {
        private Button filtreActif;

        public ucListeCommandesChef()
        {
            InitializeComponent();
            filtreActif = BtnFiltreAPreparer;
            Rafraichir();
        }

        private void Rafraichir()
        {
            List<Commande> liste;
            if (filtreActif == BtnFiltreAPreparer)
                liste = Commande.ListerAPreparer();
            else
                liste = Commande.Lister();

            GrilleCommandes.ItemsSource = liste;

            if (liste.Count == 0)
            {
                GrilleCommandes.Visibility = Visibility.Collapsed;
                LblVide.Visibility = Visibility.Visible;
            }
            else
            {
                GrilleCommandes.Visibility = Visibility.Visible;
                LblVide.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnFiltre_Click(object sender, RoutedEventArgs e)
        {
            filtreActif.Tag = null;
            filtreActif = (Button)sender;
            filtreActif.Tag = "ON";
            Rafraichir();
        }

        private void BtnPrete_Click(object sender, RoutedEventArgs e)
        {
            Commande? selectionnee = GrilleCommandes.SelectedItem as Commande;
            if (selectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une commande dans la liste.",
                    "Aucune commande sélectionnée", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectionnee.EstPrete)
            {
                MessageBox.Show("Cette commande est déjà marquée comme prête.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectionnee.EstRecuperee)
            {
                MessageBox.Show("Cette commande est déjà récupérée.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult choix = MessageBox.Show(
                "Marquer la commande n°" + selectionnee.CommandeId
                + " (" + selectionnee.ClientNomComplet + ") comme prête ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix != MessageBoxResult.Yes) return;

            selectionnee.MarquerPrete();
            Rafraichir();
        }

        private void GrilleCommandes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Commande? selectionnee = GrilleCommandes.SelectedItem as Commande;
            if (selectionnee == null) return;

            FenetreDetailCommande fenetre = new FenetreDetailCommande(selectionnee);
            fenetre.Owner = Window.GetWindow(this);
            bool? r = fenetre.ShowDialog();
            if (r == true) Rafraichir();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucPageAccueilChef());
        }
    }
}
