using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EPUBReader
{
    public static class Saver
    {
        public static void Save()
        {
            var Save = new SaveObject();
            Save.Nightmode = GlobalSettings.Nightmode;
            if (!string.IsNullOrEmpty(ViewerInteracter.GetCurrentPath()))
            {
                var CurrentBook = new BookDefinition();
                CurrentBook.Markings = ViewerInteracter.GetAllMarkings();
                CurrentBook.FilePath = ViewerInteracter.GetCurrentPath();
                CurrentBook.LastRenderPageIndex = ViewerInteracter.GetCurrentRenderPage();
                CurrentBook.RenderPageRatio = ViewerInteracter.GetCurrentRenderPageRatio();
                Save.LastBook = CurrentBook;
                Save.LibraryBooks = LibraryManager.Books;
            }
            LibraryManager.UpdateCurrentBook();

            string SaveFolder = GlobalSettings.GetSaveFolderPath();
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
            string SaveFile = Path.Combine(SaveFolder, GlobalSettings.SaveFileName);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveObject));
            using (var Writer = new StreamWriter(SaveFile))
            {
                serializer.Serialize(Writer, Save);
            }
            string LogPath = Path.Combine(SaveFolder, "parselog.txt");
            File.WriteAllLines(LogPath, EPUBParser.Logger.Log); 
            LogPath = Path.Combine(SaveFolder, "readerlog.txt");
            File.WriteAllLines(LogPath, EPUBReader.Logger.Log);
        }
    }
}
