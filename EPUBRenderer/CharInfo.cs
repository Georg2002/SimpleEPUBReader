using ExCSS;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public static class CharInfo
    {
        static CharInfo()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name.Contains(nameof(EPUBRenderer)));
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resName in resourceNames)
            {
                if (resName.EndsWith(".g.resources", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Found resource pack: {resName}");
                    using Stream stream = assembly.GetManifestResourceStream(resName);
                    if (stream == null) continue;

                    using var reader = new ResourceReader(stream);
                    foreach (DictionaryEntry entry in reader)
                    {
                        var name = entry.Key as string;
                        if (name.Contains("noto"))
                        {
                            var entryStream = entry.Value as Stream;
                            SKFontManager.Default.CreateTypeface(SKData.Create(entryStream));
                            entryStream.Close();
                        }
                    }
                }
            }
        }

        public readonly static string StandardFontFamily = "Noto Sans JP";
        public readonly static string StandardFallbackFontFamily = "Arial";
        public readonly static SKFont StandardFont = new SKFont { Size = 15, Subpixel = true, Typeface = SKTypeface.FromFamilyName(StandardFontFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), Edging = SKFontEdging.SubpixelAntialias };

        public static readonly char[] PossibleLineBreaksAfter = ", .」』、?？！!を。─）〉):\n\r　\t】≫》〟…".ToCharArray();
        public static readonly char[] PossibleLineBreaksBefore = "（「『〈【≪《(〔〝".ToCharArray();
        public static readonly char[] TrimCharacters = PossibleLineBreaksAfter.Concat(PossibleLineBreaksBefore).ToArray();

        private static readonly SpecialCharacter Wiggle = new(new Vector(0.1, 0), 1.25f, '〜', rotation: 91.5f);//0.02, -0.26
        private static readonly SpecialCharacter Questionmark = new(new Vector(0.21, 0), 1, '？');

        public const float FontOffset = 0.24f;//0.24

        public static Dictionary<char, SpecialCharacter> SpecialCharacters { get; set; } = new Dictionary<char, SpecialCharacter>()
        {
            {'」',new SpecialCharacter(new Vector(),1,'﹂')},{'「',new SpecialCharacter(new Vector(),1,'﹁')},
            {'（',new SpecialCharacter(new Vector(),1,'︵')},{'）',new SpecialCharacter(new Vector(),1,'︶')},
            {'『',new SpecialCharacter(new Vector(),1,'﹃')},{'』',new SpecialCharacter(new Vector(),1,'﹄')},
            {'。',new SpecialCharacter(new Vector(),1,'︒')},{'、',new SpecialCharacter(new Vector(),1,'︑')},
            {'?',Questionmark},{'？',Questionmark}
            ,{'!',new SpecialCharacter(new Vector(0,0),1,'!')},
            {'！',new SpecialCharacter(new Vector(0,0),1,'!')},{ 'ー',new SpecialCharacter(new Vector(),1,'│')},
            { '─',new SpecialCharacter(new Vector(),1,'│')}, {'―',new SpecialCharacter(new Vector(),1,'│')},
            {'…',new SpecialCharacter(new Vector(),1,'︙')},
            {'〈',new SpecialCharacter(new Vector(),1,'︿')},{'〉',new SpecialCharacter(new Vector(),1,'﹀')},
            {'【',new SpecialCharacter(new Vector(),1,'︻')},{'】',new SpecialCharacter(new Vector(),1,'︼')},
            {'［',new SpecialCharacter(new Vector(),1,'﹇')},{'］',new SpecialCharacter(new Vector(),1,'﹈')},
            {'≪',new SpecialCharacter(new Vector(),1,'︽')},{'≫',new SpecialCharacter(new Vector(),1,'︾')},
            {'《',new SpecialCharacter(new Vector(),1,'︽')},{'》',new SpecialCharacter(new Vector(),1,'︾')},
            {'(',new SpecialCharacter(new Vector(),1,'︵')},{')',new SpecialCharacter(new Vector(),1,'︶')},
            {'→',new SpecialCharacter(new Vector(),1,'↓')},{'：',new SpecialCharacter(new Vector(),1,'‥')},
            {'=',new SpecialCharacter(new Vector(),1,'║')}, {'〔',new SpecialCharacter(new Vector(),1,'︹')},
            {'〕',new SpecialCharacter(new Vector(),1,'︺')}, {'_',new SpecialCharacter(new Vector(),1,'∣')},
            {'~',Wiggle},{'∼',Wiggle},{'～',Wiggle},{'〜',Wiggle},{'\u0027',new SpecialCharacter(new Vector(),1,'︑')},
            {'゠',new SpecialCharacter(new Vector(),1,'║')},{'＝',new SpecialCharacter(new Vector(),1,'║')}
            ,{'〟',new SpecialCharacter(new Vector(0,-0.8),1.3f,'〟')},{'〝',new SpecialCharacter(new Vector(0,0.5),1.3f,'〝') },
            {'．',new SpecialCharacter(new Vector(1,-1.9),2f,'．') },{'-',new SpecialCharacter(new Vector(),1,'│') }
           ,{'－',new SpecialCharacter(new Vector(),1,'│') }
        };

    }

    public struct SpecialCharacter
    {
        public Vector Offset;
        public float Scaling;
        public char Replacement;
        public float Rotation;
        public SpecialCharacter(Vector Offset, float Scaling, char Replacement, float rotation = 0)
        {
            this.Offset = Offset;
            this.Scaling = Scaling;
            this.Replacement = Replacement;
            this.Rotation = rotation;
        }
    }
}
