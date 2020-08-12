using System.IO.Compression;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace EPUBParser
{
    public static class Unzipper
    {
        //returns empty list if failed
        public static List<ZipEntry> GetFiles(string FilePath)
        {
            var Files = new List<ZipEntry>();
            if (!File.Exists(FilePath))
            {
                Logger.Report(String.Format("File {0} doesn't exist", FilePath), LogType.Error);
                return Files;
            }

            try
            {
                using (var FileStream = File.OpenRead(FilePath))
                {
                    try
                    {
                        using (var zip = new ZipArchive(FileStream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                try
                                {
                                    using (var stream = entry.Open())
                                    {
                                        Logger.Report(string.Format("Reading file {0}...", entry.FullName), LogType.Info);
                                        var NewEntry = new ZipEntry
                                        {                                            
                                            Name = entry.Name,
                                            FullName = entry.FullName
                                        };

                                        using (var MemoryStream = new MemoryStream())
                                        {
                                            stream.CopyTo(MemoryStream);
                                            NewEntry.Content = MemoryStream.ToArray();
                                        }
                                        Files.Add(NewEntry);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Report(string.Format("Failed to open zip entry {0} in file {1}:", entry.Name, FilePath), LogType.Error);
                                    Logger.Report(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Report(string.Format("Failed to open zip file at {0}:", FilePath), LogType.Error);
                        Logger.Report(ex);
                        return Files;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("Failed to open file stream to {0}:", FilePath), LogType.Error);
                Logger.Report(ex);
                return Files;
            }
            return Files;
        }     
    }
}
