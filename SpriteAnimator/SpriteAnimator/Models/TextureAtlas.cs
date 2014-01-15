using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SpriteAnimator.Models
{
    [XmlRoot("TextureAtlas")]
    public class TextureAtlas
    {
        public TextureAtlas()
        {
            SubTextures = new List<SubTexture>();
        }

        [XmlAttribute("imagePath")]
        public String ImagePath { get; set; }

        [XmlElement("SubTexture")]
        public List<SubTexture> SubTextures { get; set; }
    }
}
