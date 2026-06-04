# WIKI — SAE S2.01 Boulangerie Paul

> Documentation interne de l'application. Sert de **révision** pour l'évaluation
> individuelle et de **référence** pour le groupe : où est quoi, comment ça
> fonctionne, et **pourquoi** chaque morceau de code est écrit ainsi.

**Sommaire**
1. [Vue d'ensemble](#1-vue-densemble)
2. [Comment lancer l'appli](#2-comment-lancer-lappli)
3. [Base de données](#3-base-de-données)
4. [Architecture du code](#4-architecture-du-code)
5. [La couche métier](#5-la-couche-métier)
6. [Conventions de codage](#6-conventions-de-codage)
7. [Fonctionnalité Login & navigation](#7-fonctionnalité-login--navigation)
8. [Fonctionnalité Clients (référence du patron)](#8-fonctionnalité-clients-référence-du-patron)
9. [Où trouver quoi (index)](#9-où-trouver-quoi-index)

---

## 1. Vue d'ensemble

Application **WPF .NET 8** locale pour la boulangerie Paul. Deux types d'utilisateurs :
- **Vendeur** : Produits / Commandes / Clients.
- **Pâtissier (chef)** : Produits / Commandes.

L'application a **une seule vraie fenêtre** (`MainWindow`) à l'intérieur de laquelle
on échange des « écrans » (des **UserControls**). On change d'écran via une méthode
centrale `Naviguer(...)`. La seule autre fenêtre est `FenetreFicheClient`, un **pop-up
modal** pour créer un client.

Connexion à PostgreSQL via **Npgsql** (ADO.NET pur, pas d'ORM). Pas d'autre package.

### Schéma de fonctionnement

```
+-------------------+         +--------------------------+
|     App.xaml      | ------> |   MainWindow (Window)    |
| StartupUri = MW   |         |  contient GridPrincipale |
+-------------------+         +-------------+------------+
                                            |
                                            | Naviguer(uc) :
                                            | vide la grille
                                            | et ajoute l'UC
                                            v
            +-------------------------------+--------------------------------+
            |                                                                |
       PageLogin   ucPageAccueilVendeur   ucPageAccueilChef    ucListeClients
                                                                     |
                                                            FenetreFicheClient
                                                            (ShowDialog modal)
```

---

## 2. Comment lancer l'appli

### Pré-requis (poste de dev)
- PostgreSQL 17 installé localement, superutilisateur `postgres` / mot de passe `postgres`.
- Base `boulangerie_paul` créée, alimentée par les 3 scripts du dossier `bd/`
  (dans l'ordre : `01_crebas.sql`, `02_utilisateur.sql`, `03_jeu_essais.sql`).

### Lancement
- Visual Studio : ouvrir `SAE_Paul.sln`, F5.
- Écran de login → identifiant `vendeur` ou `patissier`, mot de passe = celui de
  la base (`postgres`).

### Reset rapide de la base
Pour repartir d'un jeu de données propre, rejouer le script `03_jeu_essais.sql`
(il `DELETE` puis ré-insère).

---

## 3. Base de données

### Tables (10)
| Table | Rôle | PK |
|-------|------|----|
| `ALLERGENE` | liste des allergènes | `ALLERGENE_ID` |
| `CATEGORIE` | catégories de recettes (Gâteaux, Viennoiseries, Pains) | `CATEGORIE_ID` |
| `CATEGORIE_EVENEMENT` | types d'événements (Mariage, Anniversaire...) | `CATEGORIE_EVENEMENT_ID` |
| `CLIENT` | clients de la boulangerie | `CLIENT_ID` |
| `COMMANDE` | une commande pour un événement | `COMMANDE_ID` |
| `LIGNE_COMMANDE` | les lignes d'une commande (produit + quantité) | (`COMMANDE_ID`, `PRODUIT_ID`) |
| `PRODUIT` | recette déclinée en taille + prix | `PRODUIT_ID` |
| `RECETTE` | une recette nommée, dans une catégorie | `RECETTE_ID` |
| `RECETTE_ALLERGENE` | quels allergènes contient une recette | (`ALLERGENE_ID`, `RECETTE_ID`) |
| `UTILISATEUR` *(ajoutée par nous)* | login applicatif → rôle | `LOGIN` |

### Contraintes importantes
- `CLIENT.TELEPHONE` est **unique** → tentative de doublon → code SQL `23505`.
- `PRODUIT` : `PRIX >= 0` et `UNIQUE(RECETTE_ID, NB_PARTS)`.
- `LIGNE_COMMANDE.QUANTITE >= 1`.
- `UTILISATEUR.ROLE` doit valoir `'VENDEUR'` ou `'PATISSIER'` (CHECK).

### Scripts (`bd/`)
- `01_crebas.sql` : schéma fourni.
- `02_utilisateur.sql` : crée la table `UTILISATEUR`.
- `03_jeu_essais.sql` : ré-exécutable, supprime puis ré-insère un jeu réaliste
  (3 catégories, 8 catégories d'événement, 7 allergènes, 15 recettes, 36 produits,
  6 clients, 2 utilisateurs, 4 commandes, 9 lignes de commande).

---

## 4. Architecture du code

### Dossiers
```
SAE_Paul/
  App.xaml(.cs)          point de demarrage (StartupUri = MainWindow)
  MainWindow.xaml(.cs)   fenetre unique + methode Naviguer
  Metier/                classes metier (donnees + acces BD)
    Connexion.cs
    Allergene.cs, Categorie.cs, CategorieEvenement.cs
    Client.cs, Commande.cs, LigneCommande.cs
    Produit.cs, Recette.cs, Utilisateur.cs
  uc/                    UserControls (ecrans) + FenetreFicheClient (modale)
    PageLogin.xaml(.cs)
    ucPageAccueilVendeur.xaml(.cs)
    ucPageAccueilChef.xaml(.cs)
    ucListeClients.xaml(.cs)
    FenetreFicheClient.xaml(.cs)
  img/                   images (logo, icone deconnexion)
bd/                      scripts SQL
```

### Window vs UserControl

| | Window | UserControl |
|---|---|---|
| Apparence | Vraie fenêtre Windows (barre, croix) | Bloc d'interface à insérer **dans** une fenêtre |
| Exemple | `MainWindow`, `FenetreFicheClient` | `PageLogin`, `ucListeClients` |
| Affichage | `Show()` / `ShowDialog()` | `GridPrincipale.Children.Add(uc)` |

**Règle du projet** : une seule fenêtre normale, plein d'UserControls dedans.

### La navigation centrale

Dans `MainWindow.xaml.cs` :

```csharp
public void Naviguer(UserControl ecran)
{
    GridPrincipale.Children.Clear();   // on vide le cadre
    GridPrincipale.Children.Add(ecran); // on y met le nouvel ecran
}
```

Depuis n'importe quel autre écran, on l'appelle ainsi :

```csharp
((MainWindow)Application.Current.MainWindow).Naviguer(new ucListeClients());
```

- `Application.Current.MainWindow` = la fenêtre principale (ouverte au démarrage).
- On la **caste** en `MainWindow` parce que `Application.Current.MainWindow` est de
  type général `Window`, et qu'on veut accéder à **notre** méthode `Naviguer`.

---

## 5. La couche métier

### Principe « par fonctionnalité »

Conformément à la consigne (« découpage par fonctionnalité, pas par couche
technique »), **chaque classe métier porte elle-même ses méthodes d'accès à la
base**. Il n'y a pas de DAO ni de repository séparés. Une classe = une table +
les opérations métier qui la concernent.

### `Connexion.cs` — le point unique de connexion

```csharp
public static class Connexion
{
    private const string Hote = "localhost";
    private const int Port = 5432;
    private const string Base = "boulangerie_paul";
    private const string LoginBd = "postgres";        // login BD commun, en dur
    private static string? _motDePasse;               // saisi au login

    public static void DefinirMotDePasse(string motDePasse) { _motDePasse = motDePasse; }

    public static NpgsqlConnection Ouvrir()
    {
        string chaine = $"Host={Hote};Port={Port};Database={Base};"
                      + $"Username={LoginBd};Password={_motDePasse}";
        NpgsqlConnection cn = new NpgsqlConnection(chaine);
        cn.Open();
        return cn;
    }
}
```

**Pourquoi `static class` ?** Pas besoin d'instance : on appelle `Connexion.Ouvrir()`
directement, partout dans l'appli.

**Pourquoi mémoriser le mot de passe ?** Notre mécanisme : le login BD est en dur
(`postgres`), le mot de passe c'est celui saisi à l'écran de login. On le retient
pour pouvoir le ré-utiliser à chaque appel à la base.

### Les 9 classes métier (rôle + état)

| Classe | Rôle | Méthodes implémentées |
|--------|------|-----------------------|
| `Client` | un client | `Lister()`, `Rechercher(texte)`, `Enregistrer()`, `Supprimer()` |
| `Commande` | en-tête de commande + lignes en mémoire | `Lister()`, `ListerEnCours()`, `ListerRecuperees()`, `ListerDuJour()`, `ListerAPreparer()` (chef), `ListerParClient(clientId)`, `Enregistrer()` (transaction), `MarquerPrete()`, `MarquerRecuperee()` |
| `LigneCommande` | une ligne d'une commande | `ListerParCommande(commandeId)` + propriétés transitoires `Libelle`, `PrixUnitaire`, `SousTotal` (pour la fenêtre de détail) |
| `Produit` | recette + nb parts + prix | `Lister()`, `ListerParRecette(recetteId)`, `Enregistrer()`, `Supprimer()`, `BasculerDisponibilite()` + propriété `Libelle` |
| `Recette` | une recette nommée | `Lister()` (avec catégorie + prix min/max + nb tailles), `Enregistrer()` (INSERT/UPDATE), `Supprimer()`, `ListerMesAllergenes()`, `MettreAJourAllergenes(ids)` |
| `Categorie` | catégorie de recette | `Lister()` |
| `Allergene` | un allergène | `Lister()` |
| `CategorieEvenement` | type d'événement | `Lister()` |
| `Utilisateur` | un compte applicatif | `Trouver(login)` |

> Les classes « propriétés seules » recevront leurs méthodes BD au fur et à mesure
> qu'on implémente les fonctionnalités Produits / Commandes.

---

## 6. Conventions de codage

### Propriétés

**Style adopté (que les profs IUT enseignent)** : attribut privé en minuscule
+ propriété publique multi-lignes en PascalCase.

```csharp
private string nom;

public string Nom
{
    get => nom;
    set => nom = value;
}
```

Raccourci VS : écrire le champ privé, puis **Ctrl+R, Ctrl+E** (« Encapsuler le
champ ») pour générer la propriété.

**Équivalent court** (que les profs préfèrent éviter) : `public string Nom { get; set; }`.

### Types nullable (`?`)

| Syntaxe | Sens |
|---------|------|
| `string` | Chaîne, ne peut pas valoir `null` (le compilateur t'avertit sinon) |
| `string?` | Chaîne qui **peut** valoir `null` |
| `int` | Entier (vaut 0 par défaut) |
| `int?` | Entier **ou** `null` |

Règle simple : **on met `?` quand la colonne BD est NULLABLE**. Exemples :
`Prenom`, `Mail`, `CategorieEvenementId`, `DateEvenement`, `NbPersonne`.

### Création d'objet

On évite la syntaxe d'initialisation `new Truc { X = ... }`. On préfère :

```csharp
Truc t = new Truc();
t.X = ...;
```

Plus lisible, plus proche du modèle « instancier puis remplir » enseigné en cours.

### Noms

- **Classes / propriétés / méthodes** : PascalCase (`Client`, `Nom`, `Lister`).
- **Champs privés / variables locales / paramètres** : camelCase (`nom`,
  `motDePasse`, `texte`).
- **Constantes** : PascalCase (`MotifTelephone`), parfois UPPER_SNAKE.

### Accès BD : toujours les mêmes 4 temps

1. **Ouvrir** la connexion (`using` pour fermer automatiquement).
2. **Préparer la commande** (`NpgsqlCommand` + paramètres `@xxx` — jamais de
   concaténation avec une saisie utilisateur).
3. **Exécuter** :
   - `ExecuteReader()` pour un SELECT (plusieurs lignes).
   - `ExecuteScalar()` pour récupérer **une seule valeur** (souvent un id généré
     via `RETURNING`).
   - `ExecuteNonQuery()` pour un INSERT/UPDATE/DELETE sans retour.
4. **Transformer** la ligne BD en objet C# (`GetString`, `GetInt32`, et
   `IsDBNull` pour les colonnes nullable).

---

## 7. Fonctionnalité Login & navigation

### Le mécanisme

1. L'utilisateur saisit un **identifiant** et un **mot de passe**.
2. Le mot de passe sert à **ouvrir la connexion PostgreSQL** (login BD commun en
   dur dans `Connexion.cs`).
3. L'identifiant est cherché dans la table `UTILISATEUR` → on obtient son rôle.
4. Selon le rôle, on affiche `ucPageAccueilVendeur` ou `ucPageAccueilChef`.

### Le code (`uc/PageLogin.xaml.cs`)

```csharp
private void BtnConnexion_Click(object sender, RoutedEventArgs e)
{
    string identifiant = TxtIdentifiant.Text.Trim();
    string motDePasse  = TxtMotDePasse.Password;      // PasswordBox, pas TextBox

    if (identifiant == "" || motDePasse == "")
    {
        MessageBox.Show("Veuillez saisir un identifiant et un mot de passe.", ...);
        return;
    }

    // 1) On memorise le mdp : il servira a ouvrir la connexion PostgreSQL.
    Connexion.DefinirMotDePasse(motDePasse);

    Utilisateur? utilisateur;
    try
    {
        // 2) Premiere ouverture = validation du mdp + lookup du role.
        utilisateur = Utilisateur.Trouver(identifiant);
    }
    catch (NpgsqlException)
    {
        // Mot de passe BD errone ou serveur injoignable.
        MessageBox.Show("Mot de passe incorrect ou base de données injoignable.", ...);
        return;
    }

    // 3) Mot de passe bon mais identifiant inconnu en base.
    if (utilisateur == null)
    {
        MessageBox.Show("Identifiant inconnu.", ...);
        return;
    }

    // 4) Routage selon le role.
    MainWindow fenetre = (MainWindow)Application.Current.MainWindow;
    if (utilisateur.Role == "VENDEUR")
        fenetre.Naviguer(new uc.ucPageAccueilVendeur());
    else
        fenetre.Naviguer(new uc.ucPageAccueilChef());
}
```

**Points d'examen possibles :**
- **PasswordBox vs TextBox** : `PasswordBox.Password` (string), pas `.Text`. La
  saisie est masquée par `*`.
- **Pourquoi `try/catch NpgsqlException`** : c'est l'exception que lève Npgsql
  quand la connexion échoue (mot de passe faux notamment).
- **Pourquoi `Utilisateur?` (nullable)** : la méthode `Trouver` retourne `null`
  si l'identifiant n'existe pas → on doit tester avant d'utiliser.

### La déconnexion

Sur les pages d'accueil, le bouton rond en haut à droite (`BtnProfil`) revient
au login :
```csharp
((MainWindow)Application.Current.MainWindow).Naviguer(new PageLogin());
```

---

## 8. Fonctionnalité Clients (référence du patron)

C'est **le modèle** : tout autre fonctionnalité (Commandes, Produits...) suit
exactement ces mêmes étapes.

### Lister les clients (`Client.Lister`)

```csharp
public static List<Client> Lister()
{
    List<Client> resultat = new List<Client>();

    using (NpgsqlConnection cn = Connexion.Ouvrir())
    {
        string sql = "SELECT CLIENT_ID, NOM, PRENOM, TELEPHONE, MAIL "
                   + "FROM CLIENT ORDER BY NOM, PRENOM";

        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
        using (NpgsqlDataReader lecteur = cmd.ExecuteReader())
        {
            while (lecteur.Read())
                resultat.Add(LireDepuis(lecteur));
        }
    }

    return resultat;
}
```

- `using` ferme automatiquement la connexion (même en cas d'erreur).
- `ExecuteReader` parcourt le résultat ligne par ligne via `lecteur.Read()`.

### Rechercher (`Client.Rechercher`)

```csharp
string sql = "SELECT ... FROM CLIENT "
           + "WHERE NOM ILIKE @motif OR PRENOM ILIKE @motif OR TELEPHONE ILIKE @motif "
           + "ORDER BY NOM, PRENOM";

cmd.Parameters.AddWithValue("motif", "%" + texte + "%");
```

- **`ILIKE`** = comme `LIKE` mais **insensible à la casse** (spécifique PostgreSQL).
- **`%`** = joker « n'importe quoi ». `%mar%` = « contient mar ».
- **Paramètre `@motif`** = protection contre l'**injection SQL**. Jamais de
  concaténation directe d'une saisie utilisateur dans le SQL.

### Enregistrer (`Client.Enregistrer`)

```csharp
string sql = "INSERT INTO CLIENT (NOM, PRENOM, TELEPHONE, MAIL) "
           + "VALUES (@nom, @prenom, @telephone, @mail) RETURNING CLIENT_ID";

cmd.Parameters.AddWithValue("nom", nom);
cmd.Parameters.AddWithValue("prenom", (object?)prenom ?? System.DBNull.Value);
cmd.Parameters.AddWithValue("telephone", telephone);
cmd.Parameters.AddWithValue("mail", (object?)mail ?? System.DBNull.Value);

clientId = (int)cmd.ExecuteScalar()!;
```

- **`RETURNING CLIENT_ID`** = PostgreSQL nous renvoie l'id auto-généré (SERIAL),
  qu'on récupère avec `ExecuteScalar`.
- **`DBNull.Value`** = la représentation de SQL `NULL` côté ADO.NET. Si
  `prenom` est `null` en C#, on le convertit pour que l'INSERT envoie un vrai
  `NULL` en base (et pas la chaîne `"null"`).
- **`!`** (point d'exclamation) = on dit au compilateur « je sais que
  `ExecuteScalar` ne renverra pas `null` ici » (le RETURNING nous garantit une
  valeur). Sans ça il y aurait un warning.

### Lecture mutualisée (`LireDepuis`)

Méthode privée qui transforme la ligne courante du `NpgsqlDataReader` en objet
`Client`. Évite de répéter le même code dans `Lister` et `Rechercher`.

```csharp
private static Client LireDepuis(NpgsqlDataReader lecteur)
{
    Client c = new Client();
    c.ClientId  = lecteur.GetInt32(lecteur.GetOrdinal("CLIENT_ID"));
    c.Nom       = lecteur.GetString(lecteur.GetOrdinal("NOM"));
    c.Prenom    = lecteur.IsDBNull(lecteur.GetOrdinal("PRENOM"))
                     ? null : lecteur.GetString(lecteur.GetOrdinal("PRENOM"));
    c.Telephone = lecteur.GetString(lecteur.GetOrdinal("TELEPHONE"));
    c.Mail      = lecteur.IsDBNull(lecteur.GetOrdinal("MAIL"))
                     ? null : lecteur.GetString(lecteur.GetOrdinal("MAIL"));
    return c;
}
```

- `GetOrdinal("NOM")` : récupère le **numéro de colonne** correspondant au nom
  donné (plus lisible que `GetString(1)`).
- `IsDBNull(...)` : on **doit** tester avant de lire une colonne nullable (sinon
  exception sur les lignes où la colonne est NULL).

### UI — liste + recherche en direct (`ucListeClients.xaml.cs`)

```csharp
public ucListeClients()
{
    InitializeComponent();
    Rafraichir();                       // chargement initial
}

private void Rafraichir()
{
    string texte = TxtRecherche.Text.Trim();
    if (texte == "")
        GrilleClients.ItemsSource = Client.Lister();
    else
        GrilleClients.ItemsSource = Client.Rechercher(texte);
}

private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
{
    Rafraichir();                       // re-recherche a chaque frappe
}
```

- `ItemsSource` = la propriété du `DataGrid` qui prend une liste d'objets. Le
  DataGrid lit les **propriétés publiques** des objets (grâce aux `Binding`
  écrits dans le XAML : `Binding="{Binding Nom}"`).
- Pas besoin d'`INotifyPropertyChanged` ici : on remplace toute la liste à
  chaque rafraîchissement, le DataGrid se met à jour tout seul.

### Supprimer un client (`Client.Supprimer`)

```csharp
public void Supprimer()
{
    using (NpgsqlConnection cn = Connexion.Ouvrir())
    {
        string sql = "DELETE FROM CLIENT WHERE CLIENT_ID = @id";
        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("id", clientId);
            cmd.ExecuteNonQuery();
        }
    }
}
```

- `ExecuteNonQuery` : à utiliser pour INSERT / UPDATE / DELETE sans valeur de retour.
- **Garde-fou en base** : la clé étrangère `COMMANDE.CLIENT_ID` est en
  `on delete restrict`. Si le client a au moins une commande, PostgreSQL refuse
  le DELETE et lève une `PostgresException` de code **`23503`** (violation de clé
  étrangère). Côté UI on capte ça pour afficher un message clair :
  > « Impossible de supprimer ce client car il a déjà des commandes. »

### Voir les commandes d'un client (`Commande.ListerParClient`)

```csharp
string sql = "SELECT C.COMMANDE_ID, ..., E.CATEGORIE_EVENEMENT_NOM "
           + "FROM COMMANDE C "
           + "LEFT JOIN CATEGORIE_EVENEMENT E "
           + "       ON E.CATEGORIE_EVENEMENT_ID = C.CATEGORIE_EVENEMENT_ID "
           + "WHERE C.CLIENT_ID = @clientId "
           + "ORDER BY C.DATE_CREATION DESC";
```

- **`LEFT JOIN`** : on ramène le nom de l'événement, mais on **garde** les
  commandes qui n'en ont pas (`CATEGORIE_EVENEMENT_ID` nullable). Un `INNER JOIN`
  les aurait filtrées.
- Le résultat est trié des commandes les plus récentes aux plus anciennes.

**Propriété transitoire** : `Commande.CategorieEvenementNom` n'existe pas dans
la table `COMMANDE`. C'est une propriété C# remplie par le `JOIN`, utile pour
l'affichage uniquement. Elle est `null` quand la commande n'a pas d'événement.

### UI — interactions sur la liste

| Action | Effet |
|--------|-------|
| Saisie dans la recherche | Filtre la liste à la volée (`TextChanged`) |
| Sélectionner une ligne + cliquer « Supprimer » | Confirmation, puis `Supprimer()` |
| **Double-clic sur une ligne** | Ouvre `FenetreCommandesClient` (modale) |
| « + Nouveau client » | Ouvre `FenetreFicheClient` (modale) |

Le double-clic utilise l'événement `MouseDoubleClick` du `DataGrid` :
```csharp
private void GrilleClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
{
    Client? selectionne = GrilleClients.SelectedItem as Client;
    if (selectionne == null) return;     // garde : on a peut-etre clique sur l'entete

    FenetreCommandesClient fenetre = new FenetreCommandesClient(selectionne);
    fenetre.Owner = Window.GetWindow(this);
    fenetre.ShowDialog();
}
```

- `as Client` : cast **sûr** — renvoie `null` si l'élément n'est pas un `Client`.
- `Window.GetWindow(this)` : récupère la fenêtre qui contient ce UserControl
  (la `MainWindow`). On la passe en `Owner` pour que le pop-up s'ouvre centré
  dessus et reste lié à elle.

### UI — historique des commandes (`FenetreCommandesClient`)

Fenêtre modale qui reçoit un `Client` en paramètre du constructeur, charge ses
commandes et les affiche dans un `DataGrid`. Si la liste est vide, on cache la
grille et on affiche « Aucune commande pour ce client. »

```csharp
public FenetreCommandesClient(Client client)
{
    InitializeComponent();
    string prenom = client.Prenom ?? "";
    LblTitre.Text = "Commandes de " + (prenom + " " + client.Nom).Trim();

    List<Commande> commandes = Commande.ListerParClient(client.ClientId);
    GrilleCommandes.ItemsSource = commandes;

    if (commandes.Count == 0)
    {
        GrilleCommandes.Visibility = Visibility.Collapsed;
        LblVide.Visibility = Visibility.Visible;
    }
}
```

Formatage des colonnes du DataGrid via `StringFormat` :
```xml
<DataGridTextColumn Header="Total"
    Binding="{Binding Total, StringFormat=\{0:0.00\} €}" Width="80"/>
<DataGridTextColumn Header="Créée le"
    Binding="{Binding DateCreation, StringFormat=\{0:dd/MM/yyyy\}}" Width="*"/>
<DataGridCheckBoxColumn Header="Prête" Binding="{Binding EstPrete}" Width="60"/>
```

- `{0:0.00}` = format décimal avec 2 chiffres après la virgule.
- `{0:dd/MM/yyyy}` = format de date français.
- `DataGridCheckBoxColumn` = colonne avec case à cocher (booléens) ; en lecture
  seule car la grille a `IsReadOnly="True"`.

### UI — création (`FenetreFicheClient.xaml.cs`)

Une **Window modale** ouverte avec `ShowDialog()`. Validation en 5 temps :
1. Nom obligatoire.
2. Téléphone obligatoire **+ format**.
3. Email format (seulement s'il est renseigné).
4. Construire l'objet `Client` (`prenom`/`mail` vides → `null`).
5. `Enregistrer()` + capter le code SQL `23505` (téléphone en doublon).

### Regex de validation

```csharp
private const string MotifTelephone = @"^0[1-9][0-9]{8}$";
private const string MotifMail      = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

if (!Regex.IsMatch(telephone, MotifTelephone)) { /* erreur */ }
```

| Motif | Sens |
|-------|------|
| `^` | début de chaîne |
| `0` | commence par 0 |
| `[1-9]` | un chiffre entre 1 et 9 |
| `[0-9]{8}` | 8 chiffres de 0 à 9 |
| `$` | fin de chaîne |
| `[^@\s]+` | un ou plusieurs caractères, **sauf** `@` et espaces |
| `\.` | un vrai point (le `.` seul signifierait « n'importe quel caractère ») |
| `@""..."` | chaîne « verbatim » : pas besoin d'échapper les `\` |

---

## 9. Fonctionnalité Commandes

C'est la fonctionnalité centrale du sujet. Elle réutilise tout ce qu'on a vu sur
Clients en y ajoutant : une **transaction** (commande + lignes = tout ou rien),
des **JOIN** pour l'affichage et un calcul d'**acompte automatique**.

### Écrans

| Écran | Rôle |
|-------|------|
| `ucListeCommandes` (vendeur) | Liste avec 4 filtres ; « + Nouvelle commande » ; « Marquer récupérée » (impossible tant que pas prête) ; **double-clic = détail** |
| `ucListeCommandesChef` (pâtissier) | Liste « À préparer » par défaut ; « Marquer prête » ; **double-clic = détail** |
| `ucNouvelleCommande` | Formulaire de création : client, événement (type + nb personnes), date retrait, **sélection produits en cartes** (filtres catégorie + recherche), récap, totaux auto |
| `FenetreChoixClient` | Pop-up de sélection d'un client (recherche, double-clic, ou « + Créer ce client ») |
| `FenetreChoixTailleProduit` | Pop-up qui s'ouvre au clic sur une carte recette : choix de la taille + quantité |
| `FenetreDetailCommande` | Détail d'une commande : statut, infos, lignes, boutons « Marquer prête » / « Marquer récupérée » selon le workflow |

### Filtres de la liste (`ucListeCommandes`)

4 boutons en haut, l'actif est en noir, les autres en gris. Au clic sur un
bouton, son `Tag` passe à `"ON"` et le précédent à `null` — le `Style.Trigger`
du XAML change alors la couleur de fond automatiquement (pas besoin de toucher
au XAML depuis le code).

```csharp
private void BtnFiltre_Click(object sender, RoutedEventArgs e)
{
    filtreActif.Tag = null;
    filtreActif = (Button)sender;
    filtreActif.Tag = "ON";
    Rafraichir();
}
```

Côté SQL, on a **un seul SELECT** dans `Commande.ListerAvecFiltre(...)` ;
chaque méthode publique passe juste sa clause `WHERE ... ORDER BY ...` :

```csharp
public static List<Commande> ListerEnCours()
{
    return ListerAvecFiltre("WHERE C.EST_RECUPEREE = FALSE "
                          + "ORDER BY C.DATE_RETRAIT, CL.NOM");
}
```

→ Avantage : la requête (les JOIN, les colonnes) n'existe qu'à **un seul
endroit** ; si on ajoute demain une colonne, on la rajoute une fois.

### Liste du jour (`Commande.ListerDuJour`)

```csharp
string sql = "SELECT C.*, E.CATEGORIE_EVENEMENT_NOM, "
           + "       CL.NOM AS CLIENT_NOM, CL.PRENOM AS CLIENT_PRENOM "
           + "FROM COMMANDE C "
           + "JOIN CLIENT CL ON CL.CLIENT_ID = C.CLIENT_ID "
           + "LEFT JOIN CATEGORIE_EVENEMENT E "
           + "       ON E.CATEGORIE_EVENEMENT_ID = C.CATEGORIE_EVENEMENT_ID "
           + "WHERE C.DATE_RETRAIT = CURRENT_DATE "
           + "ORDER BY C.EST_RECUPEREE, CL.NOM, CL.PRENOM";
```

- **`CURRENT_DATE`** : fonction PostgreSQL qui retourne la date du jour (pas de
  paramètre à passer côté C#).
- **`JOIN CLIENT`** (sans LEFT) : tout commande a un client (`CLIENT_ID NOT NULL`).
- **`LEFT JOIN CATEGORIE_EVENEMENT`** : la catégorie d'événement est facultative.
- **`AS CLIENT_NOM`** : on renomme la colonne `NOM` du client pour éviter une
  collision si on JOINait avec une autre table ayant aussi `NOM`.

### Refactor `LireBase`

Avant on avait `LireDepuis` couplé à la requête « commandes par client » (avec
`CATEGORIE_EVENEMENT_NOM` codé en dur). Maintenant : `LireBase` ne lit que les
colonnes de la table `COMMANDE`, et chaque méthode (`ListerParClient`,
`ListerDuJour`) lit ensuite ses **propres** colonnes de JOIN. C'est plus
souple : si demain on ajoute `ListerNonRecuperees()`, on appelle `LireBase`
puis on ajoute juste les colonnes spécifiques.

### Enregistrement avec TRANSACTION (`Commande.Enregistrer`)

C'est le morceau le plus important à comprendre pour l'éval.

```csharp
public void Enregistrer()
{
    using (NpgsqlConnection cn = Connexion.Ouvrir())
    using (NpgsqlTransaction tx = cn.BeginTransaction())
    {
        try
        {
            // 1) INSERT de l'en-tete COMMANDE -> recupere l'id genere
            using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCmd, cn, tx))
            {
                ...
                commandeId = (int)cmd.ExecuteScalar()!;
            }

            // 2) INSERT de chaque LIGNE_COMMANDE
            foreach (LigneCommande ligne in lignes)
            {
                ligne.CommandeId = commandeId;
                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlLn, cn, tx))
                {
                    ...
                    cmd.ExecuteNonQuery();
                }
            }

            tx.Commit();          // tout est OK : on valide
        }
        catch
        {
            tx.Rollback();        // au moindre probleme : on annule TOUT
            throw;
        }
    }
}
```

**Pourquoi une transaction ?** Si le 1er INSERT (l'en-tête) réussit mais qu'un
INSERT de ligne plante, **sans transaction** on aurait une commande en base
**sans aucune ligne** — donnée incohérente. Avec `BeginTransaction()` :
- soit on appelle `tx.Commit()` à la fin → tout est écrit en base ;
- soit on appelle `tx.Rollback()` (notre `catch`) → **rien** n'est écrit.

C'est le principe **« tout ou rien »** (atomicité, le « A » de **ACID**).

**Important** : le `NpgsqlCommand` doit être créé avec la transaction
(`new NpgsqlCommand(sql, cn, tx)`), sinon il n'en fait pas partie.

### Marquer comme récupérée (`Commande.MarquerRecuperee`)

```csharp
string sql = "UPDATE COMMANDE SET EST_RECUPEREE = TRUE WHERE COMMANDE_ID = @id";
```

`ExecuteNonQuery()` (UPDATE qui ne retourne pas de valeur). On met aussi à jour
le champ `estRecuperee` en mémoire pour ne pas avoir à recharger toute la liste.

### Liste des produits (`Produit.Lister`)

JOIN sur `RECETTE` puis sur `CATEGORIE` pour récupérer les noms (qui ne sont
**pas** dans la table PRODUIT). Filtre `EST_INDISPONIBLE = FALSE`. Tri par
catégorie, recette, nombre de parts.

**Propriété `Libelle`** : calculée à partir des autres propriétés, jamais
stockée. Utilisée par le `ComboBox` via `DisplayMemberPath="Libelle"`.

```csharp
public string Libelle
{
    get => (categorieNom ?? "") + " - " + (recetteNom ?? "") +
           " (" + nbParts + " parts) - " + prix.ToString("0.00") + " €";
}
```

### Formulaire de création (`ucNouvelleCommande`)

Découpé en sections : Client → Événement (facultatif) → Date de retrait →
Produits → Récapitulatif → Totaux → Valider.

**Sélection des produits** : interface inspirée du Figma. En haut, **4 pilules
de filtre** (Tout / Gâteaux / Viennoiseries / Pains) + champ de **recherche**.
En dessous, une grille de **cartes** (`ItemsControl` + `WrapPanel`) où
**chaque carte = une recette** (Fraisier, Croissant…) — pas une taille.

Une carte affiche le nom de la recette, sa catégorie et la **fourchette de
prix** (« de 30 € à 320 € » pour le Fraisier qui existe en 6/8/12/60 parts).
**Click sur une carte** = ouverture d'une **pop-up `FenetreChoixTailleProduit`**
qui propose les tailles disponibles + un champ Quantité. Validation = ajout
au récapitulatif (cumul si le PRODUIT exact est déjà présent).

> **Pourquoi grouper par recette ?** La table `PRODUIT` a un `UNIQUE(RECETTE_ID,
> NB_PARTS)` : il y a **un PRODUIT par taille**, avec son prix figé par la
> boulangerie (les prix ne sont pas linéaires : Fraisier 6 = 30 €, 8 = 44 €,
> 12 = 55 €, 60 = 320 €). On garde donc le schéma tel quel, mais on **regroupe
> à l'écran** côté UI via une petite classe d'affichage `RecetteCard` (`RecetteId`,
> `RecetteNom`, `CategorieNom`, `List<Produit> Produits`, `PrixMin/Max`).

```xml
<ItemsControl x:Name="ListeProduits">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate><WrapPanel/></ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Button Click="CarteProduit_Click" ...>
                ... binding RecetteNom / NbParts / Prix ...
            </Button>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

Côté code, on garde **toute la liste des produits en mémoire** (`tousLesProduits`)
et `RafraichirCartes()` recalcule la liste affichée selon catégorie + recherche.

> **Note** : on a retiré la « date de l'événement » du formulaire — seule la
> date de retrait est utile en pratique. La colonne reste en base (au cas où),
> mais le formulaire ne la remplit plus.

### Fenêtre de détail (`FenetreDetailCommande`)

Ouverte au **double-clic** sur une ligne, depuis le vendeur ou le pâtissier.
Affiche :
- titre + statut visuel (badges « Prête » et « Récupérée » qui passent du gris
  au vert selon l'état) ;
- client + événement + date retrait ;
- DataGrid des **lignes de la commande** (chargées via `LigneCommande.ListerParCommande`) ;
- totaux ;
- **3 boutons** : « Marquer prête », « Marquer récupérée », « Fermer ».

Les boutons « Marquer prête » / « Marquer récupérée » sont **activés selon le
workflow** :
- *Marquer prête* : actif si `!EstPrete && !EstRecuperee`.
- *Marquer récupérée* : actif si `EstPrete && !EstRecuperee` — **on ne peut
  pas marquer récupérée tant que le pâtissier n'a pas marqué prête**.

Quand un bouton est désactivé, son fond passe gris via un `Trigger` sur
`IsEnabled` dans le template du bouton.

### Workflow d'une commande

```
   (création)              (pâtissier)             (vendeur)
   ─────────────►  À PRÉPARER  ─────────────►  PRÊTE  ─────────────►  RÉCUPÉRÉE
                 EST_PRETE=F                 EST_PRETE=T            EST_RECUPEREE=T
                 EST_RECUP=F                 EST_RECUP=F
```

Côté UI, **la vue vendeur** (`ucListeCommandes` → BtnRecuperee_Click) refuse
le passage à récupérée si `EstPrete` est faux. **La fenêtre de détail** désactive
visuellement le bouton dans le même cas. C'est la **même règle**, appliquée
des deux côtés (UI vendeur ET pop-up détail).

### Côté pâtissier (`ucListeCommandesChef`)

Vue simple : 2 filtres (**À préparer** par défaut, **Toutes**), DataGrid des
commandes, bouton « Marquer prête » sur la ligne sélectionnée, double-clic =
fenêtre de détail. Le bouton « Produits » de l'accueil pâtissier affiche un
message d'information : la gestion des produits est sur le périmètre du
coéquipier.

**`LigneAffichage`** : classe interne au UC, utilisée **uniquement à l'écran**
pendant la saisie. Elle a `Produit`, `Quantite`, et des propriétés calculées
(`Libelle`, `PrixUnitaire`, `SousTotal`). Au moment du *Valider*, on la
convertit en `LigneCommande` (qui, elle, va en base).

**`ObservableCollection<LigneAffichage>`** : c'est une liste qui **notifie le
DataGrid** quand on ajoute/retire un élément. Si on utilisait une `List<T>`
normale, le DataGrid ne se mettrait pas à jour tout seul.

**Acompte = total / 4** : recalculé à chaque ajout/retrait via
`RecalculerTotaux()`. La valeur affichée à l'écran est aussi celle envoyée
en base.

### Validation à l'enregistrement

| Règle | Message |
|-------|---------|
| Client choisi | « Choisissez un client. » |
| Date de retrait remplie | « Saisissez une date de retrait. » |
| Date de retrait ≥ aujourd'hui | « La date de retrait doit être aujourd'hui ou plus tard. » |
| Au moins 1 produit | « Ajoutez au moins un produit à la commande. » |
| Nb personnes (si rempli) entier ≥ 1 | « Nombre de personnes invalide. » |
| Quantité par ligne entier ≥ 1 | « Quantité invalide (entier >= 1). » |

### Sélection du client (`FenetreChoixClient`)

- À l'ouverture, on affiche tous les clients.
- La saisie filtre la liste en direct (`Client.Rechercher`).
- Double-clic ou bouton « Choisir » sur une ligne sélectionnée → renvoie le
  client à `ucNouvelleCommande` via la propriété `ClientChoisi`.
- Bouton « + Créer ce client » → ouvre `FenetreFicheClient` ; si la création
  réussit, on **récupère le nouveau client** via sa propriété `ClientCree`
  (ajoutée pour ça) et on le sélectionne directement.

```csharp
private void BtnCreer_Click(object sender, RoutedEventArgs e)
{
    FenetreFicheClient fiche = new FenetreFicheClient();
    fiche.Owner = this;
    bool? resultat = fiche.ShowDialog();
    if (resultat == true && fiche.ClientCree != null)
    {
        clientChoisi = fiche.ClientCree;
        DialogResult = true;
        Close();
    }
}
```

---

## 10. Fonctionnalité Produits (côté pâtissier)

Accessible depuis l'accueil pâtissier → bouton **Produits**.

### Écrans

| Écran | Rôle |
|-------|------|
| `ucListeProduits` | Liste des recettes en cartes (filtres catégorie + recherche), bouton « + Nouvelle recette ». Click sur une carte = édition. |
| `FenetreFicheRecette` | Modale unique qui gère **création** ET **édition**. En création le panneau « Tailles » est masqué jusqu'au 1er Enregistrer. |
| `FenetreFicheProduit` | Petite modale qui saisit une nouvelle taille (nb parts + prix). |

### Méthode `Recette.Lister()` — JOIN + sous-requêtes

```sql
SELECT R.RECETTE_ID, R.CATEGORIE_ID, R.RECETTE_NOM, R.RECETTE_DESCRIPTION,
       C.CATEGORIE_NOM,
       (SELECT MIN(P.PRIX) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS PRIX_MIN,
       (SELECT MAX(P.PRIX) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS PRIX_MAX,
       (SELECT COUNT(*) FROM PRODUIT P WHERE P.RECETTE_ID = R.RECETTE_ID) AS NB_TAILLES
FROM RECETTE R
JOIN CATEGORIE C ON C.CATEGORIE_ID = R.CATEGORIE_ID
ORDER BY C.CATEGORIE_NOM, R.RECETTE_NOM
```

- Les **sous-requêtes** (`SELECT ... FROM PRODUIT ...`) sont exécutées **pour chaque ligne**
  de RECETTE. On les utilise pour les **agrégats** (MIN/MAX/COUNT) qui ne sont pas dans
  RECETTE elle-même.
- **`COUNT(*)`** retourne un `bigint` en PostgreSQL → côté C# on lit avec `GetInt64`
  et on caste en `int`.
- Propriétés transitoires sur `Recette` : `CategorieNom`, `PrixMin`, `PrixMax`,
  `NbTailles`, plus `LibellePrix` (calculée) qui s'affiche sur la carte.

### Méthode `Recette.Enregistrer()` — INSERT ou UPDATE selon l'id

```csharp
public void Enregistrer()
{
    using (NpgsqlConnection cn = Connexion.Ouvrir())
    {
        if (recetteId == 0)
        {
            // CREATION : INSERT ... RETURNING RECETTE_ID
        }
        else
        {
            // MISE A JOUR : UPDATE ... WHERE RECETTE_ID = @id
        }
    }
}
```

`recetteId == 0` signifie « pas encore en base ». Après l'INSERT, on récupère
l'id généré via `RETURNING` ce qui permet à la fenêtre de basculer en mode
édition.

### Allergènes : `MettreAJourAllergenes(List<int>)`

Pattern **« DELETE all + INSERT new »** dans une **transaction** :

```csharp
public void MettreAJourAllergenes(List<int> nouveauxIds)
{
    using (NpgsqlConnection cn = Connexion.Ouvrir())
    using (NpgsqlTransaction tx = cn.BeginTransaction())
    {
        try
        {
            // 1) On supprime TOUS les liens existants
            // 2) On INSERT un lien par allergene coche
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }
}
```

- Plus simple que comparer « ce qui a été ajouté / retiré » entre l'état BD et
  l'état UI. La transaction garantit qu'on ne se retrouve pas avec des liens
  partiellement supprimés.

### `Recette.Supprimer()` — ordre des DELETE

La FK `RECETTE_ALLERGENE.RECETTE_ID` est en `on delete restrict` (pas de
CASCADE). On doit donc :
1. DELETE des liens RECETTE_ALLERGENE pour cette recette.
2. DELETE de la recette elle-même.

→ Encapsulé dans une transaction. Si l'étape 2 échoue (parce qu'il existe
encore des PRODUIT pour cette recette → erreur SQL `23503`), on **rollback**
l'étape 1 aussi : pas d'incohérence (les allergènes ne disparaissent pas
silencieusement).

### Côté UI — `FenetreFicheRecette`

**Mode création** (`recette == null` passée au constructeur) :
- Construit un objet `Recette()` neuf (RecetteId = 0).
- Le panneau « Tailles » est **masqué** (on ne peut pas ajouter une taille à
  une recette qui n'a pas d'id).
- Au clic Enregistrer → INSERT → on récupère l'id → on **bascule sur le mode
  édition** (méthode `PreparerEdition()` qui affiche le panneau Tailles).

**Mode édition** (`recette != null`) :
- Préremplit le formulaire à partir de l'objet reçu.
- Coche les allergènes via `recette.ListerMesAllergenes()`.
- Charge les tailles via `Produit.ListerParRecette(recette.RecetteId)`.
- Boutons sur chaque ligne tailles : **Basculer dispo** (UPDATE immédiat) /
  **Supprimer** (DELETE immédiat, gère le `23503` si le produit est utilisé
  dans une commande).
- Bouton « **Supprimer la recette** » en bas (visible uniquement en édition).

### Images des recettes

Les recettes ont une colonne **`IMAGE_PATH VARCHAR(500) NULL`** dans la table
`RECETTE` (ajoutée par `bd/04_image_path.sql` — déjà appliquée). On y stocke
le **chemin absolu** d'une image sur le poste (`C:\...\fraisier.jpg`).

**Limitation assumée** : si le fichier est déplacé / supprimé / si le projet
tourne sur une autre machine, l'image ne se chargera pas. Pour le rendu de la
SAE on suppose que les images sont stockées dans un dossier stable.

**Picker** (dans `FenetreFicheRecette`) :
```csharp
OpenFileDialog dialog = new OpenFileDialog();
dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
if (dialog.ShowDialog(this) == true)
{
    recette.ImagePath = dialog.FileName;
    AfficherImage(recette.ImagePath);
}
```

**Chargement sans verrou** (important : sinon le fichier reste verrouillé tant
que l'image est affichée) :
```csharp
BitmapImage img = new BitmapImage();
img.BeginInit();
img.CacheOption = BitmapCacheOption.OnLoad;   // copie en memoire puis libere
img.UriSource = new Uri(chemin, UriKind.Absolute);
img.EndInit();
ApercuImage.Source = img;
```

**Cartes** : un `Image` lié à `{Binding ImagePath}` en haut de la carte
(au-dessus du texte). Si le chemin est invalide, le `Image` se laisse vide et
le fond sombre apparaît à la place — pas d'erreur.

### Indisponibilité (`Produit.EstIndisponible`)

Sur la fiche recette, chaque taille a :
- une colonne **« Disponible »** avec une `CheckBox` liée à la propriété
  **transitoire** `EstDisponible` (= `!EstIndisponible`) — sens **positif**,
  plus lisible ;
- un bouton dont le **texte est dynamique** : binding sur la propriété
  transitoire `LibelleBoutonDispo` qui renvoie *« Rendre dispo »* ou
  *« Marquer indispo »* selon l'état actuel ;
- un **fond vert ou orange** selon l'état (via `DataTrigger` sur
  `EstIndisponible` dans le template du bouton) ;
- la **ligne entière** passe en rose clair si indisponible (`DataGrid.RowStyle`
  + `DataTrigger`).

Côté vendeur, `Produit.Lister()` filtre `WHERE EST_INDISPONIBLE = FALSE` →
les produits indisponibles disparaissent automatiquement de la pop-up de choix
des tailles dans « Nouvelle commande ».

### Allergènes en CheckBoxes — pattern `AllergeneAffichage`

Pour gérer l'état coché/non-coché, on utilise une petite classe wrapper :

```csharp
public class AllergeneAffichage
{
    private Allergene allergene;
    private bool isCochee;
    public Allergene Allergene { get => allergene; set => allergene = value; }
    public bool IsCochee { get => isCochee; set => isCochee = value; }
    public int AllergeneId { get => allergene.AllergeneId; }
    public string AllergeneNom { get => allergene.AllergeneNom; }
    ...
}
```

Et dans le XAML, **`IsChecked="{Binding IsCochee, Mode=TwoWay}"`** synchronise
automatiquement la propriété C# avec l'état de la CheckBox. À l'enregistrement
on itère sur la liste pour récupérer les `AllergeneId` cochés et on appelle
`MettreAJourAllergenes`.

---

## 11. Où trouver quoi (index)

| Je veux… | Fichier |
|----------|---------|
| Changer la chaîne de connexion BD | `Metier/Connexion.cs` |
| Ajouter un attribut à un client | `Metier/Client.cs` (et la table SQL) |
| Modifier le SQL `Lister`/`Rechercher` clients | `Metier/Client.cs` |
| Changer ce qui se passe au login | `uc/PageLogin.xaml.cs` |
| Ajouter un bouton sur l'accueil vendeur | `uc/ucPageAccueilVendeur.xaml` (+ Click dans `.cs`) |
| Ajouter une colonne dans la liste clients | `uc/ucListeClients.xaml` (DataGrid.Columns) |
| Changer les règles de validation client | `uc/FenetreFicheClient.xaml.cs` |
| Modifier l'affichage des commandes d'un client | `uc/FenetreCommandesClient.xaml` |
| Modifier la requête « commandes d'un client » | `Metier/Commande.cs` (`ListerParClient`) |
| Modifier la requête « commandes du jour » | `Metier/Commande.cs` (`ListerDuJour`) |
| Toucher au flux de création d'une commande | `uc/ucNouvelleCommande.xaml(.cs)` |
| Changer la transaction d'enregistrement | `Metier/Commande.cs` (`Enregistrer`) |
| Changer la règle « acompte = total / 4 » | `uc/ucNouvelleCommande.xaml.cs` (`RecalculerTotaux` + `BtnValider_Click`) |
| Filtrer les produits affichés au vendeur | `Metier/Produit.cs` (`Lister`) |
| Modifier le formulaire de la fiche recette | `uc/FenetreFicheRecette.xaml(.cs)` |
| Modifier la requête « liste des recettes » | `Metier/Recette.cs` (`Lister`) |
| Ajouter une nouvelle taille à une recette | `uc/FenetreFicheProduit.xaml(.cs)` + `Metier/Produit.cs` (`Enregistrer`) |
| Changer le comportement des allergènes | `Metier/Recette.cs` (`MettreAJourAllergenes`) + `uc/FenetreFicheRecette.xaml.cs` (`AllergeneAffichage`) |
| Naviguer vers un nouvel écran | depuis n'importe quel `.cs` : `((MainWindow)Application.Current.MainWindow).Naviguer(new ...)` |
| Modifier le jeu d'essais BD | `bd/03_jeu_essais.sql` |
| Ajouter / modifier une table | `bd/01_crebas.sql` (rejouer tout) |

---

## À compléter au fur et à mesure

- [ ] Fonctionnalité **Commandes** (créer, lister du jour, restituer).
- [ ] Fonctionnalité **Produits** (gérée par un coéquipier ; documenter quand prête).
- [ ] Diagramme de classes (`classes_NOM.pdf` généré depuis VS).
- [ ] `login.txt` (2 comptes de test) pour le livrable DOC.
