using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Affiche le detail d'une commande : infos, statut, lignes.
    /// Boutons "Marquer prete" et "Marquer recuperee" sont actifs selon le workflow :
    ///   - Marquer prete : possible si pas prete ET pas recuperee
    ///   - Marquer recuperee : possible si prete ET pas recuperee
    /// DialogResult = true si un changement a ete fait (l'appelant rafraichit sa liste).
    /// </summary>
    public partial class FenetreDetailCommande : Window
    {
        private static readonly SolidColorBrush BrushOk = new SolidColorBrush(Color.FromRgb(0x3A, 0x7D, 0x3A));
        private static readonly SolidColorBrush BrushOff = new SolidColorBrush(Color.FromRgb(0x9B, 0x9B, 0x9B));

        private Commande commande;
        private bool aChange;

        public FenetreDetailCommande(Commande c)
        {
            InitializeComponent();
            commande = c;
            Charger();
        }

        /// <summary>Remplit l'ecran a partir de la commande courante.</summary>
        private void Charger()
        {
            LblTitre.Text = "Commande n°" + commande.CommandeId;
            LblClient.Text = commande.ClientNomComplet == ""
                                ? "Client #" + commande.ClientId
                                : commande.ClientNomComplet;
            LblTelephone.Text = "";
            LblDateRetrait.Text = "Retrait le " + commande.DateRetrait.ToString("dd/MM/yyyy");

            // Evenement
            LblEvenement.Text = commande.CategorieEvenementNom ?? "—";
            string sousLigne = "";
            if (commande.NbPersonne != null)
                sousLigne = commande.NbPersonne + " personne" + (commande.NbPersonne > 1 ? "s" : "");
            LblNbPersonne.Text = sousLigne;

            // Statut (badges)
            BadgePrete.Background = commande.EstPrete ? BrushOk : BrushOff;
            BadgeRecuperee.Background = commande.EstRecuperee ? BrushOk : BrushOff;

            // Totaux
            LblTotal.Text = commande.Total.ToString("0.00") + " €";
            LblAcompte.Text = commande.Acompte.ToString("0.00") + " €";

            // Lignes
            List<LigneCommande> lignes = LigneCommande.ListerParCommande(commande.CommandeId);
            GrilleLignes.ItemsSource = lignes;

            // Etats des boutons (workflow)
            BtnMarquerPrete.IsEnabled = !commande.EstPrete && !commande.EstRecuperee;
            BtnMarquerRecuperee.IsEnabled = commande.EstPrete && !commande.EstRecuperee;
        }

        private void BtnMarquerPrete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult choix = MessageBox.Show(
                "Marquer la commande n°" + commande.CommandeId + " comme prête ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix != MessageBoxResult.Yes) return;

            commande.MarquerPrete();
            aChange = true;
            Charger();
        }

        private void BtnMarquerRecuperee_Click(object sender, RoutedEventArgs e)
        {
            // Garde-fou : le bouton n'est actif que si EstPrete, mais on double-controle.
            if (!commande.EstPrete)
            {
                MessageBox.Show("La commande doit être marquée prête avant d'être récupérée.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult choix = MessageBox.Show(
                "Marquer la commande n°" + commande.CommandeId + " comme récupérée ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix != MessageBoxResult.Yes) return;

            commande.MarquerRecuperee();
            aChange = true;
            Charger();
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = aChange ? true : (bool?)null;
            Close();
        }
    }
}
