using EPUBRenderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace EPUBReader
{
    public static class Saver
    {
        public static void Save()
        {
            var Save = new SaveObject
            {
                Nightmode = GlobalSettings.Nightmode,
                Fullscreen = Application.Current.MainWindow.WindowState == WindowState.Maximized
            };
            if (!string.IsNullOrEmpty(ViewerInteracter.GetCurrentPath()))
            {
                var CurrentBook = new BookDefinition
                {
                    Markings = ViewerInteracter.GetAllMarkings(),
                    FilePath = ViewerInteracter.GetCurrentPath(),
                    LastRenderPageIndex = ViewerInteracter.GetCurrentRenderPage(),
                    RenderPageRatio = ViewerInteracter.GetCurrentRenderPageRatio()
                };
                LibraryManager.UpdateCurrentBook();
                Save.LastBook = CurrentBook;
                Save.LibraryBooks = LibraryManager.Books;
                Save.MarkingColor = ViewerInteracter.MarkingColor;
            }         

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
