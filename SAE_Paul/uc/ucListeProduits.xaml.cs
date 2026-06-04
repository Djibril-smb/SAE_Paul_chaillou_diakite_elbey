using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Gestion des produits (cote patissier).
    /// Affiche une carte par recette avec filtres categorie + recherche.
    /// Click sur une carte = ouvre la fiche en mode edition.
    /// Bouton "+ Nouvelle recette" = ouvre la fiche en mode creation.
    /// </summary>
    public partial class ucListeProduits : UserControl
    {
        private List<Recette> toutesLesRecettes;
        private Button categorieActive;

        public ucListeProduits()
        {
            InitializeComponent();
            categorieActive = BtnCatTout;
            toutesLesRecettes = new List<Recette>();
            Recharger();
        }

        /// <summary>Recharge la liste des recettes depuis la BD puis rafraichit l'affichage.</summary>
        private void Recharger()
        {
            toutesLesRecettes = Recette.Lister();
            RafraichirCartes();
        }

        /// <summary>Applique le filtre categorie + recherche sur la liste en memoire.</summary>
        private void RafraichirCartes()
        {
            string? cat = null;
            if (categorieActive == BtnCatGateaux) cat = "Gâteaux";
            else if (categorieActive == BtnCatViennoiseries) cat = "Viennoiseries";
            else if (categorieActive == BtnCatPains) cat = "Pains";

            string recherche = TxtRecherche.Text.Trim().ToLower();

            List<Recette> filtres = new List<Recette>();
            foreach (Recette r in toutesLesRecettes)
            {
                bool okCat = (cat == null) || (r.CategorieNom == cat);
                bool okSearch = (recherche == "")
                                || r.RecetteNom.ToLower().Contains(recherche);
                if (okCat && okSearch)
                    filtres.Add(r);
            }

            ListeRecettes.ItemsSource = filtres;
        }

        private void BtnCategorie_Click(object sender, RoutedEventArgs e)
        {
            categorieActive.Tag = null;
            categorieActive = (Button)sender;
            categorieActive.Tag = "ON";
            RafraichirCartes();
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            RafraichirCartes();
        }

        private void BtnNouvelle_Click(object sender, RoutedEventArgs e)
        {
            // Mode creation : on passe null.
            FenetreFicheRecette fenetre = new FenetreFicheRecette(null);
            fenetre.Owner = Window.GetWindow(this);
            fenetre.ShowDialog();

            // Au retour, on recharge (la recette a peut-etre ete creee/modifiee/supprimee).
            Recharger();
        }

        private void CarteRecette_Click(object sender, RoutedEventArgs e)
        {
            Button carte = (Button)sender;
            Recette? recette = carte.DataContext as Recette;
            if (recette == null) return;

            FenetreFicheRecette fenetre = new FenetreFicheRecette(recette);
            fenetre.Owner = Window.GetWindow(this);
            fenetre.ShowDialog();

            Recharger();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            // L'ecran Produits est partage entre vendeur et patissier.
            // On retourne sur le bon accueil selon le role memorise au login.
            UserControl accueil;
            if (Connexion.Role == "VENDEUR")
                accueil = new ucPageAccueilVendeur();
            else
                accueil = new ucPageAccueilChef();

            ((MainWindow)Application.Current.MainWindow).Naviguer(accueil);
        }
    }
}
