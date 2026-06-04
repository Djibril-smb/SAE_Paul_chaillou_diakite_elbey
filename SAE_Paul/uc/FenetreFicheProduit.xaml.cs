using System.Globalization;
using System.Windows;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Petite modale pour saisir une nouvelle taille (nb parts + prix) d'une recette.
    /// Apres validation, le caller lit NbPartsChoisi et PrixChoisi pour faire l'INSERT.
    /// </summary>
    public partial class FenetreFicheProduit : Window
    {
        private int nbPartsChoisi;
        private decimal prixChoisi;

        public int NbPartsChoisi
        {
            get => nbPartsChoisi;
        }
        public decimal PrixChoisi
        {
            get => prixChoisi;
        }

        public FenetreFicheProduit()
        {
            InitializeComponent();
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtNbParts.Text.Trim(), out int parts) || parts < 1)
            {
                MessageBox.Show("Nombre de parts invalide (entier >= 1).",
                    "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // On accepte la virgule francaise OU le point.
            string texte = TxtPrix.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(texte, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal prix)
                || prix < 0)
            {
                MessageBox.Show("Prix invalide (nombre >= 0).",
                    "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            nbPartsChoisi = parts;
            prixChoisi = decimal.Round(prix, 2);
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
