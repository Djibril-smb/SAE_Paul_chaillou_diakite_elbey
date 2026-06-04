using System.Windows;
using System.Windows.Controls;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Ecran d'accueil du patissier : acces aux Produits et aux Commandes.
    /// </summary>
    public partial class ucPageAccueilChef : UserControl
    {
        public ucPageAccueilChef()
        {
            InitializeComponent();
        }

        private void BtnProfil_Click(object sender, RoutedEventArgs e)
        {
            // Deconnexion : retour a l'ecran de login.
            ((MainWindow)Application.Current.MainWindow).Naviguer(new PageLogin());
        }

        private void BtnCommandes_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeCommandesChef());
        }

        private void BtnProduits_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeProduits());
        }
    }
}
