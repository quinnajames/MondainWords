namespace MondainDeploy
{
    /// <summary>
    /// Struct to represent the properties of a word.
    /// </summary>
    public struct WordData
    { 
        public string Alphagram;
        public bool IsNew;
        public int Probability;
        public int Playability;
        public string Definition;

        /// <summary>
        /// Simple constructor, sets variables equal to input params.
        /// </summary>
        /// <param name="alphagram">The alphagram associated with the word.</param>
        /// <param name="isNew">Whether the word was newly added to the lexicon in its most recent update.</param>
        /// <param name="probability">Rank order of how likely the word is to be drawn in word games.</param>
        /// <param name="playability">Rank order of how useful the word is in word games.</param>
        /// <param name="definition">The meaning of the word.</param>
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
