using System;
using System.Collections.Generic;
using System.Linq;
using Wacton;
using Wacton.Desu.Japanese;
using Wacton.Desu.Kanji;
using Wacton.Desu.Names;

namespace WatconWrapper
{
    public enum VocabType
    {
        Word, Kanji, Name
    }

    public class DictWord
    {
        public readonly string[] WrittenForms;
        public string WrittenForm { get => Accumulate(WrittenForms); }
        public readonly string[] Readings;
        public string Reading { get => Accumulate(Readings); }
        public readonly string[] Meanings;
        public string Meaning { get => Accumulate(Meanings); }
        public readonly VocabType Type;

        public override string ToString() => WrittenForms.FirstOrDefault() + " " + Readings.FirstOrDefault();

        private string Accumulate(string[] Words)
        {
            string Res = "";
            foreach (var Word in Words)
            {
                if (Res == "") Res = Word;
                else Res += "; " + Word;
            }
            return Res;
        }

        public DictWord(object Entry)
        {
            int i = 0;
            switch ("I" + Entry.GetType().ToString().Split('.').Last())
            {
                case nameof(IKanjiEntry):
                    IKanjiEntry KEntry = (IKanjiEntry)Entry;
                    Type = VocabType.Kanji;
                    WrittenForms = new string[] { KEntry.Literal };
                    var TReadings = new List<string>();
                    foreach (var Reading in KEntry.Readings)
                    {
                        //code ja_on value 3, ja_kun value 4
                        if (Reading.Type.Value == 3 || Reading.Type.Value == 4)
                        {
                            TReadings.Add( Reading.Value);
                        }
                        i++;
                    }
                    Readings = TReadings.ToArray();
                    i = 0;
                   var TMeanings = new List<string>();
                    foreach (var meaning in KEntry.Meanings)
                    {
                        if (meaning.Language == Wacton.Desu.Enums.Language.English)
                        {
                            TMeanings.Add( meaning.Term);
                        }
                        i++;
                    }
                    Meanings = TMeanings.ToArray();
                    break;
                case nameof(INameEntry):
                    INameEntry NEntry = (INameEntry)Entry;
                    Type = VocabType.Name;
                    WrittenForms = new string[NEntry.Kanjis.Count()];
                    foreach (var IKanji in NEntry.Kanjis)
                    {
                        WrittenForms[i] = IKanji.Text;
                        i++;
                    }
                    i = 0;
                    Readings = new string[NEntry.Readings.Count()];
                    foreach (var Reading in NEntry.Readings)
                    {
                        Readings[i] = Reading.Text;
                        i++;
                    }
                    Meanings = new string[0];
                    break;
                case nameof(IJapaneseEntry):
                    IJapaneseEntry JEntry = (IJapaneseEntry)Entry;
                    Type = VocabType.Word;
                    WrittenForms = new string[JEntry.Kanjis.Count()];
                    foreach (var IKanji in JEntry.Kanjis)
                    {
                        WrittenForms[i] = IKanji.Text;
                        i++;
                    }
                    TMeanings = new List<string>();
                    foreach (var Sense in JEntry.Senses)
                    {
                        if (Sense.Glosses.Any() && Sense.Glosses.First().Language == Wacton.Desu.Enums.Language.English)
                        {
                            foreach (var Gloss in Sense.Glosses)
                            {
                                TMeanings.Add(Gloss.Term);
                            }
                        }
                    }

                    Meanings = TMeanings.ToArray();
                    i = 0;
                    Readings = new string[JEntry.Readings.Count()];
                    foreach (var Reading in JEntry.Readings)
                    {
                        Readings[i] = Reading.Text;
                        i++;
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
