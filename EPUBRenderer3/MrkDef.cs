using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EPUBRenderer3
{
    public struct MrkDef
    {
        [XmlIgnore]
        public PosDef Pos;
        [XmlIgnore]
        public byte ColorIndex;

        public MrkDef(PosDef Pos,byte ColorIndex)
        {
            this.Pos = Pos;
            this.ColorIndex = ColorIndex;
        }        

        [XmlText]
        public string ShrtTxt
        {
            get
            {
                return $"{Pos.ShrtTxt};{ColorIndex}";
            }
            set
            {
                var Parts = value.Split(';');
                if (Parts.Length != 2) return;
                Pos.ShrtTxt = Parts[0];
                try
                {
                    ColorIndex = Convert.ToByte(Parts[1]);
                }            
                catch (Exception) { }
            }
        }
    }
}
