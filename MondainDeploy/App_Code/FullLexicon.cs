using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MondainDeploy
{
    // todo: Come up with a better class name than "FullLexicon."
    public class FullLexicon
    {


        public Dictionary<string, WordData> WordsToMetadata { get; set; }
        public Dictionary<string, List<string>> AlphagramsToWords { get; set; }

        public Dictionary<int, int> MaxProbPerWord { get; set; }

        ///<summary>
        ///Create new lexicon from a text file via InitWordsToMetadata.
        ///</summary>
        ///<remarks>
        ///The path constructor expects FULL path.
        ///</remarks>


        // todo: Make this a derived class so that it can inherit from the second FullLexicon.
        // Among other things this will allow swerving of the two lines of duplicated code.
        public FullLexicon(string inputPath)
        {
            WordsToMetadata = InitWordsToMetadata(new Dictionary<string, WordData>(), inputPath);
            AlphagramsToWords = InitAlphagramsToWords(WordsToMetadata);
            MaxProbPerWord = InitMaxProbPerWord();
        }


        public FullLexicon(LexTableWrapper lexWrapper)
        {
            WordsToMetadata = lexWrapper.WordsToMetadata;
            AlphagramsToWords = InitAlphagramsToWords(WordsToMetadata);
            MaxProbPerWord = InitMaxProbPerWord();
        }

        private Dictionary<string, List<string>> InitAlphagramsToWords(Dictionary<string, WordData> wordsToMeta)
        {
            var alphaToWords = new Dictionary<string, List<string>>();
            foreach (var wordsToMetaItem in wordsToMeta)
            {
                // if there's no word matching the current alphagram, start a new alphagram/solution set
                if (!alphaToWords.ContainsKey(wordsToMetaItem.Value.Alphagram))
                    alphaToWords.Add(wordsToMetaItem.Value.Alphagram, new List<string>());
                // regardless, add the current word in
                alphaToWords[wordsToMetaItem.Value.Alphagram].Add(wordsToMetaItem.Key);
            }
            return alphaToWords;
        }

        private Dictionary<int, int> InitMaxProbPerWord()
        {
            // Dictionary with keys 2-15...
            return Enumerable.Range(2, 14).ToList().ToDictionary(x => x, 
            // and values representing how many alphagrams there are at each length in the dictionary.
                x => new List<string>(from kvp in AlphagramsToWords.ToList() where kvp.Key.Length == x select kvp.Key).Count);
            // todo: Could bake this into the DB, as it's constant values per lexicon.
        }

        private Dictionary<string, WordData> InitWordsToMetadata(Dictionary<string, WordData> wordsToMeta, string path)
        {
            try
            {
                using (var sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var tempWordData = new WordData();
                        string[] splitLine = line.Split('\t');
                        var word = splitLine[0];
                        if (word.Contains("+"))
                        {
                            word = word.TrimEnd('+');
                            // IsNew is a token not being used right now. It's indicated by that + being trimmed out.
                            // todo: Add option to show the IsNew token
                            tempWordData.IsNew = true;
                        }
                        else
                        {
                            tempWordData.IsNew = false;
                        }
                        // Writing splitLine[0] instead of word here makes the file format transparent.
                        tempWordData.Alphagram = AlphagramifyString(splitLine[0]);
                        tempWordData.Probability = MondainUI.TryParseWithDefault(splitLine[1], 1);
                        tempWordData.Playability = MondainUI.TryParseWithDefault(splitLine[2], 1);
                        tempWordData.Definition = splitLine[3];
                        wordsToMeta.Add(word, tempWordData);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return wordsToMeta;

        } // end Lexicon.FillWordsToAlphagrams

        public int GetWordCount()
        {
            return WordsToMetadata.Count;
        }

        public int GetAlphagramCount()
        {
            return AlphagramsToWords.Count;
        }


        ///Current behavior can return a quiz length that's smaller than requested under unusual circumstances.
        ///(Example: 2 questions, length 8, min prob 1, max prob 4.)
        ///Because of the way that probability is stored in the database, the customQuizLength validator can't really handle all these edge cases.
        ///This is currently handled by the Default code-behind page which just produces the smaller quiz.
        ///Todo: fix this.


        ///Also todo: a less complicated chain for determining the maxProb.
        ///Currently what happens is that in the code-behind file, if the user enters a meaningless maxProb (or minProb), it defaults to Constants.ProbabilityMaxDefault.
        ///Then that default probability (which is a number chosen to be higher than the actual meaningful limit of maxProb) gets sent to this function.
        ///This is all a bit inelegant.
        ///Because of the way we get lists, it doesn't really matter if the maxProb is out of bounds to the right -- we're only ever comparing less than or equal to.
        ///The only time we get errors is if the *minProb* is out of bounds to the right.
        public List<KeyValuePair<string, List<string>>> GetRandomQuizEntries(int returnsize, Random rnd, int minLength, int maxLength, int minProb, int maxProb)
        {
            // A minimum probability under 1 means the same thing as a minimum probability of 1.

            List<KeyValuePair<string, List<string>>> shuffledList;

            if (minProb < 1)
                minProb = 1;

            if (maxProb < 1)
            {
                // The list with only max prob. specified meaningfully.
                shuffledList =
                    new List<KeyValuePair<string, List<string>>>(AlphagramsToWords.ToList()
                        .Where(kvp => kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength
                                      && LookupPairsForWord(kvp.Value[0]).Value.Probability >= minProb)
                        .OrderBy(kvp => rnd.Next()));
            }
            else
            {
                // The list with both min prob. and max prob. specified meaningfully.
                shuffledList =
                    new List<KeyValuePair<string, List<string>>>(AlphagramsToWords.ToList()
                        .Where(kvp => kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength
                                      && LookupPairsForWord(kvp.Value[0]).Value.Probability >= minProb
                                      && LookupPairsForWord(kvp.Value[0]).Value.Probability <= maxProb)
                        .OrderBy(kvp => rnd.Next()));
               
            }
           return shuffledList.Take(returnsize).ToList();
        }

        public List<KeyValuePair<string, List<string>>> GetBlankBingoEntries(int returnsize, Random rnd, int minLength, int maxLength)
        {
            var bbCandidates = (from kvp in AlphagramsToWords
                                    where kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength
                                    select RemoveRandomLetter(kvp.Key, new Random()) 
                                into newKeySearchable select new KeyValuePair<string, List<string>>(newKeySearchable, ReturnOneLetterSteals(newKeySearchable))).ToList();

            var bbList = new List<KeyValuePair<string, List<string>>>(bbCandidates.ToList()
                .Where(kvp => Enumerable.Range(1, 9)
                    .Select(x => x)
                    .ToList()
                    .Contains(kvp.Value.Count)).OrderBy(kvp => rnd.Next()));

            return bbList.Take(returnsize).ToList();
        }

        public string RemoveRandomLetter(string s, Random rnd)
        {
            return s.Remove(rnd.Next(0, s.Length - 1), 1);
        }

        public List<string> ReturnOneLetterSteals(string input)
        {
            var stealList = new List<string>();
            input = AlphagramifyString(input);
            foreach (var letter in Enumerable.Range('A', 26).Select(x => (char)x).ToList())
            {
                var tempString = AlphagramifyString(input + letter);
                if (AlphagramsToWords.ContainsKey(tempString))
                    stealList.AddRange(AlphagramsToWords[tempString]);
            }
            return stealList;
        }



        public static string AlphagramifyString(string w)
        {
            return string.Concat(w.ToCharArray().OrderBy(x => x));
        }   

        public static List<string> GetValidAnagrams(string w, Dictionary<string, List<string>> alphaToWords)
        {
            var returnArray = new List<string>();
            if (alphaToWords.ContainsKey(AlphagramifyString(w)))
            {
                returnArray = alphaToWords[w];
            }
            return returnArray;
        }

        public KeyValuePair<string, WordData> LookupPairsForWord(string wordToSearch)
        {
            return new KeyValuePair<string, WordData>(wordToSearch, WordsToMetadata[wordToSearch]);
        }
        
        // todo: Readd the function call I removed to create DB from text file as a new function in a separate class calling AddCurrentDirToPath
        // todo: Move AddCurrentDirToPath to the separate class created by the readdition of its invocation to create DB from text file
        public static string AddCurrentDirToPath(string path)
        {
            var currentDir = Directory.GetCurrentDirectory();
            if (currentDir.ToLower().EndsWith(@"\bin\debug") ||
                currentDir.ToLower().EndsWith(@"\bin\release"))

                path = Path.GetFullPath(@"..\..\" + path);
            else
                path = Path.GetFullPath(path);
            return path;
        }



    }
}
