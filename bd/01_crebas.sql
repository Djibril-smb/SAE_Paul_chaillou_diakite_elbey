/*==============================================================*/
/* SGBD : PostgreSQL                                            */
/* Schema de la base "Boulangerie Paul" (script fourni)         */
/*==============================================================*/

drop table IF EXISTS ALLERGENE CASCADE;
drop table IF EXISTS CATEGORIE CASCADE;
drop table IF EXISTS CATEGORIE_EVENEMENT CASCADE;
drop table IF EXISTS CLIENT CASCADE;
drop table IF EXISTS COMMANDE CASCADE;
drop table IF EXISTS LIGNE_COMMANDE CASCADE;
drop table IF EXISTS PRODUIT CASCADE;
drop table IF EXISTS RECETTE CASCADE;
drop table IF EXISTS RECETTE_ALLERGENE CASCADE;

/*==============================================================*/
/* Table : ALLERGENE                                            */
/*==============================================================*/
create table ALLERGENE (
   ALLERGENE_ID         SERIAL               not null,
   ALLERGENE_NOM        VARCHAR(50)          not null,
   constraint PK_ALLERGENE primary key (ALLERGENE_ID),
   constraint AK_UK_ALLERGENE_NOM_ALLERGEN unique (ALLERGENE_NOM)
);

/*==============================================================*/
/* Table : CATEGORIE                                            */
/*==============================================================*/
create table CATEGORIE (
   CATEGORIE_ID         SERIAL               not null,
   CATEGORIE_NOM        VARCHAR(50)          not null,
   constraint PK_CATEGORIE primary key (CATEGORIE_ID),
   constraint AK_UK_CATEGORIE_NOM_CATEGORI unique (CATEGORIE_NOM)
);

/*==============================================================*/
/* Table : CATEGORIE_EVENEMENT                                  */
/*==============================================================*/
create table CATEGORIE_EVENEMENT (
   CATEGORIE_EVENEMENT_ID SERIAL             not null,
   CATEGORIE_EVENEMENT_NOM VARCHAR(50)       not null,
   constraint PK_CATEGORIE_EVENEMENT primary key (CATEGORIE_EVENEMENT_ID),
   constraint AK_UK_CATEGORIE_EVENE_CATEGORI unique (CATEGORIE_EVENEMENT_NOM)
);

/*==============================================================*/
/* Table : CLIENT                                               */
/*==============================================================*/
create table CLIENT (
   CLIENT_ID            SERIAL               not null,
   NOM                  VARCHAR(100)         not null,
   PRENOM               VARCHAR(100)         null,
   TELEPHONE            VARCHAR(15)          not null,
   MAIL                 VARCHAR(100)         null,
   constraint PK_CLIENT primary key (CLIENT_ID),
   constraint AK_UK_CLIENT_TELEPHON_CLIENT unique (TELEPHONE)
);

/*==============================================================*/
/* Table : COMMANDE                                             */
/*==============================================================*/
create table COMMANDE (
   COMMANDE_ID          SERIAL               not null,
   CATEGORIE_EVENEMENT_ID INT4               null,
   CLIENT_ID            INT4                 not null,
   DATE_CREATION        DATE                 not null,
   DATE_RETRAIT         DATE                 not null,
   ACOMPTE              DECIMAL(8,2)         not null,
   EST_PRETE            BOOL                 not null default false,
   EST_RECUPEREE        BOOL                 not null default false,
   TOTAL                DECIMAL(8,2)         not null,
   DATE_EVENEMENT       DATE                 null,
   NB_PERSONNE          INT4                 null,
   constraint PK_COMMANDE primary key (COMMANDE_ID)
);

/*==============================================================*/
/* Table : LIGNE_COMMANDE                                       */
/*==============================================================*/
create table LIGNE_COMMANDE (
   COMMANDE_ID          INT4                 not null,
   PRODUIT_ID           INT4                 not null,
   QUANTITE             INT4                 not null check (QUANTITE >= 1),
   EST_DECOUPE          BOOL                 not null default false,
   constraint PK_LIGNE_COMMANDE primary key (COMMANDE_ID, PRODUIT_ID)
);

/*==============================================================*/
/* Table : PRODUIT                                              */
/*==============================================================*/
create table PRODUIT (
   PRODUIT_ID           SERIAL               not null,
   RECETTE_ID           INT4                 not null,
   EST_INDISPONIBLE     BOOL                 not null default false,
   NB_PARTS             INT4                 not null,
   PRIX                 DECIMAL(8,2)         not null check (PRIX >= 0),
   constraint PK_PRODUIT primary key (PRODUIT_ID)
);

/*==============================================================*/
/* Table : RECETTE                                              */
/*==============================================================*/
create table RECETTE (
   RECETTE_ID           SERIAL               not null,
   CATEGORIE_ID         INT4                 not null,
   RECETTE_NOM          VARCHAR(100)         not null,
   RECETTE_DESCRIPTION  TEXT                 null,
   constraint PK_RECETTE primary key (RECETTE_ID),
   constraint AK_UK_RECETTE_NOM_RECETTE unique (RECETTE_NOM)
);

/*==============================================================*/
/* Table : RECETTE_ALLERGENE                                    */
/*==============================================================*/
create table RECETTE_ALLERGENE (
   ALLERGENE_ID         INT4                 not null,
   RECETTE_ID           INT4                 not null,
   constraint PK_RECETTE_ALLERGENE primary key (ALLERGENE_ID, RECETTE_ID)
);

/*==============================================================*/
/* Cles etrangeres                                              */
/*==============================================================*/
alter table COMMANDE
   add constraint FK_COMMANDE_EFFECTUE_CLIENT foreign key (CLIENT_ID)
      references CLIENT (CLIENT_ID)
      on delete restrict on update restrict;

alter table COMMANDE
   add constraint FK_COMMANDE_REPOND_CATEGORI foreign key (CATEGORIE_EVENEMENT_ID)
      references CATEGORIE_EVENEMENT (CATEGORIE_EVENEMENT_ID)
      on delete restrict on update restrict;

alter table LIGNE_COMMANDE
   add constraint FK_LIGNE_CO_LIGNE_COM_COMMANDE foreign key (COMMANDE_ID)
      references COMMANDE (COMMANDE_ID)
      on delete restrict on update restrict;

alter table LIGNE_COMMANDE
   add constraint FK_LIGNE_CO_LIGNE_COM_PRODUIT foreign key (PRODUIT_ID)
      references PRODUIT (PRODUIT_ID)
      on delete restrict on update restrict;

alter table PRODUIT
   add constraint FK_PRODUIT_A_RECETTE foreign key (RECETTE_ID)
      references RECETTE (RECETTE_ID)
      on delete restrict on update restrict;

alter table RECETTE
   add constraint FK_RECETTE_APPARTIEN_CATEGORI foreign key (CATEGORIE_ID)
      references CATEGORIE (CATEGORIE_ID)
      on delete restrict on update restrict;

alter table RECETTE_ALLERGENE
   add constraint FK_RECETTE__RECETTE_A_ALLERGEN foreign key (ALLERGENE_ID)
      references ALLERGENE (ALLERGENE_ID)
      on delete restrict on update restrict;

alter table RECETTE_ALLERGENE
   add constraint FK_RECETTE__RECETTE_A_RECETTE foreign key (RECETTE_ID)
      references RECETTE (RECETTE_ID)
      on delete restrict on update restrict;

alter table PRODUIT add constraint UK_RECETTE_PARTS unique (RECETTE_ID, NB_PARTS);
