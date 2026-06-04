/*==============================================================*/
/* Migration : ajout d'une image optionnelle aux recettes.      */
/* A executer une seule fois sur les bases existantes (sinon    */
/* re-jouer 01_crebas.sql repart de zero avec la colonne).      */
/*==============================================================*/

ALTER TABLE RECETTE ADD COLUMN IF NOT EXISTS IMAGE_PATH VARCHAR(500) NULL;
