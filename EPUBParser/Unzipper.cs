using System.IO.Compression;
using System.IO;
using System;
using System.Collections.Generic;
using EPUBReader;
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
                Logger.Report(String.Format("File {0} doesn't exist", FilePath));
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
                                        Logger.Report(string.Format("Reading file {0}...", entry.FullName));
                                        var NewEntry = new ZipEntry();
                                        NewEntry.EntryType = ZipEntryType.File;
                                        NewEntry.Name = entry.Name;
                                        NewEntry.FullName = entry.FullName;
                                        int Length = (int)stream.Length;
                                        NewEntry.Content = new byte[Length];
                                        stream.Read(NewEntry.Content, 0, Length);
                                        AddFileWithFolders(Files, NewEntry);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Report(string.Format("Failed to open zip entry {0} in file {1}:", entry.Name, FilePath));
                                    Logger.Report(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Report(string.Format("Failed to open zip file at {0}:", FilePath));
                        Logger.Report(ex);
                        return Files;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Report(string.Format("Failed to open file stream to {0}:", FilePath));
                Logger.Report(ex);
                return Files;
            }
            return Files;
        }

        private static void AddFileWithFolders(List<ZipEntry> Files, ZipEntry NewEntry)
        {
            //"Testsubfolder/Test2.txt"
            //"Test2.txt"
            if (NewEntry.Name != NewEntry.FullName)
            {
                string[] FolderTree = NewEntry.FullName.Split('/');
                bool Missing = false;
                string FullName = "";
                List<ZipEntry> LastFolderEntries = Files;
                for (int i = 0; i < FolderTree.Length - 1; i++)
                {
                    string Folder = FolderTree[i];
                    FullName += Folder;
                    ZipEntry NewFolder = null;
                    if (Missing || !LastFolderEntries.Exists(a => a.Name == Folder))
                    {
                        Missing = true;
                        NewFolder = new ZipEntry();
                        NewFolder.EntryType = ZipEntryType.Folder;
                        NewFolder.Name = Folder;
                        NewFolder.FullName = FullName;
                        LastFolderEntries.Add(NewFolder);
                    }
                    if (Missing)
                    {
                        LastFolderEntries = NewFolder.Subentries;
                    }
                    else
                    {
                        LastFolderEntries = LastFolderEntries.First(a => a.Name == Folder).Subentries;
                    }
                    FullName += "/";
                }
                LastFolderEntries.Add(NewEntry);
            }
            else
            {
                Files.Add(NewEntry);
            }
        }
    }
}
