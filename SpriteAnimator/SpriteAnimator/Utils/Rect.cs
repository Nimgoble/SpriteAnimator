using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteAnimator.Utils
{
    public class Rect
    {
		public Rect() { }
		public Rect(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
        public int Area { get { return Width * Height; } }
		public System.Windows.Int32Rect ToInt32Rect()
		{
			return new System.Windows.Int32Rect(X, Y, Width, Height);
		}
    }
}
