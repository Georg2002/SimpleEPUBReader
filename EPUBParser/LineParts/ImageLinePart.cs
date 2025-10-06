using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace EPUBParser
{
    public class ImageLinePart : BaseLinePart
    {
        public byte[] ImageData { get; private set; }
        public bool Inline;

  

        public ImageLinePart(string Path, bool Inline, LineSplitInfo info) : base(info)
        {
            this.Text = Path;
            this.Type = LinePartTypes.image;
            this.Inline = Inline;
        }

        public void SetImage(List<ZipEntry> Entries, ZipEntry PageEntry)
        {
            ImageData = null;
            if (Text.StartsWith("http"))
            {
                try
                {
                    //    using (var client = new WebClient())
                    {
                        //    ImageData = client.DownloadData(Text);
                    }
                }
                catch (Exception)
                {
                    Logger.Report(string.Format("failed to download image from \"{0}\"", Text), LogType.Error);
                }
            }
            else
            {
                ZipEntry Entry = ZipEntry.GetEntryByPath(Entries, Text, PageEntry);
                if (Entry != null)
                {
                    ImageData = Entry.Content;
                }
            }
            if (ImageData == null)
            {
                Logger.Report(string.Format("Image from \"{0}\" not found", Text), LogType.Error);
                return;
            }
        }
    }
}
