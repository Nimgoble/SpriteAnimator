using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpriteAnimator.ViewModels;

namespace SpriteAnimator.Events
{
    public class AtlasLoadedEvent
    {
        public TextureAtlasViewModel Atlas { get; set; }
    }
}
