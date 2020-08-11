using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPUBParser
{
    public class PackageInfo
    {
        public List<ManifestItem> Manifest;
        public List<SpineItem> Spine;
        public List<GuideItem> Guide;
        public bool RightToLeft;
        public bool Vertical;
        public string Creator;
        public string Title;
        public string Language;

        public PackageInfo(TextFile file)
        {
            Logger.Report(string.Format("Parsing package file \"{0}\"", file.Name), LogType.Info);
            Manifest = new List<ManifestItem>();
            Spine = new List<SpineItem>();
            Guide = new List<GuideItem>();

            var doc = HTMLParser.Parse(file);
            var PackageNode = HTMLParser.SafeNodeGet(doc.DocumentNode, "package");
            if (PackageNode == null) return;

            var MetadataNode = HTMLParser.SafeNodeGet(PackageNode, "metadata"); PackageNode.Element("metadata");

            if (MetadataNode != null)
            {
                Title = HTMLParser.SafeValueGet(MetadataNode, "dc:title");
                Creator = HTMLParser.SafeValueGet(MetadataNode, "dc:creator");
                Language = HTMLParser.SafeValueGet(MetadataNode, "dc:language");
                Vertical = GlobalSettings.IsVerticalLanguage(Language);
            }

            var ManifestNode = HTMLParser.SafeNodeGet(PackageNode, "manifest");
            if (ManifestNode != null)
            {
                var ItemNodes = ManifestNode.Elements("item");
                if (ItemNodes == null || ItemNodes.Count() == 0)
                {
                    Logger.Report("no manifest item nodes found", LogType.Error);
                }
                else
                {
                    foreach (var ItemNode in ItemNodes)
                    {
                        var Item = new ManifestItem();
                        Item.Id = HTMLParser.SafeAttributeGet(ItemNode, "id");
                        Item.Path = HTMLParser.SafeAttributeGet(ItemNode, "href");
                        var MediaTypeString = HTMLParser.SafeAttributeGet(ItemNode, "media-type");
                        switch (MediaTypeString)
                        {
                            case "application/xhtml+xml":
                                Item.Type = MediaType.xhtml;
                                break;
                            case "application/x-dtbncx+xml":
                                Item.Type = MediaType.toc;
                                break;
                            case "text/css":
                                Item.Type = MediaType.css;
                                break;
                            case "image/jpeg":
                                Item.Type = MediaType.image;
                                break;
                            case "":
                                Logger.Report("Media type missing", LogType.Error);
                                Item.Type = MediaType.empty;
                                break;
                            case null:
                                Logger.Report("Media type missing" + MediaTypeString, LogType.Error);
                                Item.Type = MediaType.empty;
                                break;
                            default:
                                Logger.Report("unknown media type: " + MediaTypeString, LogType.Error);
                                Item.Type = MediaType.unknown;
                                break;
                        }
                        Manifest.Add(Item);
                    }
                }
            }

            var SpineNode = HTMLParser.SafeNodeGet(PackageNode, "spine");
            if (SpineNode != null)
            {
                RightToLeft = HTMLParser.SafeAttributeGet(SpineNode, "page-progression-direction") == "rtl";
                var SpineNodes = SpineNode.Elements("itemref");
                if (SpineNodes == null || SpineNodes.Count() == 0)
                {
                    Logger.Report("no spine item nodes found", LogType.Error);
                }
                else
                {
                    foreach (var Node in SpineNodes)
                    {
                        var NewItem = new SpineItem();
                        NewItem.Id = HTMLParser.SafeAttributeGet(Node, "idref");
                        string LinearString = HTMLParser.SafeAttributeGet(Node, "linear", true);
                        if (string.IsNullOrEmpty(LinearString))
                        {
                            NewItem.Linear = true;
                        }
                        else
                        {
                            NewItem.Linear = HTMLParser.SafeAttributeGet(Node, "linear") == "yes";
                        }
                        Spine.Add(NewItem);
                    }
                    if (!Spine.First().Linear)
                    {
                        Spine = Spine.OrderBy(a => a.Id).ToList();
                    }
                }
            }

            var GuideNode = HTMLParser.SafeNodeGet(PackageNode, "guide");
            if (GuideNode != null)
            {
                var ReferenceNodes = GuideNode.Elements("reference");
                if (ReferenceNodes == null || ReferenceNodes.Count() == 0)
                {
                    Logger.Report("no reference nodes found", LogType.Error);
                }
                else
                {
                    foreach (var Node in ReferenceNodes)
                    {
                        var NewReference = new GuideItem();
                        string RawPath = HTMLParser.SafeAttributeGet(Node, "href");
                        if (RawPath.Contains('#'))
                        {
                            RawPath = RawPath.Split('#')[0];
                        }
                        NewReference.Path = RawPath;
                        NewReference.Title = HTMLParser.SafeAttributeGet(Node, "title");
                        var TypeString = HTMLParser.SafeAttributeGet(Node, "type");
                        switch (TypeString)
                        {
                            case "title-page":
                                NewReference.Type = GuideItemType.TitlePage;
                                break;
                            case "title":
                                NewReference.Type = GuideItemType.TitlePage;
                                break;
                            case "toc":
                                NewReference.Type = GuideItemType.Toc;
                                break;
                            case "text":
                                NewReference.Type = GuideItemType.Text;
                                break;
                            case "cover":
                                NewReference.Type = GuideItemType.Cover;
                                break;
                            case "":
                                NewReference.Type = GuideItemType.empty;
                                break;
                            case null:
                                NewReference.Type = GuideItemType.empty;
                                break;
                            default:
                                NewReference.Type = GuideItemType.unknown;
                                Logger.Report("unknown reference type in guide: " + TypeString, LogType.Error);
                                break;
                        }
                        Guide.Add(NewReference);
                    }
                }
            }
        }
    }

    public class ManifestItem
    {
        public string Path;
        public string Id;
        public MediaType Type;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ManifestItem))
            {
                return false;
            }
            var other = (ManifestItem)obj;
            return other.Path == Path && other.Id == Id && other.Type == Type;
        }

        public override int GetHashCode()
        {
            return Path.Length * Id.Length * (1 + Type.GetHashCode());
        }
    }

    public enum MediaType
    {
        xhtml, toc, css, image, empty, unknown
    }

    public class SpineItem
    {
        public string Id;
        public bool Linear;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(SpineItem))
            {
                return false;
            }
            var other = (SpineItem)obj;
            return other.Id == Id && other.Linear == Linear;
        }

        public override int GetHashCode()
        {
            int Hash = Id.Length;
            if (Linear)
            {
                Hash *= (Hash + 50);
            }
            return Hash;
        }
    }

    public class GuideItem
    {
        public string Path;
        public string Title;
        public GuideItemType Type;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(GuideItem))
            {
                return false;
            }
            var other = (GuideItem)obj;
            return other.Path == Path && other.Title == Title && other.Type == Type;
        }

        public override int GetHashCode()
        {
            return Path.Length * Title.Length * (1 + Type.GetHashCode());
        }
    }

    public enum GuideItemType
    {
        TitlePage, Toc, Text, Cover,
        empty,
        unknown
    }
}
