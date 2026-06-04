using System.Windows;
using System.Windows.Controls;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Ecran d'accueil du vendeur : acces aux Produits, Commandes et Clients.
    /// </summary>
    public partial class ucPageAccueilVendeur : UserControl
    {
        public ucPageAccueilVendeur()
        {
            InitializeComponent();
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeClients());
        }

        private void BtnProfil_Click(object sender, RoutedEventArgs e)
        {
            // Deconnexion : retour a l'ecran de login.
            ((MainWindow)Application.Current.MainWindow).Naviguer(new PageLogin());
        }
    }
}
