using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EPUBParser
{
    public class ImageLinePart : BaseLinePart
    {
        private byte[] ImageData;
        public bool Inline;

        public ImageSource GetImage()
        {
            if (ImageData == null)
            {
                Logger.Report(string.Format("image at \"{0}\" missing", Text), LogType.Error);
                return null;
            }
            BitmapImage Image = null;
            try
            {
                Image = new BitmapImage();
                using (var mem = new MemoryStream(ImageData))
                {
                    mem.Position = 0;
                    Image.BeginInit();
                    Image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    Image.CacheOption = BitmapCacheOption.OnLoad;
                    Image.UriSource = null;
                    Image.StreamSource = mem;
                    Image.EndInit();
                }
                Image.Freeze();
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("image from \"{0}\" couldn't be loaded", Text), LogType.Error);
                Logger.Report(ex.Message, LogType.Error);
            }

            return Image;
        }

        public ImageLinePart(string Path, bool Inline)
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
