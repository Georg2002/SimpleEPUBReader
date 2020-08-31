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
            Save.Markings = ViewerInteracter.GetAllMarkings();
            Save.LastOpen = ViewerInteracter.GetCurrentPath();
            Save.LastRenderPageIndex = ViewerInteracter.GetCurrentRenderPage();
            Save.RenderPageRatio = ViewerInteracter.GetCurrentRenderPageRatio();

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
        }
    }
}
