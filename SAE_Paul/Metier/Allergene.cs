namespace SAE_Paul.Metier
{
    public class Allergene
    {
        private int allergeneId;
        private string allergeneNom;

        public int AllergeneId
        {
            get => allergeneId;
            set => allergeneId = value;
        }
        public string AllergeneNom
        {
            get => allergeneNom;
            set => allergeneNom = value;
        }

        public Allergene()
        {
            allergeneNom = "";
        }
    }
}
