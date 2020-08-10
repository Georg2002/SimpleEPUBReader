using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPUBParser
{
    public enum ZipEntryType
    {
        File, Folder
    }

    public class ZipEntry
    {
        public string Name;
        public string FullName;
        public byte[] Content;
        public List<ZipEntry> Subentries;
        public ZipEntryType EntryType;

        public ZipEntry()
        {
            Subentries = new List<ZipEntry>();
        }

        public override string ToString()
        {
            return FullName;
        }

        public static ZipEntry GetEntry(List<ZipEntry> Files, string FullName)
        {
            return GetEntry(Files, FullName.Split('/'), 0);
        }

        private static ZipEntry GetEntry(List<ZipEntry> Files, string[] FullName, int Step)
        {         
            ZipEntry Entry = Files.FirstOrDefault(a => a.Name == FullName[Step]);
            if (Entry == null)
            {
                return null;
            }
            else
            {
                if (Step == FullName.Length -1)
                {
                    return Entry;
                }
                else
                {
                    return GetEntry(Entry.Subentries, FullName, ++Step);
                }
            }
        }
    }
}
