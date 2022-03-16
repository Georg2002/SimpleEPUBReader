using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EPUBRenderer3
{
    public static class CharInfo
    {
        public readonly static Typeface StandardTypeface = new Typeface(new FontFamily("Hiragino Sans GB W3"), FontStyles.Normal,
   FontWeights.Normal, new FontStretch(), new FontFamily("Global User Interface"));

        public static char[] PossibleLineBreaksAfter = ", .」』、?？！!を。─）〉):\n\r　\t】≫》〟".ToCharArray();
        public static char[] PossibleLineBreaksBefore = "（「『〈【≪《(〔〝".ToCharArray();

        private static readonly SpecialCharacter Wiggle = new SpecialCharacter(new Vector(0, -1.1), 2.1f, '≀');//-1
        private static readonly SpecialCharacter Questionmark = new SpecialCharacter(new Vector(0.21, 0), 1, '？');

        public const float FontOffset = 0.24f;//0.24

        public static Dictionary<char, SpecialCharacter> SpecialCharacters = new Dictionary<char, SpecialCharacter>()
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
            {'~',Wiggle},{'∼',Wiggle},{'～',Wiggle},{'\u0027',new SpecialCharacter(new Vector(),1,'︑')},
            {'゠',new SpecialCharacter(new Vector(),1,'║')},{'＝',new SpecialCharacter(new Vector(),1,'║')}
            ,{'〟',new SpecialCharacter(new Vector(0,-0.8),1.3f,'〟')},{'〝',new SpecialCharacter(new Vector(0,0.5),1.3f,'〝') },
            {'．',new SpecialCharacter(new Vector(1,-1.9),2f,'．') },{'-',new SpecialCharacter(new Vector(),1,'│') }
        };

    }

    public struct SpecialCharacter
    {
        public Vector Offset;
        public float Scaling;
        public char Replacement;
        public SpecialCharacter(Vector Offset, float Scaling, char Replacement)
        {
            this.Offset = Offset;
            this.Scaling = Scaling;
            this.Replacement = Replacement;
        }
    }
}
