
using System.Xml.Serialization;

namespace GenerateStrmFromRclone.Model
{
    [XmlRoot(ElementName="MIDIA")]
    public class Midia
    {
        [XmlElement(ElementName = "PATH")]
		public string? PATH { get; set; }
		[XmlElement(ElementName = "MD5")]
		public string? MD5 { get; set; }
		
    }
}