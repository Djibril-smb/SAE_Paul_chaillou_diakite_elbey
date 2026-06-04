using System.Windows;
using System.Windows.Controls;

namespace SAE_Paul
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Au demarrage on affiche l'ecran de connexion.
            Naviguer(new PageLogin());
        }

        public void Naviguer(UserControl ecran)
        {
            GridPrincipale.Children.Clear();
            GridPrincipale.Children.Add(ecran);
        }
    }
}
