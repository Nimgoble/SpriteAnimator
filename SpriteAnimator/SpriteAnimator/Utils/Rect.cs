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
        public Rect(Rect other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }
        public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
        public int Area { get { return Width * Height; } }
        public int Top { get { return Y; } }
        public int Right { get { return X + Width; } }
        public int Bottom { get { return Y + Height; } }
        public int Left { get { return X; } }
		public System.Windows.Int32Rect ToInt32Rect()
		{
			return new System.Windows.Int32Rect(X, Y, Width, Height);
		}
        public bool IntersectsWith(Rect other)
        {
            return (other.X < this.X + this.Width) &&
            (this.X < (other.X + other.Width)) &&
            (other.Y < this.Y + this.Height) &&
            (this.Y < other.Y + other.Height);
        }
    }
}
