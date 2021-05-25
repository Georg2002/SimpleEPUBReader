using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBReader2
{
    public static class LanguageResources
    {
        public static Dictionary<char, char> HiraganaDict = new Dictionary<char, char>()
        {
            { 'ア','あ' },{ 'ァ','ぁ' },{ 'カ','か' },{ 'サ','さ' },{ 'タ','た' },
            { 'ナ','な' },{ 'ハ','は' },{ 'マ','ま' },{ 'ヤ','や' },{ 'ャ','ゃ' },
            { 'ラ','ら' },{ 'ワ','わ' },{ 'ガ','が' },{ 'ザ','ざ' },{ 'ダ','だ' },
            { 'バ','ば' },{ 'パ','ぱ' },{ 'イ','い' },{ 'ィ','ぃ' },{ 'キ','き' },
            { 'シ','し' },{ 'チ','ち' },{ 'ニ','に' },{ 'ヒ','ひ' },{ 'ミ','み' },
            { 'リ','り' },{ 'ヰ','ゐ' },{ 'ギ','ぎ' },{ 'ジ','じ' },{ 'ヂ','ぢ' },
            { 'ビ','び' },{ 'ピ','ぴ' },{ 'ウ','う' },{ 'ゥ','ぅ' },{ 'ク','く' },
            { 'ス','す' },{ 'ツ','つ' },{ 'ヌ','ぬ' },{ 'フ','ふ' },{ 'ム','む' },
            { 'ユ','ゆ' },{ 'ュ','ゅ' },{ 'ル','る' },{ 'グ','ぐ' },{ 'ズ','ず' },
            { 'ヅ','づ' },{ 'ブ','ぶ' },{ 'プ','ぷ' },{ 'エ','え' },{ 'ェ','ぇ' },
            { 'ケ','け' },{ 'セ','せ' },{ 'テ','て' },{ 'ネ','ね' },{ 'ヘ','へ' },
            { 'メ','め' },{ 'レ','れ' },{ 'ヱ','ゑ' },{ 'ゲ','げ' },{ 'ゼ','ぜ' },
            { 'デ','で' },{ 'ベ','べ' },{ 'ペ','ぺ' },{ 'オ','お' },{ 'ォ','ぉ' },
            { 'コ','こ' },{ 'ソ','そ' },{ 'ト','と' },{ 'ノ','の' },{ 'ホ','ほ' },
            { 'モ','も' },{ 'ヨ','よ' },{ 'ョ','ょ' },{ 'ロ','ろ' },{ 'ヲ','を' },
            { 'ゴ','ご' },{ 'ゾ','ぞ' },{ 'ド','ど' },{ 'ボ','ぼ' },{ 'ポ','ぽ' },
            { 'ヴ','ゔ' },{ 'ッ','っ' },{ 'ン','ん' },{ 'ヽ','ゝ' },{ 'ヾ','ゞ' }
        };

        public static string GetHiragana(string text)
        {
            string Hira = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (LanguageResources.HiraganaDict.ContainsKey(c))
                {
                    Hira += LanguageResources.HiraganaDict[c];
                }
                else
                {
                    Hira += c;
                }
            }
            return Hira;
        }

        internal static string GetKatakana(string text)
        {
            string Kata = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (HiraganaDict.ContainsValue(c))
                {
                    Kata += HiraganaDict.First(a=>a.Value==c).Key;
                }
                else
                {
                    Kata += c;
                }
            }
            return Kata;
        }

        private struct Inflection
        {
            public string BaseForm;
            public string InflectedForm;
            public Inflection(string Inflection, string Base)
            {
                InflectedForm = Inflection;
                BaseForm = Base;
            }
        }

        //eg adjectives, will not be applied recursively
        private static Inflection[] NormalInflections = new Inflection[]
        {
            new Inflection("き","い")
        };

        //eg verbs, will be applied recursively
        private static Inflection[] OverwriteInflections = new Inflection[]
       {
           //adjective
           new Inflection("かった","い"),new Inflection("くない","い"),
           new Inflection("く","い"),new Inflection("くある","い"),
           //random, sort later
             new Inflection("された","される"),
           new Inflection("される","す"), new Inflection("される","する"),

           //present-negative
           new Inflection("ない","る"), new Inflection("さない","す"),
           new Inflection("しない","する"), new Inflection("かない","く"),
           new Inflection("こない","くる"), new Inflection("がない","ぐ"),
           new Inflection("ばない","ぶ"), new Inflection("たない","つ"),
           new Inflection("まない","む"), new Inflection("わない","う"),
           new Inflection("らない","る"), new Inflection("なない","ぬ"),

           //past
           new Inflection("した","す"), new Inflection("いた","く"),
           new Inflection("きた","くる"), new Inflection("った","く"),
           new Inflection("いだ","ぐ"), new Inflection("んだ","む"),
           new Inflection("んだ","ぬ"), new Inflection("んだ","ぶ"),
           new Inflection("った","る"), new Inflection("った","つ"),
           new Inflection("った","う"), new Inflection("た","る"),

           //past-negative
            new Inflection("かった","い"),

            //keigo-masu-inflections
             new Inflection("ました","ます"), new Inflection("ません","ます"),
           new Inflection("ませんでした","ます"),new Inflection("ましょう","ます"),
           //keigo-masu-stem-backtransformations
           new Inflection("ます","る"), new Inflection("ぎます","ぐ"),
           new Inflection("します","す"), new Inflection("ちます","つ"),
           new Inflection("びます","ぶ"), new Inflection("にます","ぬ"),
           new Inflection("みます","む"), new Inflection("きます","く"),
           new Inflection("ります","る"),new Inflection("います","う"),

           //te-form
           new Inflection("て","た"), new Inflection("で","だ"), 
           new Inflection("くて","い"),new Inflection("いる",""),  

           //potential form
           new Inflection("られる","る"), new Inflection("える","う"),
           new Inflection("ける","く"), new Inflection("せる","す"),
            new Inflection("てる","つ"), new Inflection("ねる","ぬ"),
           new Inflection("べる","ぶ"), new Inflection("める","む"),
           new Inflection("できる","する"), new Inflection("こられる","くる"),

           //volitional form
           new Inflection("よう","る"), new Inflection("そう","す"),
           new Inflection("こう","く"), new Inflection("ごう","ぐ"),
            new Inflection("ぼう","ぶ"), new Inflection("とう","つ"),
           new Inflection("もう","む"), new Inflection("ろう","る"),
           new Inflection("のう","ぬ"), new Inflection("おう","う"),
           new Inflection("しよう","する"), new Inflection("こよう","くる"),

           //shimau-chau
           new Inflection("しまう","")
       };

        //思われます 思われる　思う

        internal static List<string> GetPossibleBaseForms(string text)
        {
            var Res = new List<string>();
            text = GetHiragana(text);
            Res.Add(text);

            foreach (var Inf in NormalInflections)
            {
                if (text.EndsWith(Inf.InflectedForm))
                {
                    string NewText = ReplaceEnd(text, Inf.InflectedForm, Inf.BaseForm);
                    Res.Add(NewText);
                }
            }
            GetBaseFormRecursive(Res, text, 0);
            for (int i = 0; i < Res.Count - 1; i++)
            {
                if (Res.Count(a => a==Res[i]) > 1)
                {
                    Res.RemoveAt(i);
                }
            }
            return Res;
        }

        private static void GetBaseFormRecursive(List<string> Res, string text, int Depth)
        {
            Depth++;
            if (Depth > 4)
            {
                return;
            }
            foreach (var Inf in OverwriteInflections)
            {
                if (text.EndsWith(Inf.InflectedForm) && text.Length > Inf.InflectedForm.Length)
                {
                    string NewText = ReplaceEnd(text, Inf.InflectedForm, Inf.BaseForm);
                    Res.Add(NewText);
                    GetBaseFormRecursive(Res, NewText, Depth);
                }
            }
        }

        private static string ReplaceEnd(string text, string Old, string New)
        {
            return text.Remove(text.Length - Old.Length) + New;
        }       
    }
}

