using System.IO;
using System.Net;
using System.Drawing;

namespace EPUBParser
{
    public class ImageLinePart : BaseLinePart
    {
        private byte[] ImageData;
        public bool Inline;

        public System.Drawing.Image GetImage()
        {
            if (ImageData == null)
            {
                Logger.Report(string.Format("image at \"{0}\" missing", Text), LogType.Error);
                return null;
            }
            Image img = null;
            try
            {
                using (var mem = new MemoryStream(ImageData))
                {
                    img = Image.FromStream(mem);
                    return img;
                }
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("image from \"{0}\" couldn't be loaded", Text), LogType.Error);
                Logger.Report(ex.Message, LogType.Error);
            }
            return img;
        }

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
                    using (var client = new WebClient())
                    {
                        ImageData = client.DownloadData(Text);
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
