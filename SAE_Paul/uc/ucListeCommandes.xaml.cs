using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Liste des commandes avec 4 filtres : a preparer (defaut), recuperees, toutes, du jour.
    /// Permet d'en creer une nouvelle et de marquer une commande comme recuperee.
    /// </summary>
    public partial class ucListeCommandes : UserControl
    {
        // Filtre actuellement actif (un des 4 boutons en haut).
        private Button filtreActif;

        public ucListeCommandes()
        {
            InitializeComponent();

            // Par defaut : "A preparer".
            filtreActif = BtnFiltreEnCours;
            Rafraichir();
        }

        /// <summary>Recharge la grille selon le filtre actif.</summary>
        private void Rafraichir()
        {
            List<Commande> liste;

            if (filtreActif == BtnFiltreEnCours)
                liste = Commande.ListerEnCours();
            else if (filtreActif == BtnFiltreRecuperees)
                liste = Commande.ListerRecuperees();
            else if (filtreActif == BtnFiltreDuJour)
                liste = Commande.ListerDuJour();
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
            // On enleve le marqueur "ON" de l'ancien, on le met sur le nouveau.
            filtreActif.Tag = null;
            filtreActif = (Button)sender;
            filtreActif.Tag = "ON";

            Rafraichir();
        }

        private void BtnNouvelle_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucNouvelleCommande());
        }

        private void BtnRecuperee_Click(object sender, RoutedEventArgs e)
        {
            Commande? selectionnee = GrilleCommandes.SelectedItem as Commande;
            if (selectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une commande dans la liste.",
                    "Aucune commande sélectionnée", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectionnee.EstRecuperee)
            {
                MessageBox.Show("Cette commande est déjà marquée comme récupérée.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Workflow : le patissier doit avoir marque la commande prete d'abord.
            if (!selectionnee.EstPrete)
            {
                MessageBox.Show("Cette commande n'est pas encore prête.\n"
                              + "Le pâtissier doit la marquer prête avant qu'elle puisse être récupérée.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult choix = MessageBox.Show(
                "Marquer la commande n°" + selectionnee.CommandeId
                + " (" + selectionnee.ClientNomComplet + ") comme récupérée ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (choix != MessageBoxResult.Yes)
                return;

            selectionnee.MarquerRecuperee();
            Rafraichir();
        }

        private void GrilleCommandes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Commande? selectionnee = GrilleCommandes.SelectedItem as Commande;
            if (selectionnee == null) return;

            FenetreDetailCommande fenetre = new FenetreDetailCommande(selectionnee);
            fenetre.Owner = Window.GetWindow(this);
            bool? resultat = fenetre.ShowDialog();
            if (resultat == true)
                Rafraichir();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucPageAccueilVendeur());
        }
    }
}
