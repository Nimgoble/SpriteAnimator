using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SpriteAnimator.Models
{
    public class SubTexture
    {
        [XmlAttribute("name")]
        public String Name { get; set; }
        [XmlAttribute("x")]
        public Int32 X { get; set; }
        [XmlAttribute("y")]
        public Int32 Y { get; set; }
        [XmlAttribute("width")]
        public Int32 Width { get; set; }
        [XmlAttribute("height")]
        public Int32 Height { get; set; }
    }
}
