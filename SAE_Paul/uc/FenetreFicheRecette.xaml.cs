using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Npgsql;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Modale unique pour creer (recette == null) ou modifier une recette.
    /// En mode creation : pas encore d'id => le panneau "Tailles" est masque
    /// jusqu'a ce qu'on clique Enregistrer (qui fait l'INSERT et bascule en mode edition).
    /// En mode edition : on peut tout modifier + gerer les tailles + supprimer la recette.
    /// </summary>
    public partial class FenetreFicheRecette : Window
    {
        // Petite classe d'affichage pour les checkboxes d'allergenes.
        public class AllergeneAffichage
        {
            private Allergene allergene;
            private bool isCochee;

            public Allergene Allergene
            {
                get => allergene;
                set => allergene = value;
            }
            public bool IsCochee
            {
                get => isCochee;
                set => isCochee = value;
            }
            public int AllergeneId
            {
                get => allergene.AllergeneId;
            }
            public string AllergeneNom
            {
                get => allergene.AllergeneNom;
            }

            public AllergeneAffichage(Allergene a, bool coche)
            {
                allergene = a;
                isCochee = coche;
            }
        }

        private Recette recette;
        private List<AllergeneAffichage> allergenesAffiches;

        public FenetreFicheRecette(Recette? r)
        {
            InitializeComponent();

            // Mode creation : on construit une recette vierge avec id = 0.
            recette = r ?? new Recette();
            allergenesAffiches = new List<AllergeneAffichage>();

            CmbCategorie.ItemsSource = Categorie.Lister();
            ChargerAllergenes();

            if (recette.RecetteId == 0)
                PreparerCreation();
            else
                PreparerEdition();
        }

        // -------------------------------------------------------------------
        //  Preparation des deux modes
        // -------------------------------------------------------------------

        private void PreparerCreation()
        {
            LblTitre.Text = "Nouvelle recette";
            PanneauTailles.Visibility = Visibility.Collapsed;
            LblAideCreation.Visibility = Visibility.Visible;
            BtnSupprimerRecette.Visibility = Visibility.Collapsed;
        }

        private void PreparerEdition()
        {
            LblTitre.Text = "Recette : " + recette.RecetteNom;
            TxtNom.Text = recette.RecetteNom;
            TxtDescription.Text = recette.RecetteDescription ?? "";
            SelectionnerCategorie(recette.CategorieId);
            CocherAllergenes(recette.ListerMesAllergenes());
            AfficherImage(recette.ImagePath);

            PanneauTailles.Visibility = Visibility.Visible;
            LblAideCreation.Visibility = Visibility.Collapsed;
            BtnSupprimerRecette.Visibility = Visibility.Visible;

            ChargerTailles();
        }

        /// <summary>Met a jour l'apercu image + le libelle nom de fichier.</summary>
        private void AfficherImage(string? chemin)
        {
            if (string.IsNullOrEmpty(chemin) || !File.Exists(chemin))
            {
                ApercuImage.Source = null;
                LblImageNom.Text = "Aucune image";
                return;
            }

            try
            {
                // BeginInit/EndInit + CacheOption.OnLoad pour ne pas garder un verrou sur le fichier.
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(chemin, UriKind.Absolute);
                img.EndInit();
                ApercuImage.Source = img;
                LblImageNom.Text = Path.GetFileName(chemin);
            }
            catch
            {
                ApercuImage.Source = null;
                LblImageNom.Text = "Image illisible";
            }
        }

        private void SelectionnerCategorie(int categorieId)
        {
            foreach (object item in CmbCategorie.Items)
            {
                Categorie c = (Categorie)item;
                if (c.CategorieId == categorieId)
                {
                    CmbCategorie.SelectedItem = c;
                    break;
                }
            }
        }

        private void ChargerAllergenes()
        {
            allergenesAffiches.Clear();
            foreach (Allergene a in Allergene.Lister())
                allergenesAffiches.Add(new AllergeneAffichage(a, false));
            ListeAllergenes.ItemsSource = allergenesAffiches;
        }

        private void CocherAllergenes(List<int> idsCoches)
        {
            foreach (AllergeneAffichage aa in allergenesAffiches)
                aa.IsCochee = idsCoches.Contains(aa.AllergeneId);

            // Force la mise a jour des CheckBox a l'ecran.
            ListeAllergenes.ItemsSource = null;
            ListeAllergenes.ItemsSource = allergenesAffiches;
        }

        private void ChargerTailles()
        {
            GrilleTailles.ItemsSource = Produit.ListerParRecette(recette.RecetteId);
        }

        // -------------------------------------------------------------------
        //  Bouton Enregistrer (INSERT en creation, UPDATE en edition)
        // -------------------------------------------------------------------

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            string nom = TxtNom.Text.Trim();
            if (nom == "")
            {
                MessageBox.Show("Le nom est obligatoire.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Categorie? cat = CmbCategorie.SelectedItem as Categorie;
            if (cat == null)
            {
                MessageBox.Show("Choisissez une catégorie.", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // On copie les saisies dans l'objet metier.
            recette.RecetteNom = nom;
            recette.CategorieId = cat.CategorieId;
            string desc = TxtDescription.Text.Trim();
            recette.RecetteDescription = desc == "" ? null : desc;

            try
            {
                recette.Enregistrer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                MessageBox.Show("Une recette avec ce nom existe déjà.",
                    "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // On synchronise les allergenes.
            List<int> idsCoches = new List<int>();
            foreach (AllergeneAffichage aa in allergenesAffiches)
                if (aa.IsCochee) idsCoches.Add(aa.AllergeneId);
            recette.MettreAJourAllergenes(idsCoches);

            MessageBox.Show("Recette enregistrée.", "OK",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Apres l'INSERT initial on bascule l'ecran en mode edition.
            PreparerEdition();
        }

        // -------------------------------------------------------------------
        //  Gestion des tailles
        // -------------------------------------------------------------------

        private void BtnAjouterTaille_Click(object sender, RoutedEventArgs e)
        {
            FenetreFicheProduit fenetre = new FenetreFicheProduit();
            fenetre.Owner = this;

            bool? r = fenetre.ShowDialog();
            if (r != true) return;

            Produit p = new Produit();
            p.RecetteId = recette.RecetteId;
            p.NbParts = fenetre.NbPartsChoisi;
            p.Prix = fenetre.PrixChoisi;
            p.EstIndisponible = false;

            try
            {
                p.Enregistrer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                MessageBox.Show("Cette taille (nombre de parts) existe déjà pour cette recette.",
                    "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChargerTailles();
        }

        private void BtnBasculerDispo_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement btn = (FrameworkElement)sender;
            Produit? p = btn.Tag as Produit;
            if (p == null) return;

            p.BasculerDisponibilite();
            ChargerTailles();
        }

        private void BtnSupprimerTaille_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement btn = (FrameworkElement)sender;
            Produit? p = btn.Tag as Produit;
            if (p == null) return;

            MessageBoxResult choix = MessageBox.Show(
                "Supprimer la taille de " + p.NbParts + " parts ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix != MessageBoxResult.Yes) return;

            try
            {
                p.Supprimer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                MessageBox.Show("Impossible : cette taille est utilisée dans des commandes existantes.\n"
                              + "Vous pouvez la marquer indisponible à la place.",
                    "Suppression refusée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChargerTailles();
        }

        // -------------------------------------------------------------------
        //  Bouton Supprimer la recette
        // -------------------------------------------------------------------

        private void BtnSupprimerRecette_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult choix = MessageBox.Show(
                "Supprimer la recette \"" + recette.RecetteNom + "\" ?\n\n"
                + "Si elle a des tailles utilisées dans des commandes, la suppression sera refusée.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix != MessageBoxResult.Yes) return;

            try
            {
                recette.Supprimer();
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                MessageBox.Show("Impossible : la recette a des tailles existantes.\n"
                              + "Supprimez d'abord toutes ses tailles (ou marquez-les indisponibles).",
                    "Suppression refusée", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Close();
        }

        private void BtnFermer_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // -------------------------------------------------------------------
        //  Gestion de l'image
        // -------------------------------------------------------------------

        private void BtnChoisirImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisir une image pour la recette";
            dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
            if (dialog.ShowDialog(this) == true)
            {
                // On stocke le chemin absolu dans l'objet metier. Il sera persiste
                // a l'appui sur "Enregistrer".
                recette.ImagePath = dialog.FileName;
                AfficherImage(recette.ImagePath);
            }
        }

        private void BtnRetirerImage_Click(object sender, RoutedEventArgs e)
        {
            recette.ImagePath = null;
            AfficherImage(null);
        }
    }
}
