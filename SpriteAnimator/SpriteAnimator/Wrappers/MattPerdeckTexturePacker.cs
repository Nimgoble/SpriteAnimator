using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpriteAnimator.Utils;
using Mapper;

namespace SpriteAnimator.Wrappers
{
    public class MattPerdeckTexturePacker : ITexturePacker
    {
        public MattPerdeckTexturePacker()
        {

        }
        public string Name { get { return "Invalid"; } }
        public Rect Pack(Rect source)
        {
            return null;
        }
        public IEnumerable<Rect> PackAll(IEnumerable<Rect> source)
        {
            return null;
        }
        public void SetPackingMethod(PackingMethod packingMethod) { }
        private static List<PackingMethod> packingMethods = new List<PackingMethod>();
        public IEnumerable<PackingMethod> AvailablePackingMethods { get { return packingMethods; } }
        public void Reset() { }
        public IEnumerable<Rect> GetFreeRectangles() { return null; }
        public Rect GetTextureSize() { return null; }

        //public class Sprite : ISprite
        //{

        //}
    }
}
