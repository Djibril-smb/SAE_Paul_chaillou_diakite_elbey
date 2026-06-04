using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Pop-up pour choisir un client : recherche par nom, double-clic = selection,
    /// bouton "Creer ce client" qui ouvre la fiche de creation si le client n'existe pas.
    /// Apres ShowDialog(), le client choisi est accessible via la propriete ClientChoisi.
    /// </summary>
    public partial class FenetreChoixClient : Window
    {
        private Client? clientChoisi;
        public Client? ClientChoisi
        {
            get => clientChoisi;
        }

        public FenetreChoixClient()
        {
            InitializeComponent();
            Rafraichir();
        }

        /// <summary>Recharge la grille selon la recherche.</summary>
        private void Rafraichir()
        {
            string texte = TxtRecherche.Text.Trim();

            List<Client> liste;
            if (texte == "")
                liste = Client.Lister();
            else
                liste = Client.Rechercher(texte);

            GrilleClients.ItemsSource = liste;

            // Message "aucun client" seulement quand on a tape quelque chose et que rien ne matche.
            LblAucun.Visibility = (texte != "" && liste.Count == 0)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            Rafraichir();
        }

        /// <summary>Confirme la selection du client mis en surbrillance.</summary>
        private void Choisir(Client? c)
        {
            if (c == null)
            {
                MessageBox.Show("Sélectionnez d'abord un client dans la liste.",
                    "Aucun client sélectionné", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            clientChoisi = c;
            DialogResult = true;
            Close();
        }

        private void BtnChoisir_Click(object sender, RoutedEventArgs e)
        {
            Choisir(GrilleClients.SelectedItem as Client);
        }

        private void GrilleClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Choisir(GrilleClients.SelectedItem as Client);
        }

        /// <summary>Ouvre la fiche de creation. Si succes, on selectionne directement le nouveau client.</summary>
        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            FenetreFicheClient fiche = new FenetreFicheClient();
            fiche.Owner = this;

            // On peut pre-remplir le nom avec ce qui a ete tape dans la recherche.
            // (optionnel : on s'en passera ici pour rester simple)

            bool? resultat = fiche.ShowDialog();
            if (resultat == true && fiche.ClientCree != null)
            {
                clientChoisi = fiche.ClientCree;
                DialogResult = true;
                Close();
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
