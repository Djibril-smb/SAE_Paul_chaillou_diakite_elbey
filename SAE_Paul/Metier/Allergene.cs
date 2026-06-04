namespace SAE_Paul.Metier
{
    /// <summary>Allergene present dans une recette. Table ALLERGENE.</summary>
    public class Allergene
    {
        public int AllergeneId { get; set; }
        public string AllergeneNom { get; set; }

        public Allergene()
        {
            AllergeneNom = "";
        }
    }
}
