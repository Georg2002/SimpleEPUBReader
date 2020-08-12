using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace EPUBParser
{
    public class TextFile : BaseDataFile
    {
        public string Text;

        //    public TextFile(byte[] ByteData) : base(ByteData) { }

        public TextFile(ZipEntry ZipEntry) : base(ZipEntry) { }

        public override void Init(byte[] ByteData)
        {
            if (ByteData == null)
            {
                Text = "";
                Logger.Report(string.Format("no byte data given, can't set text of text file \"{0}\""
                    , FullName), LogType.Error);
            }
            else
            {              
                Text = Encoding.UTF8.GetString(ByteData);
            }
        }
    }

    public class ImageFile : BaseDataFile
    {
        public byte[] ImageData;

        //   public ImageFile(byte[] ByteData) : base(ByteData) { }

        public ImageFile(ZipEntry ZipEntry) : base(ZipEntry) { }

        public override void Init(byte[] ByteData)
        {
            ImageData = ByteData;
        }

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
                Logger.Report(string.Format("Couldn't load image \"{0}\"", Name), LogType.Error);
                Logger.Report(ex);
            }
            return Result;
        }
    }

    public class BaseDataFile : IBaseFile
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        //   public BaseDataFile(byte[] ByteData)
        //   {
        //       Init(ByteData);
        //   }
        public BaseDataFile(ZipEntry ZipEntry)
        {
            if (ZipEntry == null)
            {
                FullName = Name = "#ERROR#";
                Logger.Report("zip entry is null, can't initialize base data file", LogType.Error);
            }
            else
            {
                FullName = ZipEntry.FullName;
                Name = ZipEntry.Name;
                Init(ZipEntry.Content);
            }
        }

        public virtual void Init(byte[] ByteData)
        {

        }
    }

    public interface IBaseFile
    {
        string Name { get; set; }
        string FullName { get; set; }
    }
}
