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
| `Client` | un client | `Lister()`, `Rechercher(texte)`, `Enregistrer()` |
| `Utilisateur` | un compte applicatif | `Trouver(login)` |
| `Categorie` | catégorie de recette | propriétés seules |
| `CategorieEvenement` | type d'événement | propriétés seules |
| `Allergene` | un allergène | propriétés seules |
| `Recette` | une recette nommée | propriétés seules |
| `Produit` | recette + nb parts + prix | propriétés seules |
| `LigneCommande` | une ligne d'une commande | propriétés seules |
| `Commande` | en-tête de commande + lignes en mémoire | propriétés seules |

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

## 9. Où trouver quoi (index)

| Je veux… | Fichier |
|----------|---------|
| Changer la chaîne de connexion BD | `Metier/Connexion.cs` |
| Ajouter un attribut à un client | `Metier/Client.cs` (et la table SQL) |
| Modifier le SQL `Lister`/`Rechercher` clients | `Metier/Client.cs` |
| Changer ce qui se passe au login | `uc/PageLogin.xaml.cs` |
| Ajouter un bouton sur l'accueil vendeur | `uc/ucPageAccueilVendeur.xaml` (+ Click dans `.cs`) |
| Ajouter une colonne dans la liste clients | `uc/ucListeClients.xaml` (DataGrid.Columns) |
| Changer les règles de validation client | `uc/FenetreFicheClient.xaml.cs` |
| Naviguer vers un nouvel écran | depuis n'importe quel `.cs` : `((MainWindow)Application.Current.MainWindow).Naviguer(new ...)` |
| Modifier le jeu d'essais BD | `bd/03_jeu_essais.sql` |
| Ajouter / modifier une table | `bd/01_crebas.sql` (rejouer tout) |

---

## À compléter au fur et à mesure

- [ ] Fonctionnalité **Commandes** (créer, lister du jour, restituer).
- [ ] Fonctionnalité **Produits** (gérée par un coéquipier ; documenter quand prête).
- [ ] Diagramme de classes (`classes_NOM.pdf` généré depuis VS).
- [ ] `login.txt` (2 comptes de test) pour le livrable DOC.
