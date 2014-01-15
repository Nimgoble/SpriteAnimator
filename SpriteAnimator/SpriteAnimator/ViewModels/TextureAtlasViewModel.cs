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
    public class TextureAtlasViewModel : Screen
    {
        public TextureAtlasViewModel()
        {
        }

        public TextureAtlasViewModel(TextureAtlas textureAtlas, String imagePath)
        {
            this.ImagePath = imagePath;

            image = new BitmapImage(new Uri(imagePath));

            List<String> animationNames = (from subTexture in textureAtlas.SubTextures select subTexture.Name.Substring(0, subTexture.Name.Length - 4)).ToList();
            HashSet<String> uniqueAnimationNames = new HashSet<String>(animationNames);
            foreach (String animationName in uniqueAnimationNames)
            {
                animations.Add
                (
                    new AnimationViewModel
                    (
                        animationName, 
                        (from subTexture in textureAtlas.SubTextures where (subTexture.Name.Substring(0, subTexture.Name.Length - 4) == animationName) select subTexture).ToList(),
                        image
                    )
                );
            }
        }

        #region Properties
        private ObservableCollection<AnimationViewModel> animations = new ObservableCollection<AnimationViewModel>();
        public ObservableCollection<AnimationViewModel> Animations
        {
            get { return animations; }
        }

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
            }
        }

        private BitmapImage image = null;
        public BitmapImage Image
        {
            get { return image; }
        }
        #endregion
    }
}
