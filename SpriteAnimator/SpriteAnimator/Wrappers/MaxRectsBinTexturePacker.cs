using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpriteAnimator.Utils;

namespace SpriteAnimator.Wrappers
{
    public class MaxRectsBinTexturePacker : ITexturePacker
    {
        private MaxRectsBinPack packer;
        private PackingMethod packingMethod = PackingMethod.BestAreaFit;
        public MaxRectsBinTexturePacker()
        {
            Reset();
        }
        public string Name { get { return "Max Rects Bin"; } }
        public Rect Pack(Rect source)
        {
            return packer.Insert(source.Width, source.Height, PackingMethodToFreeRectChoiceHeuristic(packingMethod), MaxRectsBinPack.GrowDirection.DoNotGrow);
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
            this.packer = new MaxRectsBinPack(int.MaxValue, int.MaxValue, false);
        }
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
