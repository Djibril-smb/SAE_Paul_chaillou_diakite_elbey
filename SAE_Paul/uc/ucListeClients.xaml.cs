using System.Windows;
using System.Windows.Controls;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Liste des clients avec recherche en direct et acces a la creation d'un client.
    /// </summary>
    public partial class ucListeClients : UserControl
    {
        public ucListeClients()
        {
            InitializeComponent();
            Rafraichir();
        }

        /// <summary>Recharge la grille avec tous les clients (ou le resultat d'une recherche).</summary>
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
            // Ouvre la fiche de creation dans une fenetre modale.
            FenetreFicheClient fenetre = new FenetreFicheClient();
            bool? resultat = fenetre.ShowDialog();

            // Si l'utilisateur a enregistre, on recharge la liste pour voir le nouveau client.
            if (resultat == true)
                Rafraichir();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucPageAccueilVendeur());
        }
    }
}
