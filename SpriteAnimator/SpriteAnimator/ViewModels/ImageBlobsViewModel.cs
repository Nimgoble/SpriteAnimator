using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Caliburn.Micro;
using AForge.Imaging;

using SpriteAnimator.Events;

namespace SpriteAnimator.ViewModels
{
    public class ImageBlobsViewModel : Screen, IHandle<ImageLoadedEvent>
    {
        private readonly IEventAggregator eventAggregator;
        public ImageBlobsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
        }

        #region Methods
        public void ProcessImage()
        {
            try
            {
                if (currentImage != null)
                {
                    //This one is for AForge
                    System.Drawing.Bitmap bitmap = AForge.Imaging.Image.FromFile(currentImage.UriSource.AbsolutePath);
                    System.Drawing.Bitmap bitmap2 = AForge.Imaging.Image.Clone(bitmap, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.ProcessImage(bitmap2);

                    blobs.Clear();
                    Blob[] blobsArray = blobCounter.GetObjectsInformation();
                    foreach (Blob blob in blobsArray)
                    {
                        blobs.Add(blob);
                    }
                }
                else
                {
                    blobs.Clear();
                }
            }
            catch (Exception ex)
            {
                String debugMe = String.Empty;
            }
        }
        #endregion

        #region IHandle
        public void Handle(ImageLoadedEvent ev)
        {
            this.CurrentImage = ev.Image;
        }
        #endregion

        #region Properties
        private BitmapImage currentImage = null;
        public BitmapImage CurrentImage
        {
            get { return currentImage; }
            set
            {
                if (value == currentImage)
                    return;

                currentImage = value;
                ProcessImage();
                NotifyOfPropertyChange(() => CurrentImage);
            }
        }

        private ObservableCollection<Blob> blobs = new ObservableCollection<Blob>();
        public ObservableCollection<Blob> Blobs
        {
            get { return blobs; }
            set
            {
                if (value == blobs)
                    return;

                blobs = value;
                NotifyOfPropertyChange(() => Blobs);
            }
        }

        private ObservableCollection<Blob> selectedBlobs = new ObservableCollection<Blob>();
        public ObservableCollection<Blob> SelectedBlobs
        {
            get { return selectedBlobs; }
            set
            {
                if (value == selectedBlobs)
                    return;

                selectedBlobs = value;
                NotifyOfPropertyChange(() => SelectedBlobs);
            }
        }
        #endregion
    }
}
