using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatconWrapper;
using Wacton;
using Wacton.Desu.Japanese;
using Wacton.Desu.Kanji;
using Wacton.Desu.Names;

namespace TestApp
{
  public  class DictWordExtension : DictWord
    {     
        public DictWordExtension(object Entry)
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
                            TReadings.Add(Reading.Value);
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
                            TMeanings.Add(meaning.Term);
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
