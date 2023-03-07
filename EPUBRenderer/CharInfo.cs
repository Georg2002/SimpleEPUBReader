using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer
{
    public static class CharInfo
    {
        public readonly static FontFamily StandardFallbackFontFamily = new FontFamily("Global User Interface");
        //font as embedded resource for assembly stuff, normal resource for font family
        public readonly static FontFamily StandardFontFamily = new FontFamily("Arial"); //Fonts.GetFontFamilies(new Uri("pack://application:,,,/EPUBRenderer;component/"), "./Fonts/").FirstOrDefault(a => a.ToString().Contains("Hiragino"));
        public readonly static Typeface StandardTypeface = new Typeface(StandardFontFamily, FontStyles.Normal,
   FontWeights.Normal, new FontStretch(), StandardFallbackFontFamily);

        public static char[] PossibleLineBreaksAfter = ", .」』、?？！!を。─）〉):\n\r　\t】≫》〟…".ToCharArray();
        public static char[] PossibleLineBreaksBefore = "（「『〈【≪《(〔〝".ToCharArray();
        public static char[] TrimCharacters = PossibleLineBreaksAfter.Concat(PossibleLineBreaksBefore).ToArray();
        //0.02, -0.26
        private static readonly SpecialCharacter Wiggle = new SpecialCharacter(new Vector(-0, 0.02), 1.34f, '〜', rotation: 91.5);
        private static readonly SpecialCharacter Questionmark = new SpecialCharacter(new Vector(0.21, 0), 1, '？');
        private static readonly SpecialCharacter VertLine = new SpecialCharacter(new Vector(0, -0.1), 1, '│');

        public const float FontOffset = 0.24f;//0.24

        public static Dictionary<char, SpecialCharacter> SpecialCharacters { get; set; } = new Dictionary<char, SpecialCharacter>()
        {
            {'」',new SpecialCharacter(new Vector(),1,'﹂')},{'「',new SpecialCharacter(new Vector(),1,'﹁')},
            {'（',new SpecialCharacter(new Vector(),1,'︵')},{'）',new SpecialCharacter(new Vector(),1,'︶')},
            {'『',new SpecialCharacter(new Vector(),1,'﹃')},{'』',new SpecialCharacter(new Vector(),1,'﹄')},
            {'。',new SpecialCharacter(new Vector(),1,'︒')},{'、',new SpecialCharacter(new Vector(),1,'︑')},
            {'?',Questionmark},{'？',Questionmark}
            ,{'!',new SpecialCharacter(new Vector(0,0),1,'!')},{'！',new SpecialCharacter(new Vector(0,0),1,'!')},
            { 'ー',VertLine},{ '─',VertLine}, {'―',VertLine},{'-',VertLine },{'－',VertLine },
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
            {'．',new SpecialCharacter(new Vector(1,-1.9),2f,'．') }
        };

        public static long FontDataLength;
        public static IntPtr FontDataPtr;     
    }

    public struct SpecialCharacter
    {
        public Vector Offset;
        public float Scaling;
        public char Replacement;
        public double Rotation;
        public SpecialCharacter(Vector Offset, float Scaling, char Replacement, double rotation = 0)
        {
            this.Offset = Offset;
            this.Scaling = Scaling;
            this.Replacement = Replacement;
            this.Rotation = rotation;
        }
    }
}
