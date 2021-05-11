using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EPUBRenderer3
{
    public static class CharInfo
    {
        public static char[] PossibleLineBreaksAfter = ", .」』、?？！!を。─）〉):\n\r　\t】≫》〟".ToCharArray();
        public static char[] PossibleLineBreaksBefore = "（「『〈【≪《(〔〝".ToCharArray();

        private static readonly SpecialCharacter Wiggle = new SpecialCharacter(new Vector(0, -1), 2.3f, '≀');
        private static readonly SpecialCharacter Questionmark = new SpecialCharacter(new Vector(0.21, 0), 1, '？');//new Vector(0.21, 0)

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
            { '─',new SpecialCharacter(new Vector(),1,'│')},{'…',new SpecialCharacter(new Vector(),1,'︙')},
            {'〈',new SpecialCharacter(new Vector(),1,'︿')},{'〉',new SpecialCharacter(new Vector(),1,'﹀')},
            {'【',new SpecialCharacter(new Vector(),1,'︻')},{'】',new SpecialCharacter(new Vector(),1,'︼')},
            {'≪',new SpecialCharacter(new Vector(),1,'︽')},{'≫',new SpecialCharacter(new Vector(),1,'︾')},
            {'《',new SpecialCharacter(new Vector(),1,'︽')},{'》',new SpecialCharacter(new Vector(),1,'︾')},
            {'(',new SpecialCharacter(new Vector(),1,'︵')},{')',new SpecialCharacter(new Vector(),1,'︶')},
            {'→',new SpecialCharacter(new Vector(),1,'↓')},{'：',new SpecialCharacter(new Vector(),1,'‥')},
            {'=',new SpecialCharacter(new Vector(),1,'║')}, {'〔',new SpecialCharacter(new Vector(),1,'︹')},
            {'〕',new SpecialCharacter(new Vector(),1,'︺')}, {'_',new SpecialCharacter(new Vector(),1,'∣')},
            {'~',Wiggle},{'∼',Wiggle},{'～',Wiggle},{'\u0027',new SpecialCharacter(new Vector(),1,'︑')},
            {'゠',new SpecialCharacter(new Vector(),1,'║')},{'＝',new SpecialCharacter(new Vector(),1,'║')}
            ,{'〟',new SpecialCharacter(new Vector(0,-0.8),1.3f,'〟')},{'〝',new SpecialCharacter(new Vector(0,0.5),1.3f,'〝') }
         //   ,{'〟',new SpecialCharacter(new Vector(-0.15,-0.1),1.3f,'〝')},{'〝',new SpecialCharacter(new Vector(0.15,-0.43),1.3f,'〟') }
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
