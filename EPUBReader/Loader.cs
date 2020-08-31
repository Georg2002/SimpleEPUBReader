using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EPUBReader
{
    public static class Loader
    {
        public static SaveObject? Load()
        {
            var SaveFolder = GlobalSettings.GetSaveFolderPath();
            var SaveFile = Path.Combine(SaveFolder, GlobalSettings.SaveFileName);
            if (File.Exists(SaveFile))
            {
                var Serializer = new XmlSerializer(typeof(SaveObject));
                using (var Reader = new StreamReader(SaveFile))
                {
                    try
                    {
                        return (SaveObject)Serializer.Deserialize(Reader);
                    }
                    catch (Exception ex)
                    {
                        Logger.Report(ex);
                    }
                }
            }
            return null;
        }
    }
}
