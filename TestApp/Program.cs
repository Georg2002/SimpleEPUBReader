using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wacton.Desu.Japanese;
using Wacton.Desu.Kanji;
using Wacton.Desu.Names;
using WatconWrapper;
using System.Linq;
using System.IO;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteDictsToFile();
        }

        private static void WriteDictsToFile()
        {
            var Dict = new List<DictWord>();

            var JDict = new JapaneseDictionary();
            var JEntries = JDict.GetEntries().ToArray();
            Console.WriteLine("JDict entries loaded");
            foreach (var Entry in JEntries)
            {
                Dict.Add(new DictWordExtension(Entry));
            }
            Console.WriteLine("JDict entries added to dict");
            var KDict = new KanjiDictionary();
            var KEntries = KDict.GetEntries().ToArray();
            Console.WriteLine("KDict entries loaded");
            foreach (var Entry in KEntries)
            {
                Dict.Add(new DictWordExtension(Entry));
            }
            Console.WriteLine("KDict entries added to dict");
            var NDict = new NameDictionary();
            var NEntries = NDict.GetEntries().ToArray();
            Console.WriteLine("NDict entries loaded");
            foreach (var Entry in NEntries)
            {
                Dict.Add(new DictWordExtension(Entry));
            }
            Console.WriteLine("NDict entries added to dict");
            Write(Dict);           
        }

        private static void Write(List<DictWord> Dict)
        {
            int i = 0;
            int Count = 0;
            using (var Writer = new StreamWriter(@"D:\Informatik\EPUBReader\JapaneseDictionary\Resources\Dict.txt"))
            {
                foreach (var Word in Dict)
                {
                    i++;
                    Count++;
                    if (i > 1000)
                    {
                        i = 0;
                        Console.WriteLine(Count.ToString() + " words written to file");
                    }
                    Writer.WriteLine(((int)Word.Type).ToString());
                    foreach (var item in Word.WrittenForms)
                    {
                        Writer.WriteLine(item);
                    }
                    Writer.WriteLine();
                    foreach (var item in Word.Readings)
                    {
                        Writer.WriteLine(item);
                    }
                    Writer.WriteLine();
                    foreach (var item in Word.Meanings)
                    {
                        Writer.WriteLine(item);
                    }
                    Writer.WriteLine();
                }
                Writer.Close();
            }
        }
    }
}
