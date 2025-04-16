using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EPUBParser
{
    public class ImageLinePart : BaseLinePart
    {
        private ImageObject ImageData;
        public bool Inline;

        public ImageObject GetImage()
        {
            if (this.ImageData == null)
            {
                Logger.Report($"image at \"{Text}\" missing", LogType.Error);
                return null;
            }
            return this.ImageData;
        }

        public ImageLinePart(string Path, bool Inline, LineSplitInfo info) : base(info)
        {
            Text = Path;
            Type = LinePartTypes.image;
            this.Inline = Inline;
        }
        private static HttpClient client = new HttpClient();
        public async Task SetImage(List<ZipEntry> Entries, ZipEntry PageEntry)
        {
            byte[] data = null;
            this.ImageData = null;
            if (Text.StartsWith("http"))
            {
                try
                {
                    data = await client.GetByteArrayAsync(this.Text);
                }
                catch (Exception)
                {
                    Logger.Report(string.Format("failed to download image from \"{0}\"", Text), LogType.Error);
                }
            }
            else
            {
                ZipEntry entry = ZipEntry.GetEntryByPath(Entries, Text, PageEntry);
                if (entry is null)
                {
                    Logger.Report($"Image from \"{Text}\" not found", LogType.Error);
                    return;
                }
                data = entry.Content;
            }
            try
            {
                double width, height;
                using (var memStream = new MemoryStream(data))
                {
                    var decoder = BitmapDecoder.Create(memStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    var frame = decoder.Frames.First();
                    width = frame.PixelWidth;
                    height = frame.PixelHeight;

                }
                this.ImageData = new()
                {
                    Data = data,
                    Width = width,
                    Height = height
                };
            }
            catch (Exception)
            {
                Logger.Report(string.Format("Image from \"{0}\" not found", Text), LogType.Error);
            }
        }
    }

    public class ImageObject
    {
        public byte[] Data;
        public double Width;
        public double Height;
    }
}
