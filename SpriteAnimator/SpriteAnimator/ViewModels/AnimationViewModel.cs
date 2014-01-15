using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Caliburn.Micro;

using SpriteAnimator.Models;

namespace SpriteAnimator.ViewModels
{
    public class AnimationViewModel : Screen
    {
        public AnimationViewModel(String name, List<SubTexture> subTextures, BitmapImage image)
        {
            this.Name = name;
            foreach (SubTexture subTexture in subTextures)
                frames.Add(new AnimationFrameViewModel(subTexture, image));

            frames.OrderBy(x => x.Index);
        }

        #region Properties
        //The name of this animation
        private String name = String.Empty;
        public String Name
        {
            get { return name; }
            set
            {
                if (value == name)
                    return;

                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        //The frames in this animation
        private ObservableCollection<AnimationFrameViewModel> frames = new ObservableCollection<AnimationFrameViewModel>();
        public ObservableCollection<AnimationFrameViewModel> Frames
        {
            get { return frames; }
        }

        public Int32 FrameCount
        {
            get { return frames.Count; }
        }
        #endregion
    }
}
