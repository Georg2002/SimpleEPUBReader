using EPUBRenderer3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace EPUBReader2
{
    static class SaveAndLoad
    {
        const string FileName = "saveV2.xml";

        internal static SaveStruc LoadSave()
        {
            var SaveFolder = GetSaveFolder();
            var SaveFile = Path.Combine(SaveFolder, FileName);
            if (File.Exists(SaveFile))
            {
                var Serializer = new XmlSerializer(typeof(SaveStruc));
                using (var Reader = new StreamReader(SaveFile))
                {
                    try
                    {
                        return (SaveStruc)Serializer.Deserialize(Reader);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Loading save filed failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            return new SaveStruc();
        }

        internal static void Save(SaveStruc saveStruc)
        {
            string SaveFolder = GetSaveFolder();
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }
            string SaveFile = Path.Combine(SaveFolder, FileName);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveStruc));
            using (var Writer = new StreamWriter(SaveFile))
            {
                serializer.Serialize(Writer, saveStruc);
            }
        }

        private static string GetSaveFolder()
        {
            var AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(AppData, "SimpleEpubReader");
        }
    }

    public struct SaveStruc
    {
        public bool KatakanaLearningMode;
  public int CurrentBookIndex;
        public bool Fullscreen;
        public Vector WindowSize;
        public string LastDirectory;
        public byte ColorIndex;
        public bool DictOpen;
        public List<LibraryBook> Books;      
    }
}
