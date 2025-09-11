using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JapaneseDictionary
{
    public class JapDictionary
    {

        Task DictTask;
        Dictionary<char, DictWord[]> Dict;

        bool LookupActive;
        bool Abort;

        public JapDictionary() => this.GetAllEntries();

        private void GetAllEntries()
        {
            DictTask = Task.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = Path.Combine(assembly.Location, "..", "Resources", "Dict.txt");
                var TDict = new Dictionary<char, List<DictWord>>();
                var tempList = new List<string>(20);
                var tempCharList = new List<char>(20);
                using (StreamReader reader = new StreamReader(resourcePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var NewWord = new DictWord(reader, tempList);
                        IncludeDict(NewWord, TDict, tempCharList);
                    }
                }
                Dict = new Dictionary<char, DictWord[]>();
                foreach (var entry in TDict)
                {
                    Dict.Add(entry.Key, entry.Value.ToArray());
                }
            });            
        }

        public async Task<List<DictWord>> Lookup(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<DictWord>();
            if (LookupActive)
            {
                Abort = true;
                int i = 0;
                while (LookupActive == true)
                {
                    if (++i > 1000) return new List<DictWord>();
                    await Task.Delay(5);
                }
                Abort = false;
            }

            LookupActive = true;
            if (Dict == null) await DictTask;

            text = text.Trim();
            string[] Searchwords = GetSearchwords(text);
            List<DictWord> Results = await this.DictLookup(Searchwords);
            Results = GetSortedResults(Results);
            LookupActive = false;
            return Results;
        }

        private static List<DictWord> GetSortedResults(List<DictWord> results)
        {
            List<DictWord> SortedRes = new List<DictWord>();
            foreach (var Word in results) if (!SortedRes.Contains(Word)) SortedRes.Add(Word);
            return SortedRes.OrderBy(a => (int)a.Type).ToList();
        }

        private async Task<List<DictWord>> DictLookup(string[] searchwords)
        {
            List<DictWord> Res = new List<DictWord>();
            await Task.Run(() =>
             {
                 if (searchwords.Length == 0) return;
                 foreach (var Searchword in searchwords)
                 {
                     if (Abort) break;
                     if (Searchword.Length == 0) continue;
                     char FirstLetter = Searchword[0];
                     if (!Dict.ContainsKey(FirstLetter)) continue;
                     var PartialDict = Dict[FirstLetter];
                     var NewResults = PartialDict.Where(a => a.Readings.Any(b => b == Searchword) || a.WrittenForms.Any(c => c == Searchword)).Take(10).ToList();
                     Res.AddRange(NewResults);
                 }
             });
            return Res;
        }

        private static void IncludeDict(DictWord NewWord, Dictionary<char, List<DictWord>> Dict, List<char> startingLetters)
        {
            foreach (var Word in NewWord.Readings)
            {
                char StartingLetter = Word[0];
                if (!startingLetters.Contains(StartingLetter)) startingLetters.Add(StartingLetter);
            }
            foreach (var Word in NewWord.WrittenForms)
            {
                char StartingLetter = Word[0];
                if (!startingLetters.Contains(StartingLetter)) startingLetters.Add(StartingLetter);
            }
            foreach (var C in startingLetters)
            {
                if (!Dict.ContainsKey(C)) Dict.Add(C, new List<DictWord>());
                Dict[C].Add(NewWord);
            }
            startingLetters.Clear();
        }

        private static string[] GetSearchwords(string text)
        {
            text = LanguageResources.Trim(text);
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
            var BaseForm = LanguageResources.GetPossibleBaseForms(text);

            string Katakana = LanguageResources.GetKatakana(text);
            List<string> res = new List<string>();
            res.Add(text);
            if (Katakana != text) res.Add(Katakana);

            foreach (var Form in BaseForm) if (Form != null && Form != text) res.Add(Form);
            return res.ToArray();
        }
    }
}
