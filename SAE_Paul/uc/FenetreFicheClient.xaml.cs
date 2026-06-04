using System.Windows;
using Npgsql;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Fenetre modale de creation d'un client.
    /// Valide les champs obligatoires puis appelle Client.Enregistrer().
    /// DialogResult = true si l'enregistrement a reussi (la liste se rafraichit alors).
    /// </summary>
    public partial class FenetreFicheClient : Window
    {
        public FenetreFicheClient()
        {
            InitializeComponent();
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            string nom = TxtNom.Text.Trim();
            string telephone = TxtTelephone.Text.Trim();

            // 1) Validation des champs obligatoires (NOM et TELEPHONE sont NOT NULL en base).
            if (nom == "")
            {
                MessageBox.Show("Le nom est obligatoire.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (telephone == "")
            {
                MessageBox.Show("Le téléphone est obligatoire.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Construction de l'objet metier a partir de la saisie.
            Client client = new Client
            {
                Nom = nom,
                Prenom = TxtPrenom.Text.Trim() == "" ? null : TxtPrenom.Text.Trim(),
                Telephone = telephone,
                Mail = TxtMail.Text.Trim() == "" ? null : TxtMail.Text.Trim()
            };

            // 3) Enregistrement + gestion de l'erreur "telephone deja utilise".
            try
            {
                client.Enregistrer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                // 23505 = violation de contrainte unique (ici TELEPHONE).
                MessageBox.Show("Un client avec ce numéro de téléphone existe déjà.",
                    "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4) Tout s'est bien passe : on ferme en signalant le succes.
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
