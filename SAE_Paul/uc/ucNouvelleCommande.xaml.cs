using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SAE_Paul.Metier;

namespace SAE_Paul.uc
{
    /// <summary>
    /// Formulaire de creation d'une commande.
    /// Etapes : choisir un client, choisir un evenement (facultatif), saisir la date de retrait,
    /// cliquer sur une carte recette (pop-up taille + quantite), puis valider.
    /// L'acompte est calcule automatiquement (total / 4).
    /// </summary>
    public partial class ucNouvelleCommande : UserControl
    {
        // -------------------------------------------------------------------
        //  Classes d'AFFICHAGE (vivent dans la memoire de l'ecran, pas en base).
        // -------------------------------------------------------------------

        /// <summary>
        /// Une carte recette affichee dans la grille des produits.
        /// Regroupe toutes les tailles disponibles d'une meme recette pour eviter
        /// l'effet "Fraisier x4" qu'on avait avant.
        /// </summary>
        public class RecetteCard
        {
            private int recetteId;
            private string recetteNom;
            private string categorieNom;
            private string? imagePath;
            private List<Produit> produits;
            private decimal prixMin;
            private decimal prixMax;

            public int RecetteId
            {
                get => recetteId;
                set => recetteId = value;
            }
            public string RecetteNom
            {
                get => recetteNom;
                set => recetteNom = value;
            }
            public string CategorieNom
            {
                get => categorieNom;
                set => categorieNom = value;
            }
            public string? ImagePath
            {
                get => imagePath;
                set => imagePath = value;
            }
            public List<Produit> Produits
            {
                get => produits;
                set => produits = value;
            }
            public decimal PrixMin
            {
                get => prixMin;
                set => prixMin = value;
            }
            public decimal PrixMax
            {
                get => prixMax;
                set => prixMax = value;
            }

            /// <summary>Texte affiche sur la carte : un prix unique ou une fourchette.</summary>
            public string LibellePrix
            {
                get => prixMin == prixMax
                        ? prixMin.ToString("0.00") + " €"
                        : "de " + prixMin.ToString("0.00") + " € à " + prixMax.ToString("0.00") + " €";
            }

            public RecetteCard()
            {
                recetteNom = "";
                categorieNom = "";
                produits = new List<Produit>();
            }
        }

        /// <summary>Une ligne dans le recapitulatif de la commande en cours.</summary>
        public class LigneAffichage
        {
            private Produit produit;
            private int quantite;

            public Produit Produit
            {
                get => produit;
                set => produit = value;
            }
            public int Quantite
            {
                get => quantite;
                set => quantite = value;
            }
            public string Libelle
            {
                get => produit.Libelle;
            }
            public decimal PrixUnitaire
            {
                get => produit.Prix;
            }
            public decimal SousTotal
            {
                get => produit.Prix * quantite;
            }

            public LigneAffichage(Produit p, int qte)
            {
                produit = p;
                quantite = qte;
            }
        }

        // -------------------------------------------------------------------
        //  Etat courant.
        // -------------------------------------------------------------------
        private Client? clientChoisi;
        private ObservableCollection<LigneAffichage> lignesAffichees;
        private List<Produit> tousLesProduits;
        private Button categorieActive;

        public ucNouvelleCommande()
        {
            InitializeComponent();

            lignesAffichees = new ObservableCollection<LigneAffichage>();
            GrilleLignes.ItemsSource = lignesAffichees;

            // Date de retrait par defaut = demain, minimum = aujourd'hui.
            DpDateRetrait.SelectedDate = DateTime.Today.AddDays(1);
            DpDateRetrait.DisplayDateStart = DateTime.Today;

            CmbEvenement.ItemsSource = CategorieEvenement.Lister();

            tousLesProduits = Produit.Lister();
            categorieActive = BtnCatTout;
            RafraichirCartes();

            RecalculerTotaux();
        }

        // -------------------------------------------------------------------
        //  Selection du client
        // -------------------------------------------------------------------

