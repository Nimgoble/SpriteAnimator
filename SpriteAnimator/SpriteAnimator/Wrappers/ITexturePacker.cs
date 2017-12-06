using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpriteAnimator.Utils;

namespace SpriteAnimator.Wrappers
{
    public enum PackingMethod
    {
        BestShortSideFit, //< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        BestLongSideFit, //< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        BestAreaFit, //< -BAF: Positions the rectangle into the smallest free rect into which it fits.
        BottomLeftRule, //< -BL: Does the Tetris placement.
        ContactPointRule,
        Horizontal,
        Vertical
    }
    public interface ITexturePacker
    {
        string Name { get; }
        Rect Pack(Rect source);
        IEnumerable<Rect> PackAll(IEnumerable<Rect> source);
        void SetPackingMethod(PackingMethod packingMethod);
        IEnumerable<PackingMethod> AvailablePackingMethods { get; }
        void Reset();
        IEnumerable<Rect> GetFreeRectangles();
        Rect GetTextureSize();
    }
}
