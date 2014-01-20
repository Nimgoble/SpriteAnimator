using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

using System.Runtime.CompilerServices;
using System.Diagnostics;

using AForge.Imaging;

namespace SpriteAnimator.Controls
{
    /// <summary>
    /// Interaction logic for AForgeBlobDisplay.xaml
    /// </summary>
    public partial class AForgeBlobDisplay : UserControl
    {
        public static readonly DependencyProperty BlobImageProperty = DependencyProperty.Register
        (
            "BlobImage", 
            typeof(BitmapImage), 
            typeof(AForgeBlobDisplay), 
            new FrameworkPropertyMetadata
            (
                null,
                BlobImageChanged,
                BlobImageCoerced
            ),
            BlobImageValidation
        );

        private static void BlobImageChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            String debugMe = String.Empty;
        }

        private static object BlobImageCoerced(DependencyObject sender, object data)
        {
            return data;
        }

        private static Boolean BlobImageValidation(object data)
        {
            String debugMe = String.Empty;
            return true;
        }

        private BitmapImage blobImage = null;
        public BitmapImage BlobImage
        {
            get { return (BitmapImage)GetValue(BlobImageProperty); }
            set { SetValue(BlobImageProperty, value); }
        }

        private ImageCanvas imageCanvas = null;
        public AForgeBlobDisplay()
        {
            /*
                blobImage = value;
                if (this.imageCanvas != null)
                    imageCanvas.ImageSource = value;

                ProcessImage();

                this.InvalidateVisual();
            */ 
            InitializeComponent();
            this.itemsControl.SelectionChanged += SelectedItemsChanged;
        }

        #region Overrides

        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = (blobImage == null) ? base.MeasureOverride(constraint) : new Size(blobImage.PixelWidth + 25, blobImage.PixelHeight + 25);
            return desiredSize;
        }

        #endregion

        #region Callbacks

        private void SelectedItemsChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Blob blob in e.AddedItems)
            {
                this.SelectedItems.Add(blob);
            }

            foreach (Blob blob in e.RemovedItems)
            {
                this.SelectedItems.Remove(blob);
            }
        }

        private void ImageCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            imageCanvas = sender as ImageCanvas;
        }

        #endregion

        #region Methods

        public void ProcessImage()
        {
            try
            {
                if (blobImage != null)
                {
                    //This one is for AForge
                    System.Drawing.Bitmap bitmap = AForge.Imaging.Image.FromFile(blobImage.UriSource.AbsolutePath);
                    System.Drawing.Bitmap bitmap2 = AForge.Imaging.Image.Clone(bitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.ProcessImage(bitmap2);

                    items.Clear();
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    foreach (Blob blob in blobs)
                    {
                        items.Add(blob);
                    }

                    this.InvalidateMeasure();
                }
                else
                {
                    items.Clear();
                }
            }
            catch (Exception ex)
            {
                String debugMe = String.Empty;
            }
        }

        #endregion

        #region Properties

        private ObservableCollection<Blob> items = new ObservableCollection<Blob>();
        public ObservableCollection<Blob> Items
        {
            get { return items; }
            set
            {
                if (value == items)
                    return;

                items = value;
            }
        }

        private ObservableCollection<Blob> selectedItems = new ObservableCollection<Blob>();
        public ObservableCollection<Blob> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                if (value == selectedItems)
                    return;

                selectedItems = value;
            }
        }
        #endregion
    }
}
