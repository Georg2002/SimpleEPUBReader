using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatconWrapper
{
    public static class LanguageResources
    {
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
           new Inflection("ければ","い"),

           //random, sort later
           new Inflection("てる","ている"),           

           //present-negative
           new Inflection("ない","る"), new Inflection("さない","す"),
           new Inflection("しない","する"), new Inflection("かない","く"),
           new Inflection("こない","くる"), new Inflection("がない","ぐ"),
           new Inflection("ばない","ぶ"), new Inflection("たない","つ"),
           new Inflection("まない","む"), new Inflection("わない","う"),
           new Inflection("らない","る"), new Inflection("なない","ぬ"),

           //past
           new Inflection("した","す"), new Inflection("た","る"),
           new Inflection("いた","く"), new Inflection("した","する"),
           new Inflection("きた","くる"), new Inflection("った","く"),
           new Inflection("いだ","ぐ"), new Inflection("んだ","む"),
           new Inflection("んだ","ぬ"), new Inflection("んだ","ぶ"),
           new Inflection("った","る"), new Inflection("った","つ"),
           new Inflection("った","う"),

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
           new Inflection("します","する"),

           //stem-backtransformations
           new Inflection("ぎ","ぐ"),new Inflection("い","う"),
           new Inflection("し","す"), new Inflection("ち","つ"),
           new Inflection("び","ぶ"), new Inflection("に","ぬ"),
           new Inflection("み","む"), new Inflection("き","く"),
           new Inflection("り","る"),new Inflection("し","する"),

           //te-form
           new Inflection("て","た"), new Inflection("で","だ"),
           new Inflection("くて","い"),new Inflection("いる",""),  

           //potential form
           new Inflection("られる","る"), new Inflection("える","う"),
           new Inflection("ける","く"), new Inflection("せる","す"),
           new Inflection("てる","つ"), new Inflection("ねる","ぬ"),
           new Inflection("べる","ぶ"), new Inflection("める","む"),
           new Inflection("できる","する"), new Inflection("こられる","くる"),

           //passive form
           new Inflection("される","す"), new Inflection("される","する"),
           new Inflection("かれる","く"), new Inflection("まれる","む"),
           new Inflection("がれる","ぐ"), new Inflection("ばれる","ぶ"),
           new Inflection("たれる","つ"), new Inflection("なれる","ぬ"),
           new Inflection("われる","う"),

           //volitional form
           new Inflection("よう","る"), new Inflection("そう","す"),
           new Inflection("こう","く"), new Inflection("ごう","ぐ"),
           new Inflection("ぼう","ぶ"), new Inflection("とう","つ"),
           new Inflection("もう","む"), new Inflection("ろう","る"),
           new Inflection("のう","ぬ"), new Inflection("おう","う"),
           new Inflection("しよう","する"), new Inflection("こよう","くる"),

           //shimau-chau
           new Inflection("しまう",""), new Inflection("ちゃう",""),

           //imperative
           new Inflection("ろ","る"), new Inflection("せ","す"),
           new Inflection("け","く"), new Inflection("げ","ぐ"),
           new Inflection("べ","ぶ"), new Inflection("て","つ"),
           new Inflection("め","む"), new Inflection("れ","る"),
           new Inflection("ね","ぬ"), new Inflection("え","う"),
           new Inflection("しろ","する"), new Inflection("こい","くる"),
           new Inflection("くれる","くれ"),

           //ba-form            
           new Inflection("けば","く"), new Inflection("げば","ぐ"),
           new Inflection("べば","ぶ"), new Inflection("てば","つ"),
           new Inflection("めば","む"), new Inflection("れば","る"),
           new Inflection("ねば","ぬ"), new Inflection("えば","う"),
           new Inflection("せば","す"),

           //causative         
           new Inflection("かせる","く"), new Inflection("がせる","ぐ"),
           new Inflection("ばせる","ぶ"), new Inflection("たせる","つ"),
           new Inflection("ませる","む"), new Inflection("らせる","る"),
           new Inflection("なせる","ぬ"), new Inflection("わせる","う"),
           new Inflection("させる","す"),new Inflection("させる","る"),
           new Inflection("させる","する"),new Inflection("こさせる","くる"),
       };

        public static readonly Dictionary<char, char> HiraganaDict = new Dictionary<char, char>()
        {
            { 'ア','あ' },{ 'ァ','ぁ' },{ 'カ','か' },{ 'サ','さ' },{ 'タ','た' },
            { 'ナ','な' },{ 'ハ','は' },{ 'マ','ま' },{ 'ヤ','や' },{ 'ャ','ゃ' },
            { 'ラ','ら' },{ 'ワ','わ' },{ 'ガ','が' },{ 'ザ','ざ' },{ 'ダ','だ' },
            { 'バ','ば' },{ 'パ','ぱ' },

            { 'イ','い' },{ 'ィ','ぃ' },{ 'キ','き' },
            { 'シ','し' },{ 'チ','ち' },{ 'ニ','に' },{ 'ヒ','ひ' },{ 'ミ','み' },
            { 'リ','り' },{ 'ヰ','ゐ' },{ 'ギ','ぎ' },{ 'ジ','じ' },{ 'ヂ','ぢ' },
            { 'ビ','び' },{ 'ピ','ぴ' },

            { 'ウ','う' },{ 'ゥ','ぅ' },{ 'ク','く' },
            { 'ス','す' },{ 'ツ','つ' },{ 'ヌ','ぬ' },{ 'フ','ふ' },{ 'ム','む' },
            { 'ユ','ゆ' },{ 'ュ','ゅ' },{ 'ル','る' },{ 'グ','ぐ' },{ 'ズ','ず' },
            { 'ヅ','づ' },{ 'ブ','ぶ' },{ 'プ','ぷ' },

            { 'エ','え' },{ 'ェ','ぇ' },
            { 'ケ','け' },{ 'セ','せ' },{ 'テ','て' },{ 'ネ','ね' },{ 'ヘ','へ' },
            { 'メ','め' },{ 'レ','れ' },{ 'ヱ','ゑ' },{ 'ゲ','げ' },{ 'ゼ','ぜ' },
            { 'デ','で' },{ 'ベ','べ' },{ 'ペ','ぺ' },

            { 'オ','お' },{ 'ォ','ぉ' },
            { 'コ','こ' },{ 'ソ','そ' },{ 'ト','と' },{ 'ノ','の' },{ 'ホ','ほ' },
            { 'モ','も' },{ 'ヨ','よ' },{ 'ョ','ょ' },{ 'ロ','ろ' },{ 'ヲ','を' },
            { 'ゴ','ご' },{ 'ゾ','ぞ' },{ 'ド','ど' },{ 'ボ','ぼ' },{ 'ポ','ぽ' },
            { 'ヴ','ゔ' },{ 'ッ','っ' },{ 'ン','ん' },{ 'ヽ','ゝ' },{ 'ヾ','ゞ' }
        };

        public static readonly Dictionary<char, char> KatakanaDict;

        //hiragana with the vowels e or i
        public static char[] RuVerbVowelsHiragana = new char[]
        {
             'い','ぃ','き','し','ち','に','ひ','み','り','ゐ','ぎ','じ','ぢ','び','ぴ',
             'え','ぇ','け','せ','て','ね','へ','め','れ','ゑ','げ','ぜ','で','べ','ぺ',
        };

        static LanguageResources()
        {
            KatakanaDict = new Dictionary<char, char>(HiraganaDict.Count);
            foreach (var Entry in HiraganaDict)
            {
                KatakanaDict.Add(Entry.Value, Entry.Key);
            }
        }

        public static string GetHiragana(string text)
        {
            return SwitchKana(text, HiraganaDict);
        }

        internal static string GetKatakana(string text)
        {
            return SwitchKana(text, KatakanaDict);
        }

        private static string SwitchKana(string text, Dictionary<char, char> Dict)
        {
            string Switched = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (Dict.ContainsKey(c))
                {
                    Switched += Dict[c];
                }
                else
                {
                    Switched += c;
                }
            }
            return Switched;
        }             

        internal static List<string> GetPossibleBaseForms(string text)
        {
            var Res = new List<string>();
            text = GetHiragana(text);
            Res.Add(text);

            //ru verbs with check for last verb, as it would otherwise be added every time
            if (RuVerbVowelsHiragana.Contains(text.Last()))
            {
                string RuVerb = text + "る";
                Res.Add(RuVerb);
                GetBaseFormRecursive(Res, RuVerb, 0);
            }

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
                if (Res.Count(a => a == Res[i]) > 1)
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
    }
}

