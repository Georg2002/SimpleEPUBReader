using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JapaneseDictionary
{
    public enum VocabType
    {
        Word, Kanji, Name
    }

    public class DictWord
    {
        public string[] WrittenForms;
        public string WrittenForm { get => string.Join("; ", WrittenForms); }
        public string[] Readings;
        public string Reading { get => string.Join("; ", Readings); }
        public string[] Meanings;
        public string Meaning { get => string.Join("; ", Meanings); }
        public VocabType Type;

        public override string ToString() => WrittenForms.FirstOrDefault() + " " + Readings.FirstOrDefault();
        public DictWord() { }

        public DictWord(StreamReader Reader, List<string> tempList)
        {
            Type = (VocabType)Convert.ToInt32(Reader.ReadLine());
            WrittenForms = GetWordArray(Reader, tempList);
            this.Readings = GetWordArray(Reader, tempList);
            Meanings = GetWordArray(Reader, tempList);
            tempList.Clear();
        }

        private static string[] GetWordArray(StreamReader Reader, List<string> TempList)
        {            
            while (true)
            {
                string Line = Reader.ReadLine();
                if (string.IsNullOrWhiteSpace(Line)) break;
                TempList.Add(Line);
            }
            var Res = TempList.ToArray(); 
            TempList.Clear();
            return Res;
        }
    }
}
