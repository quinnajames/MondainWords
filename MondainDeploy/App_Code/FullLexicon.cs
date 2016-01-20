using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MondainDeploy
{
    public class FullLexicon
    {
        public Dictionary<string, WordData> WordsToMetadata { get; set; }
        public Dictionary<string, List<string>> AlphagramsToWords { get; set; }

        public Dictionary<int, int> MaxProbPerWord { get; set; }

        ///<summary>
        ///Constructor expects FULL path.
        ///</summary>
        public FullLexicon(string inputPath)
        {
            WordsToMetadata = InitWordsToMetadata(new Dictionary<string, WordData>(), inputPath);
            AlphagramsToWords = InitAlphagramsToWords(WordsToMetadata);
            MaxProbPerWord = InitMaxProbPerWord();
        }
        // Possible future change: a wrapped FullLexicon could inherit from the standard FullLexicon
        public FullLexicon(LexTableWrapper ltw)
        {
            WordsToMetadata = ltw.WordsToMetadata;
            AlphagramsToWords = InitAlphagramsToWords(WordsToMetadata);
            MaxProbPerWord = InitMaxProbPerWord();
        }

        private Dictionary<string, List<string>> InitAlphagramsToWords(Dictionary<string, WordData> wordsToMeta)
        {
            Dictionary<string, List<string>> ATW = new Dictionary<string, List<string>>();
            foreach (var wtmItem in wordsToMeta)
            {
                // if there's no word matching the current alphagram, start a new alphagram/solution set
                if (!ATW.ContainsKey(wtmItem.Value.Alphagram))
                    ATW.Add(wtmItem.Value.Alphagram, new List<string>());
                // regardless, add the current word in
                ATW[wtmItem.Value.Alphagram].Add(wtmItem.Key);
            }
            return ATW;
        }

        private Dictionary<int, int> InitMaxProbPerWord()
        {
            Dictionary<int, int> maxProb = new Dictionary<int, int>();
            for (var x = 2; x < 15 - 1; x++)
            {
                maxProb.Add(x, new List<string>(from kvp in AlphagramsToWords.ToList()
                    where kvp.Key.Length == x
                    select kvp.Key).Count);
            }
            return maxProb;
        }

        private Dictionary<string, WordData> InitWordsToMetadata(Dictionary<string, WordData> wordsToMeta, string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
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
                            tempWordData.IsNew = true;
                        }
                        else
                        {
                            tempWordData.IsNew = false;
                        }
                        tempWordData.Alphagram = AlphagramifyString(word);
                        tempWordData.Probability = int.Parse(splitLine[1]);
                        tempWordData.Playability = int.Parse(splitLine[2]);
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

        ///<summary>
        ///Current behavior can return a quiz length that's smaller than requested under unusual circumstances.
        ///(Example: 2 questions, length 8, min prob 1, max prob 4.)
        ///Because of the way that probability is stored in the database, the customQuizLength validator can't really handle all these edge cases.
        ///This is currently handled by the Default code-behind page which just produces the smaller quiz.
        ///For future enhancement.
        ///</summary>
        public List<KeyValuePair<string, List<string>>> GetRandomQuizEntries(Int32 returnsize, Random rnd, int minLength, int maxLength, int minProb, int maxProb)
        {
            if (minProb <= 0)
                minProb = 1;

            if (maxProb <= 0)
            {
                var shuffledList =
                    new List<KeyValuePair<string, List<string>>>(from kvp in AlphagramsToWords.ToList()
                        where kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength
                              && LookupPairsForWord(kvp.Value[0]).Value.Probability >= minProb
                        orderby rnd.Next()
                        select kvp);
                return shuffledList.Take(returnsize).ToList();
            }
            else
            {
                var shuffledList =
                    new List<KeyValuePair<string, List<string>>>(from kvp in AlphagramsToWords.ToList()
                        where kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength
                              && LookupPairsForWord(kvp.Value[0]).Value.Probability >= minProb
                              && LookupPairsForWord(kvp.Value[0]).Value.Probability <= maxProb
                        orderby rnd.Next()
                        select kvp);
                return shuffledList.Take(returnsize).ToList();
            }
        } // end GetRandomQuizEntries

        public List<KeyValuePair<string, List<string>>> GetBlankBingoEntries(Int32 returnsize, Random rnd, int minLength, int maxLength)
        {
            var bbCandidates = new List<KeyValuePair<string, List<string>>>();
            foreach (var kvp in AlphagramsToWords)
            {
                if (kvp.Key.Length <= maxLength && kvp.Key.Length >= minLength)
                {
                    var newKeySearchable = RemoveRandomLetter(kvp.Key, new Random());
                    bbCandidates.Add(new KeyValuePair<string, List<string>>(newKeySearchable,
                        ReturnOneLetterSteals(newKeySearchable)));
                }
            }

            var bbList = new List<KeyValuePair<string, List<string>>>(from kvp in bbCandidates.ToList()
                where Enumerable.Range(1, 9)
                    .Select(x => (int)x)
                    .ToList()
                    .Contains(kvp.Value.Count)
                orderby rnd.Next()
                select kvp);

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

        public static List<string> GetValidAnagrams(string w, Dictionary<string, List<string>> ATW)
        {
            var returnArray = new List<string>();
            if (ATW.ContainsKey(AlphagramifyString(w)))
            {
                returnArray = ATW[w];
            }
            return returnArray;

        } // end GetValidAnagrams
        public KeyValuePair<string, WordData> LookupPairsForWord(string wordToSearch)
        {
            return new KeyValuePair<string, WordData>(wordToSearch, WordsToMetadata[wordToSearch]);
        }
        
        // This should be moved out of FullLexicon.
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
