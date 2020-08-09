using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using EPUBReader;

namespace EPUBParser
{
    public class RawBookData
    {
        public List<TextFile> Chapters;
        public List<TextFile> CSSFiles;

        public RawBookData()
        {
            Chapters = new List<TextFile>();
            CSSFiles = new List<TextFile>();
        }
    }

    //Test needed!!!!!!!!!!!!!!!
    public class TextFile : BaseFile
    {
       public string[] Lines;
        public TextFile(byte[] Data)
        {
            Lines = Encoding.UTF8.GetString(Data).Split('\n');
        }
    }

    //Test needed!!!!!!!!!!!!
    public class ImageFile : BaseFile
    {
      public  byte[] ImageData;

        public Image GetImage()
        {
            Image Result = null;
            try
            {
                using (var Stream = new MemoryStream(ImageData, 0, ImageData.Length, false, false))
                {
                    Result = Image.FromStream(Stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("Couldn't load image {0}", Name));
                Logger.Report(ex);
            }
           
            return Result;
        }
    }

    public class BaseFile
    {
      public  string Name;
    }
}
