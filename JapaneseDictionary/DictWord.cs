using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JapaneseDictionary
{
    public enum VocabType : byte
    {
        Word, Kanji, Name
    }

    public class DictWord
    {
        private string[] data;//block written forms, block readings, block meanings
        private byte readingsStartIndex;
        private byte meaningsStartIndex;
        public Span<string> WrittenForms => data.AsSpan(0, readingsStartIndex);
        public string WrittenForm { get => string.Join("; ", WrittenForms); }
        public Span<string> Readings => data.AsSpan(readingsStartIndex, meaningsStartIndex - readingsStartIndex);
        public string Reading { get => string.Join("; ", Readings); }
        public Span<string> Meanings => data.AsSpan(meaningsStartIndex);
        public string Meaning { get => string.Join("; ", Meanings); }
        public VocabType Type;

        private string spanFirst(Span<string> span) => span.Length == 0 ? "" : span[0];
        public override string ToString() => spanFirst(WrittenForms) + " " + spanFirst(Readings);
        public DictWord() { }

        public DictWord(StreamReader Reader, List<string> tempList)
        {
            Type = (VocabType)Convert.ToInt32(Reader.ReadLine());
            GetWordArray(Reader, tempList);
            this.readingsStartIndex = (byte)tempList.Count;//limited to 255 entries total
            GetWordArray(Reader, tempList);
            this.meaningsStartIndex = (byte)tempList.Count;
            GetWordArray(Reader, tempList);
            tempList.TrimExcess();
            this.data = tempList.ToArray();
            tempList.Clear();
        }

        private static void GetWordArray(StreamReader Reader, List<string> TempList)
        {
            while (true)
            {
                string Line = Reader.ReadLine();
                if (string.IsNullOrWhiteSpace(Line)) break;
                TempList.Add(Line);
            }
        }

        public bool MatchesSearchword(string searchword)
        {
            for (int i = 0; i<meaningsStartIndex; i++)
            {
                if (data[i] == searchword) return true;
            }
            return false;
        }
    }
}
