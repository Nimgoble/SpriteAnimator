using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpriteAnimator.Utils;

namespace SpriteAnimator.Wrappers
{
    public class MaxRectsTexturePacker : ITexturePacker
    {
        private MaxRectsPacker packer;
        public MaxRectsTexturePacker()
        {
            Reset();
        }
        public string Name { get { return "Max Rects Packer(With Padding)"; } }
        public Rect Pack(Rect source)
        {
            return packer.Add(source.Width, source.Height);
        }
        public IEnumerable<Rect> PackAll(IEnumerable<Rect> source)
        {
            return packer.Add(source.ToList());
        }
        public void SetPackingMethod(PackingMethod packingMethod)
        {
        }
        private static List<PackingMethod> packingMethods = new List<PackingMethod>();
        public IEnumerable<PackingMethod> AvailablePackingMethods { get { return packingMethods; } }
        public void Reset()
        {
            this.packer = new MaxRectsPacker(4096, 4096, 2, false, false);
        }
        public IEnumerable<Rect> GetFreeRectangles()
        {
            return packer.FreeRects;
        }
        public Rect GetTextureSize()
        {
            return new Rect(0, 0, packer.Width, packer.Height);
        }
    }
}
