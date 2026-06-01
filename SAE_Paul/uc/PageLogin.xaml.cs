using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SAE_Paul
{
    /// <summary>
    /// Logique d'interaction pour PageLogin.xaml
    /// </summary>
    public partial class PageLogin : UserControl
    {
        public PageLogin()
        {
            InitializeComponent();
        }
       
        private void BtnConnexion_Click(object sender, RoutedEventArgs e)
        {
            string monIdentifiant = TxtIdentifiant.Text;
            string monMotDePasse = TxtMotDePasse.Text;

            // --- C'EST ICI QU'ON METTRA LA CONNEXION A LA BASE DE DONN2ES ---
            // Pour l'instant, on affiche juste un message pour tester que ça récupère bien

            MessageBox.Show("Identifiant tapé : " + monIdentifiant + "\nMot de passe tapé : " + monMotDePasse);
        }
    
    }
}
