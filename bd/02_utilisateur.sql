/*==============================================================*/
/* Table applicative UTILISATEUR                                */
/* Non presente dans le crebas fourni.                          */
/* Role : associer un identifiant de connexion a un role        */
/* (VENDEUR ou PATISSIER) afin d'afficher le bon ecran.         */
/* Le mot de passe saisi a l'ecran de login sert lui a ouvrir   */
/* la connexion PostgreSQL (il n'est donc pas stocke ici).      */
/*==============================================================*/

drop table IF EXISTS UTILISATEUR CASCADE;

create table UTILISATEUR (
   LOGIN   VARCHAR(50)  not null,
   ROLE    VARCHAR(20)  not null,
   constraint PK_UTILISATEUR primary key (LOGIN),
   constraint CK_UTILISATEUR_ROLE check (ROLE in ('VENDEUR', 'PATISSIER'))
);
