using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using System.Timers;

using Microsoft.Win32;

using Caliburn.Micro;

using SpriteAnimator.Models;
using SpriteAnimator.Events;

namespace SpriteAnimator.ViewModels
{
    public class MainViewModel : Screen, IHandle<AtlasLoadedEvent>
    {
        #region Private Members
        private Timer animationTimer = new Timer();
        private BitmapImage defaultImage;
        private CroppedBitmap defaultCroppedImage;

        private readonly IEventAggregator eventAggregator;
        #endregion

        public MainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            animationTimer.AutoReset = true;
            animationTimer.Elapsed += animationTimer_Elapsed;
            try
            {
                defaultImage = new BitmapImage(new Uri(@"pack://application:,,,/Content/default.png"));
                defaultCroppedImage = new CroppedBitmap(defaultImage, new System.Windows.Int32Rect(10, 10, 280, 280));
            }
            catch (Exception ex)
            {
                String debugMe = ex.Message;
            }
        }

        #region Animations
        void animationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Caliburn.Micro.Execute.BeginOnUIThread
            (
                new System.Action
                (
                    () => 
                    {
                        UpdateAnimationFrame();
                    }
                )
            );
        }

        private void UpdateAnimationFrame()
        {
            if(this.SelectedFrame.Index == SelectedAnimation.FrameCount && !loopAnimation)
            {
                animationTimer.Stop();
                IsAnimationPlaying = false;
            }
            else
                SelectedFrame = (this.SelectedFrame.Index == SelectedAnimation.FrameCount) ? selectedAnimation.Frames[0] : selectedAnimation.Frames[selectedFrame.Index]; 
        }
        #endregion

        #region Commands
        public void ChooseImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false };
            if (openFileDialog.ShowDialog() == true)
                ImagePath = openFileDialog.FileName;
        }

        public void ChooseAtlasXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = false };
            if (openFileDialog.ShowDialog() == true)
                AtlasXMLPath = openFileDialog.FileName;
        }

        public void LoadImageAndAtlas()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TextureAtlas));
                using (FileStream stream = new FileStream(atlasXMLPath, FileMode.Open))
                {
                    TextureAtlas textureAtlas = serializer.Deserialize(stream) as TextureAtlas;
                    CurrentTextureAtlas = new TextureAtlasViewModel(textureAtlas, imagePath);
                    this.eventAggregator.Publish(new ImageLoadedEvent() { Image = CurrentTextureAtlas.Image });
                }
            }
            catch (Exception ex)
            {
                String debugMe = String.Empty;
            }
        }

        public Boolean CanLoadImageAndAtlas
        {
            get
            {
                return (!String.IsNullOrEmpty(atlasXMLPath) && !String.IsNullOrEmpty(imagePath));
            }
        }

        public void PlayAnimation()
        {
            Int32 framesPerSecond = -1;
            if (!Int32.TryParse(this.animationFPS, out framesPerSecond))
                return;

            //Reset the animation
            SelectedFrame = SelectedAnimation.Frames[0];

            animationTimer.Interval = 1000 / (Double)framesPerSecond;
            animationTimer.Start();
            IsAnimationPlaying = true;
        }

        public Boolean CanPlayAnimation
        {
            get { return (this.SelectedAnimation != null && isAnimationPlaying == false); }
        }

        public void StopAnimation()
        {
            animationTimer.Stop();
            IsAnimationPlaying = false;
        }

        public Boolean CanStopAnimation
        {
            get { return isAnimationPlaying; }
        }
        #endregion

        #region IHandle
        public void Handle(AtlasLoadedEvent ev)
        {
            CurrentTextureAtlas = ev.Atlas;
        }
        #endregion

        #region Properties
        private String imagePath = String.Empty;
        public String ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value == imagePath)
                    return;

                imagePath = value;
                NotifyOfPropertyChange(() => ImagePath);
                NotifyOfPropertyChange(() => CanLoadImageAndAtlas);
            }
        }

        private String atlasXMLPath = String.Empty;
        public String AtlasXMLPath
        {
            get { return atlasXMLPath; }
            set
            {
                if (value == atlasXMLPath)
                    return;

                atlasXMLPath = value;
                NotifyOfPropertyChange(() => AtlasXMLPath);
                NotifyOfPropertyChange(() => CanLoadImageAndAtlas);
            }
        }

        private TextureAtlasViewModel currentTextureAtlas = null;
        public TextureAtlasViewModel CurrentTextureAtlas
        {
            get { return currentTextureAtlas; }
            set
            {
                if (value == currentTextureAtlas)
                    return;

                currentTextureAtlas = value;
                NotifyOfPropertyChange(() => CurrentTextureAtlas);
                //NotifyOfPropertyChange(() => Animations);
            }
        }

        //public ObservableCollection<AnimationViewModel> Animations
        //{
        //    get
        //    {
        //        return (currentTextureAtlas == null) ? new ObservableCollection<AnimationViewModel>() : currentTextureAtlas.Animations;
        //    }
        //}

        private AnimationViewModel selectedAnimation = null;
        public AnimationViewModel SelectedAnimation
        {
            get { return selectedAnimation; }
            set
            {
                if (value == selectedAnimation)
                    return;

                selectedAnimation = value;
                NotifyOfPropertyChange(() => SelectedAnimation);
                NotifyOfPropertyChange(() => CanPlayAnimation);
            }
        }

        private AnimationFrameViewModel selectedFrame = null;
        public AnimationFrameViewModel SelectedFrame
        {
            get { return selectedFrame; }
            set
            {
                if(value == selectedFrame)
                    return;

                selectedFrame = value;
                NotifyOfPropertyChange(() => SelectedFrame);
                NotifyOfPropertyChange(() => CurrentAnimationFrameImage);
            }
        }

        public CroppedBitmap CurrentAnimationFrameImage
        {
            get
            {
                CroppedBitmap rtn = (SelectedFrame == null) ? defaultCroppedImage : SelectedFrame.FrameImage;
                return rtn;
            }
        }

        private Boolean isAnimationPlaying = false;
        public Boolean IsAnimationPlaying
        {
            get { return isAnimationPlaying; }
            set
            {
                if (value == isAnimationPlaying)
                    return;

                isAnimationPlaying = value;
                NotifyOfPropertyChange(() => IsAnimationPlaying);
                NotifyOfPropertyChange(() => FPSEnabled);
                NotifyOfPropertyChange(() => CanPlayAnimation);
                NotifyOfPropertyChange(() => CanStopAnimation);
            }
        }

        private String animationFPS = "30";
        public String AnimationFPS
        {
            get { return animationFPS; }
            set
            {
                if (value == animationFPS)
                    return;

                animationFPS = value;
                NotifyOfPropertyChange(() => AnimationFPS);
            }
        }

        public Boolean FPSEnabled
        {
            get { return (isAnimationPlaying == false); }
        }

        private Boolean loopAnimation = false;
        public Boolean LoopAnimation
        {
            get { return loopAnimation; }
            set
            {
                if (value == loopAnimation)
                    return;

                loopAnimation = value;
                NotifyOfPropertyChange(() => LoopAnimation);
            }
        }

        #endregion
    }
}
