using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using SpriteAnimator.Utils;

namespace SpriteAnimator.Wrappers
{
    public class MaxRectsBinTexturePacker : ITexturePacker
    {
        private MaxRectsBinPack packer;
        private PackingMethod packingMethod = PackingMethod.BestAreaFit;
        private Size size;
        public MaxRectsBinTexturePacker()
        {
            Reset();
        }
        public string Name { get { return "Max Rects Bin"; } }
        public Rect Pack(Rect source)
        {
            return packer.Insert(source.Width, source.Height, PackingMethodToFreeRectChoiceHeuristic(packingMethod));
        }
        public IEnumerable<Rect> PackAll(IEnumerable<Rect> source)
        {
            return packer.Insert(source.ToList(), PackingMethodToFreeRectChoiceHeuristic(packingMethod));
        }
        public void SetPackingMethod(PackingMethod packingMethod)
        {
            this.packingMethod = packingMethod;
        }
        private static List<PackingMethod> packingMethods = new List<PackingMethod>()
        {
            PackingMethod.BestAreaFit,
            PackingMethod.BestLongSideFit,
            PackingMethod.BestShortSideFit,
            PackingMethod.BottomLeftRule,
            PackingMethod.ContactPointRule
        };
        public IEnumerable<PackingMethod> AvailablePackingMethods { get { return packingMethods; } }
        public void Reset()
        {
            this.packer = new MaxRectsBinPack(int.MaxValue - 1, int.MaxValue - 1, false);
        }
        public IEnumerable<Rect> GetFreeRectangles()
        {
            return packer.FreeRects;
        }
        public Rect GetTextureSize()
        {
            return new Rect(0, 0, packer.Width, packer.Height);
        }
        public void SetTextureSize(Size sive) { }
        private MaxRectsBinPack.FreeRectChoiceHeuristic PackingMethodToFreeRectChoiceHeuristic(PackingMethod packingMethod)
        {
            switch(packingMethod)
            {
                case PackingMethod.BestAreaFit:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit;
                case PackingMethod.BestLongSideFit:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit;
                case PackingMethod.BestShortSideFit:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit;
                case PackingMethod.BottomLeftRule:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule;
                case PackingMethod.ContactPointRule:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule;
                default:
                    return MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit;
            }
        }
    }
}
