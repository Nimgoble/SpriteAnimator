using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using SpriteAnimator.Utils;

namespace SpriteAnimator.ViewModels
{
    public class RectangleViewModel
    {
        public RectangleViewModel(Rect model, Color color)
        {
            this.color = new SolidColorBrush(color);
            this.model = model;
        }

        private SolidColorBrush color;
        public SolidColorBrush Color { get { return color; } }

        private Rect model;
        public Rect Model { get { return model; } }
    }
}
