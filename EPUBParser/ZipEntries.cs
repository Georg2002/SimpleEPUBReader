using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EPUBParser
{
    public enum ZipEntryType
    {
        File, Folder
    }

    public class ZipEntry : IBaseFile
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public byte[] Content;

        public override string ToString()
        {
            return FullName;
        }

        public static ZipEntry GetEntryByPath(List<ZipEntry> files, string RelativePath, ZipEntry RelativeTo)
        {
            if (files == null)
            {
                Logger.Report(string.Format("file list was null, can't search for \"{0}}\"", RelativePath), LogType.Error);
                return null;
            }
            if (RelativeTo == null)
            {
                Logger.Report(string.Format("base entry for relative path is null, can't search for \"{0}\"", RelativePath), LogType.Error);
                return null;
            }

            var RelativeBaseParts = RelativeTo.FullName.Split('/');
            List<string> PathParts = RelativeBaseParts.Take(RelativeBaseParts.Length - 1).ToList();
            var RelativeParts = RelativePath.Split('/');
            int Index = PathParts.Count - 1;
            foreach (var RelativePart in RelativeParts)
            {
                if (RelativePart == "..")
                {
                    if (Index < 0)
                    {
                        Logger.Report(string.Format("relative path \"{0}\"} invalid, can't find file", RelativePart), LogType.Error);
                        return null;
                    }
                    else
                    {
                        PathParts.RemoveAt(Index);
                        Index--;
                    }                   
                }
                else
                {
                    PathParts.Add(RelativePart);
                    Index++;
                }
            }

            string Path = "";
            for (int i = 0; i < PathParts.Count; i++)
            {
                var Part = PathParts[i];
                if (i != 0)
                {
                    Path += "/";
                }
                Path += Part;
            }

            ZipEntry Result = GetEntryByPath(files, Path);
            if (Result == null)
            {
                Logger.Report(string.Format("file \"{0}\" not found", RelativePath), LogType.Error);
            }
            return Result;
        }

        public static ZipEntry GetEntryByPath(List<ZipEntry> files, string path)
        {
            if (files == null)
            {
                Logger.Report(string.Format("file list was null, can't search for \"{0}}\"", path), LogType.Error);
                return null;
            }
            var Result = files.FirstOrDefault(a => a.FullName == path);
            if (Result == null)
            {
                Logger.Report(string.Format("file \"{0}\" not found", path), LogType.Error);
            }
            return Result;
        }

        public static ZipEntry GetEntryByName(List<ZipEntry> files, string name, bool NoError = false)
        {
            if (files == null)
            {
                Logger.Report(string.Format("file list was null, can't search for \"{0}}\"", name), LogType.Error);
                return null;
            }
            var Result = files.FirstOrDefault(a => a.Name == name);
            if (Result == null && !NoError)
            {
                Logger.Report(string.Format("file \"{0}\" not found", name), LogType.Error);
            }
            return Result;
        }
    }
}
