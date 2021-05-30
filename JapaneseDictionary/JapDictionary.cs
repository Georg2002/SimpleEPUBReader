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
        JapaneseDictionary JDict = new JapaneseDictionary();
        IJapaneseEntry[] JEntries;
        KanjiDictionary KDict = new KanjiDictionary();
        IKanjiEntry[] KEntries;
        NameDictionary NDict = new NameDictionary();
        INameEntry[] NEntries;
        Task<object[]>[] DictTasks;

        bool LookupActive;
        bool Abort;

        public JapDictionary()
        {
            GetAllEntries();
        }

        private async void GetAllEntries()
        {
            DictTasks = new Task<object[]>[3];
            DictTasks[0] = GetEntries(() => JDict.GetEntries().ToArray());
            DictTasks[1] = GetEntries(() => KDict.GetEntries().ToArray());
            DictTasks[2] = GetEntries(() => NDict.GetEntries().ToArray());
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
            if (text == "") return new List<DictWord>();         
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
            if (JEntries == null)
            {
                JEntries = (IJapaneseEntry[])await DictTasks[0];                
                KEntries = (IKanjiEntry[])await DictTasks[1];
                NEntries = (INameEntry[])await DictTasks[2];
                foreach (var DictTask in DictTasks)
                {
                    DictTask.Dispose();
                }
            }

            text = text.Trim();
            string[] Searchwords = GetSearchwords(text);
            string Hiragana = LanguageResources.GetHiragana(text);
            Console.WriteLine(Searchwords.Length.ToString());
            List<DictWord> Results = new List<DictWord>();
            var J = JLookup(Searchwords);
            var K = KLookup(text);
            var N = NLookup(Hiragana);
            Results.AddRange(await J);
            Results.AddRange(await K);
            Results.AddRange(await N);
            LookupActive = false;
            return Results;
        }
        private async Task<IEnumerable<DictWord>> NLookup(string text)
        {
            List<DictWord> Results = new List<DictWord>();
            await Task.Run(() =>
            {
                foreach (var Entry in NEntries)
                {
                    if (Abort) break;
                    if (Entry.Kanjis.Any(a => a.Text == text) || Entry.Readings.Any(a => a.Text == text))
                    {
                        DictWord NewRes = new DictWord();
                        NewRes.Type = VocabType.Name;
                        foreach (var IKanji in Entry.Kanjis)
                        {
                            NewRes.WrittenForm = Accumulate(NewRes.WrittenForm, IKanji.Text);
                        }
                        foreach (var Reading in Entry.Readings)
                        {
                            NewRes.Readings = Accumulate(NewRes.Readings, Reading.Text);
                        }
                        Results.Add(NewRes);
                    }
                }
            });
            return Results;
        }

        private async Task<IEnumerable<DictWord>> KLookup(string text)
        {
            List<DictWord> Results = new List<DictWord>();
            if (text.Length == 1)
            {
                await Task.Run(() =>
                {
                    foreach (var Entry in KEntries)
                    {
                        if (Abort) break;
                        if (Entry.Literal == text)
                        {
                            DictWord NewRes = new DictWord();
                            NewRes.Type = VocabType.Kanji;
                            NewRes.WrittenForm = text;

                            foreach (var meaning in Entry.Meanings)
                            {
                                if (meaning.Language == Wacton.Desu.Enums.Language.English)
                                {
                                    NewRes.Meanings = Accumulate(NewRes.Meanings, meaning.Term);
                                }
                            }
                            foreach (var Reading in Entry.Readings)
                            {
                                //code ja_on value 3, ja_kun value 4
                                if (Reading.Type.Value == 3 || Reading.Type.Value == 4)
                                {
                                    NewRes.Readings = Accumulate(NewRes.Readings, Reading.Value);
                                }
                            }
                            Results.Add(NewRes);
                        }
                    }
                });
            }
            return Results;
        }

        private async Task<IEnumerable<DictWord>> JLookup(string[] searchwords)
        {
            List<DictWord> Results = new List<DictWord>();
            await Task.Run(() =>
            {
                foreach (var Entry in JEntries)
                {
                    if (Abort) break;

                    if (Entry.Kanjis.Any(a => searchwords.Contains(a.Text)) || Entry.Readings.Any(a => searchwords.Contains(a.Text)))
                    {
                        DictWord NewRes = new DictWord();
                        NewRes.Type = VocabType.Word;
                        foreach (var IKanji in Entry.Kanjis)
                        {
                            NewRes.WrittenForm = Accumulate(NewRes.WrittenForm, IKanji.Text);
                        }
                        foreach (var Sense in Entry.Senses)
                        {
                            if (Sense.Glosses.First().Language == Wacton.Desu.Enums.Language.English)
                            {
                                foreach (var Gloss in Sense.Glosses)
                                {
                                    NewRes.Meanings = Accumulate(NewRes.Meanings, Gloss.Term);
                                }
                            }
                        }
                        foreach (var Reading in Entry.Readings)
                        {
                            NewRes.Readings = Accumulate(NewRes.Readings, Reading.Text);
                        }
                        Results.Add(NewRes);
                    }
                }
            });
            return Results;
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

        private string Accumulate(string Sum, string New)
        {
            if (string.IsNullOrEmpty(Sum))
            {
                return New;
            }
            else
            {
                return Sum + "; " + New;
            }
        }
    }

    public enum VocabType
    {
        Word, Kanji, Name
    }

    public struct DictWord
    {
        public string WrittenForm;
        public string Readings;
        public string Meanings;
        public VocabType Type;
        public override string ToString()
        {
            return WrittenForm.Split(';').FirstOrDefault().TrimEnd() + " " + Readings.Split(';').FirstOrDefault().TrimEnd();
        }
    }
}
