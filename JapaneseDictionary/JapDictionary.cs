using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wacton.Desu;
using Wacton.Desu.Japanese;
using Wacton.Desu.Kanji;
using Wacton.Desu.Names;

namespace WatconWrapper
{
    public class JapDictionary
    {

        Task DictTask;
        Dictionary<char, List<DictWord>> Dict;

        bool LookupActive;
        bool Abort;

        public JapDictionary()
        {
            GetAllEntries();
        }

        private void GetAllEntries()
        {
            DictTask = Task.Run(async () =>
            {
                JapaneseDictionary JDict = new JapaneseDictionary();
                IJapaneseEntry[] JEntries;
                KanjiDictionary KDict = new KanjiDictionary();
                IKanjiEntry[] KEntries;
                NameDictionary NDict = new NameDictionary();
                INameEntry[] NEntries;
                var JTask = GetEntries(() => JDict.GetEntries().ToArray());
                var KTask = GetEntries(() => KDict.GetEntries().ToArray());
                var NTask = GetEntries(() => NDict.GetEntries().ToArray());
                JEntries = (IJapaneseEntry[])await JTask;
                NEntries = (INameEntry[])await NTask;
                KEntries = (IKanjiEntry[])await KTask;
                JTask.Dispose();
                KTask.Dispose();
                NTask.Dispose();
                var TDict = new Dictionary<char, List<DictWord>>();
                IncludeDict(JEntries, TDict);
                JEntries = null;
                IncludeDict(NEntries, TDict);
                NEntries = null;
                IncludeDict(KEntries, TDict);
                KEntries = null;
                Dict = TDict;

                GC.Collect();
            });
        }

        private async Task<object[]> GetEntries(Func<object[]> func)
        {
            object[] Res = null;
            await Task.Run(() =>
            {
                Res = func.Invoke();
            });
            return Res;
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
                    i++;
                    if (i > 1000)
                    {
                        return new List<DictWord>();
                    }
                    await Task.Delay(5);
                }
                Abort = false;
            }

            LookupActive = true;
            if (Dict == null)
            {
                await DictTask;
            }

            text = text.Trim();
            string[] Searchwords = GetSearchwords(text);
            List<DictWord> Results = await DictLookup(Searchwords);
            Results = GetSortedResults(Results);
            LookupActive = false;
            return Results;
        }

        private List<DictWord> GetSortedResults(List<DictWord> results)
        {
            List<DictWord> SortedRes = new List<DictWord>();
            foreach (var Word in results)
            {
                if (!SortedRes.Contains(Word))
                {
                    SortedRes.Add(Word);
                }
            }
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
                     var NewResults = PartialDict.Where(a => a.Readings.Any(b => b == Searchword) || a.WrittenForms.Any(c => c == Searchword)).ToList();
                     Res.AddRange(NewResults);
                 }
             });
            return Res;
        }

        private void IncludeDict(object[] Entries, Dictionary<Char, List<DictWord>> Dict)
        {
            foreach (var Entry in Entries)
            {
                DictWord NewWord = new DictWord(Entry);

                string StartingLetters = "";
                foreach (var Word in NewWord.Readings)
                {
                    char StartingLetter = Word[0];
                    if (!StartingLetters.Contains(StartingLetter)) StartingLetters += StartingLetter;
                }
                foreach (var Word in NewWord.WrittenForms)
                {
                    char StartingLetter = Word[0];
                    if (!StartingLetters.Contains(StartingLetter)) StartingLetters += StartingLetter;
                }
                foreach (var C in StartingLetters)
                {
                    if (!Dict.ContainsKey(C))
                    {
                        Dict.Add(C, new List<DictWord>());
                    }
                    Dict[C].Add(NewWord);
                }
            }
        }

        private string[] GetSearchwords(string text)
        {
            var BaseForm = LanguageResources.GetPossibleBaseForms(text);

            string Katakana = LanguageResources.GetKatakana(text);
            List<string> res = new List<string>();
            res.Add(text);
            if (Katakana != text)
            {
                res.Add(Katakana);
            }

            foreach (var Form in BaseForm)
            {
                if (Form != null && Form != text)
                {
                    res.Add(Form);
                }
            }
            return res.ToArray();
        }
    }
}
