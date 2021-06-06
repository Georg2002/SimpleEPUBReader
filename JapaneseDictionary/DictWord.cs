using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WatconWrapper
{
    public enum VocabType
    {
        Word, Kanji, Name
    }

    public class DictWord
    {
        public string[] WrittenForms;
        public string WrittenForm { get => Accumulate(WrittenForms); }
        public string[] Readings;
        public string Reading { get => Accumulate(Readings); }
        public string[] Meanings;
        public string Meaning { get => Accumulate(Meanings); }
        public VocabType Type;

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

        public DictWord() { }

        public DictWord(StreamReader Reader)
        {
            Type = (VocabType)Convert.ToInt32(Reader.ReadLine());
            var TempList = new List<string>();           
            WrittenForms = GetWordArray(Reader,TempList);
            Readings = GetWordArray(Reader,TempList);
            Meanings = GetWordArray(Reader,TempList);
            TempList.Clear();
        }

        private string[] GetWordArray(StreamReader Reader, List<string> TempList)
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
