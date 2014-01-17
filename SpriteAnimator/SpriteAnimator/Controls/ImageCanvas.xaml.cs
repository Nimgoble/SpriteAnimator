using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpriteAnimator.Controls
{
    /// <summary>
    /// Interaction logic for ImageCanvas.xaml
    /// </summary>
    public partial class ImageCanvas : Canvas
    {
        public static readonly DependencyProperty _ImageSource = DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(ImageCanvas), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(_ImageSource); }
            set
            {
                SetValue(_ImageSource, value);
                imageDrawLocation = new Rect(0, 0, value.PixelWidth, value.PixelHeight);
                this.InvalidateVisual();
            }
        }

        //private BitmapImage sourceImage = null;
        private Rect imageDrawLocation = new Rect();
        public ImageCanvas()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ImageSource != null)
                dc.DrawImage(ImageSource, imageDrawLocation);

            base.OnRender(dc);
        }

        /*public void SetImage(String imageLocation)
        {
            BitmapImage image = new BitmapImage(new Uri(imageLocation));
            sourceImage = image;
            imageDrawLocation = new Rect(0, 0, sourceImage.PixelWidth, sourceImage.PixelHeight);
        }*/
    }
}
