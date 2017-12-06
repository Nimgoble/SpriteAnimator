using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteAnimator.Utils
{
    public class MaxRectsPacker
    {
        private List<Rect> rects = new List<Rect>();
        private List<Rect> freeRects = new List<Rect>();
        public List<Rect> FreeRects { get { return freeRects; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        private bool verticalExpand = false;
        private int maxWidth;
        private int maxHeight;
        private int width;
        private int height;
        private int padding;
        private bool smart;
        private bool pot;
        private bool square;
        private const int EDGE_MAX_VALUE = 4096;
        private const int EDGE_MIN_VALUE = 128;
        public MaxRectsPacker(int? maxWidth = null, int? maxHeight = null, int padding = 0, bool smart = true, bool pot = true, bool square = true, bool verticalExpand = false)
        {
            this.maxWidth = maxWidth ?? EDGE_MAX_VALUE;
            this.maxHeight = maxHeight ?? EDGE_MAX_VALUE;
            this.width = (smart) ? 0 : this.maxWidth;
            this.height = (smart) ? 0 : this.maxHeight;
            this.padding = padding;
            this.smart = smart;
            this.pot = pot;
            this.square = square;
            this.verticalExpand = verticalExpand;
            freeRects.Add(new Rect(0, 0, this.maxWidth + padding, this.maxHeight + padding));
        }

        public Rect Add(int width, int height)
        {
            var node = FindNode(width + padding, height + padding);
            if (node != null)
            {
                UpdateBinSize(node);
                PlaceRect(node);
                this.verticalExpand = (this.width > this.height);
                node.X += this.padding;
                node.Y += this.padding;
                node.Height -= this.padding;
                node.Width -= this.padding;
                return node;
            }
            else 
            {
                Rect growHorizontal = new Rect(this.width + this.padding, 0, width + this.padding, height + this.padding);
                Rect growVertical = new Rect(0, this.height + this.padding, width + this.padding, height + this.padding);
                if (UpdateBinSize(!this.verticalExpand ? growHorizontal : growVertical) || UpdateBinSize(!this.verticalExpand ? growVertical : growHorizontal))
                    return Add(width, height);
            }

            return null;
        }

        public List<Rect> Add(List<Rect> rects)
        {
            var rtn = new Rect[rects.Count];

            var rectsCopy = new List<Rect>();
            rectsCopy.AddRange(rects);
            rectsCopy.Sort((a, b) => Math.Max(b.Width, b.Height) - Math.Max(a.Width, a.Height));

            foreach(var rect in rectsCopy)
            {
                var originalIndex = rects.IndexOf(rect);
                var bestNode = Add(rect.Width, rect.Height);
                rtn[originalIndex] = bestNode;
            }
            return rtn.ToList();
        }

        private void PlaceRect(Rect node)
        {
            int numRectanglesToProcess = freeRects.Count;
            for (int i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(freeRects[i], ref node))
                {
                    freeRects.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();
            rects.Add(node);
        }

        private Rect FindNode(int width, int height)
        {
            int area = width * height;
            var bestFreeRect = freeRects.Where(x => x.Width >= width && x.Height >= height).OrderBy(x => x.Area - area).FirstOrDefault();
            if (bestFreeRect == null)
                return null;
            var bestRect = new Rect(bestFreeRect);
            bestRect.Width = width;
            bestRect.Height = height;
            return bestRect;
        }

        private bool SplitFreeNode(Rect freeNode, ref Rect usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (!usedNode.IntersectsWith(freeNode))
                return false;

            if (usedNode.Left < freeNode.Right && usedNode.Right > freeNode.Left)
            {
                // New node at the top side of the used node.
                if (usedNode.Top > freeNode.Top && usedNode.Top < freeNode.Bottom)
                {
                    Rect newNode = new Rect(freeNode);
                    newNode.Height = usedNode.Top - newNode.Top;
                    freeRects.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Bottom < freeNode.Bottom)
                {
                    Rect newNode = new Rect(freeNode);
                    newNode.Y = usedNode.Bottom;
                    newNode.Height = freeNode.Bottom - usedNode.Bottom;
                    freeRects.Add(newNode);
                }
            }

            if (usedNode.Top < freeNode.Bottom && usedNode.Bottom > freeNode.Top)
            {
                // New node at the left side of the used node.
                if (usedNode.Left > freeNode.Left && usedNode.Left < freeNode.Right)
                {
                    Rect newNode = new Rect(freeNode);
                    newNode.Width = usedNode.Left - newNode.Left;
                    freeRects.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.Right < freeNode.Right)
                {
                    Rect newNode = new Rect(freeNode);
                    newNode.X = usedNode.Right;
                    newNode.Width = freeNode.Right - usedNode.Right;
                    freeRects.Add(newNode);
                }
            }

            return true;
        }

        private bool UpdateBinSize(Rect node)
        {
            if (!this.smart)
                return false;
            if (IsContainedIn(node))
                return false;

            var tempWidth = Math.Max(this.width, node.X + node.Width - this.padding);
            var tempHeight = Math.Max(this.height, node.Y + node.Height - this.padding);
            if(this.pot)
            {
                tempWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log(tempWidth, 2)));
                tempHeight = (int)Math.Pow(2, Math.Ceiling(Math.Log(tempHeight, 2)));
            }

            if(this.square)
            {
                tempWidth = tempHeight = Math.Max(tempWidth, tempHeight);
            }

            if (tempWidth > this.maxWidth + this.padding ||
                tempHeight > this.maxHeight + this.padding)
                return false;

            ExpandFreeRects(tempWidth + this.padding, tempHeight + this.padding);
            this.width = tempWidth;
            this.height = tempHeight;
            return true;
        }

        private void ExpandFreeRects(int width, int height)
        {
            this.freeRects.ForEach
            (
                freeRect =>
                {
                    if (freeRect.X + freeRect.Width >= Math.Min(this.width + this.padding, width))
                        freeRect.Width = width - freeRect.X;
                    if (freeRect.Y + freeRect.Height >= Math.Min(this.height + this.padding, height))
                        freeRect.Height = height - freeRect.Y;
                }
            );
            this.freeRects.Add(new Rect(0, this.height + this.padding, width, height - (this.height - this.padding)));
            this.freeRects.RemoveAll(x => x.Width <= 0 || x.Height <= 0);
            PruneFreeList();
        }

        void PruneFreeList()
        {
            freeRects.RemoveAll(x => freeRects.Where(y => y != x && IsContainedIn(x, y)).Any());
        }

        private bool IsContainedIn(Rect a, Rect b = null)
        {
            if (b == null)
                b = new Rect(0, 0, this.maxWidth, this.maxHeight);

            return a.X >= b.X && a.Y >= b.Y
                && a.X + a.Width <= b.X + b.Width
                && a.Y + a.Height <= b.Y + b.Height;
        }
    }
}