        private void BtnChoisirClient_Click(object sender, RoutedEventArgs e)
        {
            FenetreChoixClient fenetre = new FenetreChoixClient();
            fenetre.Owner = Window.GetWindow(this);

            bool? resultat = fenetre.ShowDialog();
            if (resultat == true && fenetre.ClientChoisi != null)
            {
                clientChoisi = fenetre.ClientChoisi;
                LblClient.Text = ((clientChoisi.Prenom ?? "") + " " + clientChoisi.Nom).Trim()
                               + "  -  " + clientChoisi.Telephone;
                LblClient.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        // -------------------------------------------------------------------
        //  Filtres & cartes
        // -------------------------------------------------------------------

        private void BtnCategorie_Click(object sender, RoutedEventArgs e)
        {
            categorieActive.Tag = null;
            categorieActive = (Button)sender;
            categorieActive.Tag = "ON";
            RafraichirCartes();
        }

        private void TxtRechercheProduit_TextChanged(object sender, TextChangedEventArgs e)
        {
            RafraichirCartes();
        }

        /// <summary>
        /// Filtre les produits (categorie + recherche), puis les regroupe par recette
        /// pour n'afficher qu'une carte par recette.
        /// </summary>
        private void RafraichirCartes()
        {
            string? cat = null;
            if (categorieActive == BtnCatGateaux) cat = "Gâteaux";
            else if (categorieActive == BtnCatViennoiseries) cat = "Viennoiseries";
            else if (categorieActive == BtnCatPains) cat = "Pains";

            string recherche = TxtRechercheProduit.Text.Trim().ToLower();

            // 1) Filtrage produits.
            List<Produit> produitsFiltres = new List<Produit>();
            foreach (Produit p in tousLesProduits)
            {
                bool okCat = (cat == null) || (p.CategorieNom == cat);
                bool okSearch = (recherche == "")
                                || (p.RecetteNom != null && p.RecetteNom.ToLower().Contains(recherche));
                if (okCat && okSearch)
                    produitsFiltres.Add(p);
            }

            // 2) Regroupement par recette.
            List<RecetteCard> cartes = GrouperParRecette(produitsFiltres);

            ListeProduits.ItemsSource = cartes;
        }

        /// <summary>
        /// Construit la liste de cartes : une carte par recette, contenant toutes ses tailles,
        /// avec un prix min et un prix max pour l'affichage.
        /// </summary>
        private List<RecetteCard> GrouperParRecette(List<Produit> produits)
        {
            Dictionary<int, RecetteCard> parRecette = new Dictionary<int, RecetteCard>();

            foreach (Produit p in produits)
            {
                if (!parRecette.ContainsKey(p.RecetteId))
                {
                    RecetteCard r = new RecetteCard();
                    r.RecetteId = p.RecetteId;
                    r.RecetteNom = p.RecetteNom ?? "";
                    r.CategorieNom = p.CategorieNom ?? "";
                    r.ImagePath = p.ImagePath;
                    r.PrixMin = p.Prix;
                    r.PrixMax = p.Prix;
                    parRecette[p.RecetteId] = r;
                }

                RecetteCard rc = parRecette[p.RecetteId];
                rc.Produits.Add(p);
                if (p.Prix < rc.PrixMin) rc.PrixMin = p.Prix;
                if (p.Prix > rc.PrixMax) rc.PrixMax = p.Prix;
            }

            List<RecetteCard> resultat = new List<RecetteCard>();
            foreach (KeyValuePair<int, RecetteCard> kv in parRecette)
            {
                // Tailles triees du plus petit au plus grand pour la pop-up.
                kv.Value.Produits.Sort((a, b) => a.NbParts.CompareTo(b.NbParts));
                resultat.Add(kv.Value);
            }

            // Tri categorie puis nom de recette pour un ordre stable a l'ecran.
            resultat.Sort((a, b) =>
            {
                int c = string.Compare(a.CategorieNom, b.CategorieNom, StringComparison.OrdinalIgnoreCase);
                if (c != 0) return c;
                return string.Compare(a.RecetteNom, b.RecetteNom, StringComparison.OrdinalIgnoreCase);
            });

            return resultat;
        }

        private void CarteRecette_Click(object sender, RoutedEventArgs e)
        {
            Button carte = (Button)sender;
            RecetteCard? recette = carte.DataContext as RecetteCard;
            if (recette == null) return;

            FenetreChoixTailleProduit fenetre = new FenetreChoixTailleProduit(recette.RecetteNom, recette.Produits);
            fenetre.Owner = Window.GetWindow(this);

            bool? r = fenetre.ShowDialog();
            if (r == true && fenetre.ProduitChoisi != null)
            {
                AjouterAuPanier(fenetre.ProduitChoisi, fenetre.QuantiteChoisie);
            }
        }

        /// <summary>Ajoute le produit au recap. S'il est deja present, cumule la quantite.</summary>
        private void AjouterAuPanier(Produit produit, int qte)
        {
            LigneAffichage? existante = null;
            foreach (LigneAffichage l in lignesAffichees)
            {
                if (l.Produit.ProduitId == produit.ProduitId)
                {
                    existante = l;
                    break;
                }
            }

            if (existante != null)
            {
                existante.Quantite += qte;
                // Force le redessin (les colonnes calculees ne se re-evaluent pas seules).
                int idx = lignesAffichees.IndexOf(existante);
                lignesAffichees.RemoveAt(idx);
                lignesAffichees.Insert(idx, existante);
            }
            else
            {
                lignesAffichees.Add(new LigneAffichage(produit, qte));
            }

            RecalculerTotaux();
        }

        private void BtnRetirer_Click(object sender, RoutedEventArgs e)
        {
            LigneAffichage? selectionnee = GrilleLignes.SelectedItem as LigneAffichage;
            if (selectionnee == null)
            {
                MessageBox.Show("Sélectionnez une ligne dans le récapitulatif.",
                    "Aucune ligne sélectionnée", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            lignesAffichees.Remove(selectionnee);
            RecalculerTotaux();
        }

        // -------------------------------------------------------------------
        //  Totaux
        // -------------------------------------------------------------------

        private void RecalculerTotaux()
        {
            decimal total = 0;
            foreach (LigneAffichage l in lignesAffichees)
                total += l.SousTotal;

            decimal acompte = total / 4m;

            LblTotal.Text = total.ToString("0.00") + " €";
            LblAcompte.Text = acompte.ToString("0.00") + " €";
        }

        // -------------------------------------------------------------------
        //  Validation
        // -------------------------------------------------------------------

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            if (clientChoisi == null)
            {
                MessageBox.Show("Choisissez un client.", "Client manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DpDateRetrait.SelectedDate == null)
            {
                MessageBox.Show("Saisissez une date de retrait.", "Date manquante",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DateTime dateRetrait = DpDateRetrait.SelectedDate.Value.Date;
            if (dateRetrait < DateTime.Today)
            {
                MessageBox.Show("La date de retrait doit être aujourd'hui ou plus tard.",
                    "Date invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (lignesAffichees.Count == 0)
            {
                MessageBox.Show("Ajoutez au moins un produit à la commande.",
                    "Aucun produit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Nb personnes (facultatif).
            int? nbPersonne = null;
            string texteNb = TxtNbPersonne.Text.Trim();
            if (texteNb != "")
            {
                if (!int.TryParse(texteNb, out int n) || n < 1)
                {
                    MessageBox.Show("Nombre de personnes invalide.", "Saisie invalide",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                nbPersonne = n;
            }

            // Calcul des totaux.
            decimal total = 0;
            foreach (LigneAffichage l in lignesAffichees)
                total += l.SousTotal;
            decimal acompte = decimal.Round(total / 4m, 2);
            total = decimal.Round(total, 2);

            // Construction de l'objet metier.
            Commande commande = new Commande();
            commande.ClientId = clientChoisi.ClientId;
            commande.DateCreation = DateTime.Today;
            commande.DateRetrait = dateRetrait;
            commande.Total = total;
            commande.Acompte = acompte;
            commande.NbPersonne = nbPersonne;

            CategorieEvenement? ev = CmbEvenement.SelectedItem as CategorieEvenement;
            if (ev != null)
                commande.CategorieEvenementId = ev.CategorieEvenementId;

            foreach (LigneAffichage la in lignesAffichees)
            {
                LigneCommande lc = new LigneCommande();
                lc.ProduitId = la.Produit.ProduitId;
                lc.Quantite = la.Quantite;
                lc.EstDecoupe = false;
                commande.Lignes.Add(lc);
            }

            try
            {
                commande.Enregistrer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement :\n" + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Commande n°" + commande.CommandeId + " enregistrée.\n"
                          + "Total : " + total.ToString("0.00") + " €\n"
                          + "Acompte : " + acompte.ToString("0.00") + " €",
                "Commande enregistrée", MessageBoxButton.OK, MessageBoxImage.Information);

            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeCommandes());
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeCommandes());
        }
    }
}
