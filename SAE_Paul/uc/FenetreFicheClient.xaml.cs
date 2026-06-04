using System.Text.RegularExpressions;
using System.Windows;
using Npgsql;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    public partial class FenetreFicheClient : Window
    {
        // Telephone francais : 10 chiffres, commence par 0 puis 1-9, puis 8 chiffres. Ex : 0612345678.
        private const string MotifTelephone = @"^0[1-9][0-9]{8}$";

        // Email simple : au moins un caractere, puis @, puis un domaine, puis . puis une extension.
        private const string MotifMail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // Client effectivement cree (null si annulation). Recuperable apres ShowDialog().
        private Client? clientCree;
        public Client? ClientCree
        {
            get => clientCree;
        }

        public FenetreFicheClient()
        {
            InitializeComponent();
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            string nom = TxtNom.Text.Trim();
            string prenom = TxtPrenom.Text.Trim();
            string telephone = TxtTelephone.Text.Trim();
            string mail = TxtMail.Text.Trim();

            // 1) Champ NOM obligatoire (NOT NULL en base).
            if (nom == "")
            {
                MessageBox.Show("Le nom est obligatoire.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Champ TELEPHONE obligatoire + format.
            if (telephone == "")
            {
                MessageBox.Show("Le téléphone est obligatoire.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(telephone, MotifTelephone))
            {
                MessageBox.Show("Numéro de téléphone invalide.\nFormat attendu : 10 chiffres commençant par 0 (ex. 0612345678).",
                    "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) MAIL facultatif : on ne controle le format que s'il est renseigne.
            if (mail != "" && !Regex.IsMatch(mail, MotifMail))
            {
                MessageBox.Show("Adresse email invalide.\nFormat attendu : nom@domaine.fr",
                    "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4) Construction de l'objet metier.
            Client client = new Client();
            client.Nom = nom;
            client.Prenom = prenom == "" ? null : prenom;
            client.Telephone = telephone;
            client.Mail = mail == "" ? null : mail;

            // 5) Enregistrement + gestion du doublon de telephone.
            try
            {
                client.Enregistrer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                MessageBox.Show("Un client avec ce numéro de téléphone existe déjà.",
                    "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            clientCree = client;
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
