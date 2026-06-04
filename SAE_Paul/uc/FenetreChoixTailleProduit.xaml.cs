using System.Collections.Generic;
using System.Windows;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Pop-up qui propose les tailles disponibles d'une recette + une quantite.
    /// Apres ShowDialog(), on lit ProduitChoisi (la taille selectionnee) et QuantiteChoisie.
    /// </summary>
    public partial class FenetreChoixTailleProduit : Window
    {
        private Produit? produitChoisi;
        private int quantiteChoisie;

        public Produit? ProduitChoisi
        {
            get => produitChoisi;
        }
        public int QuantiteChoisie
        {
            get => quantiteChoisie;
        }

        public FenetreChoixTailleProduit(string nomRecette, List<Produit> tailles)
        {
            InitializeComponent();
            LblTitre.Text = nomRecette;
            ListeTailles.ItemsSource = tailles;

            // Pre-selection : la plus petite taille.
            if (tailles.Count > 0)
                ListeTailles.SelectedIndex = 0;
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            Produit? p = ListeTailles.SelectedItem as Produit;
            if (p == null)
            {
                MessageBox.Show("Choisissez une taille.", "Sélection manquante",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtQuantite.Text.Trim(), out int qte) || qte < 1)
            {
                MessageBox.Show("Quantité invalide (entier >= 1).", "Quantité invalide",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            produitChoisi = p;
            quantiteChoisie = qte;
            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
