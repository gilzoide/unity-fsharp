using System;
using System.Xml;

namespace Gilzoide.FSharp.Editor.Internal
{
    [Serializable]
    public class PackageReference
    {
        public string Name;
        public string Version;

        public XmlElement AddElementTo(XmlNode parent)
        {
            return parent.AddElement("PackageReference", "Include", Name, "Version", Version);
        }
    }
}
