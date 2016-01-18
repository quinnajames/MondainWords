namespace MondainDeploy
{
    public struct WordData
    { 
        public string Alphagram;
        public bool IsNew;
        public int Probability;
        public int Playability;
        public string Definition;

        public WordData(string alphagram, bool isNew, int probability, int playability, string definition)
        {
            Alphagram = alphagram;
            IsNew = isNew;
            Probability = probability;
            Playability = playability;
            Definition = definition;
        }
    }
}
