using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpriteAnimator.Utils;

namespace SpriteAnimator.Wrappers
{
    class ChevyRayTexturePacker : ITexturePacker
    {
        private RectanglePacker packer = new RectanglePacker();
        private PackingMethod packingMethod;
        public ChevyRayTexturePacker()
        {
        }
        public string Name { get { return "Chevy Ray"; } }
        public Rect Pack(Rect source)
        {
            int x, y;
            return packer.Pack(source.Width, source.Height, out x, out y) ? new Rect(x, y, source.Width, source.Height) : null;
        }
        public IEnumerable<Rect> PackAll(IEnumerable<Rect> source)
        {
            return source.Select(x => Pack(x)).ToList();
        }
        public void SetPackingMethod(PackingMethod packingMethod) { }
        private static List<PackingMethod> packingMethods = new List<PackingMethod>();
        public IEnumerable<PackingMethod> AvailablePackingMethods { get { return packingMethods; } }
        public void Reset()
        {
            this.packer = new RectanglePacker();
        }
    }
}
