using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

using SpriteAnimator.Models;

namespace SpriteAnimator.ViewModels
{
    public class SubTextureViewModel : Screen
    {
        public SubTextureViewModel(SubTexture subTexture)
        {
            this.Name = subTexture.Name;
            this.X = subTexture.X;
            this.Y = subTexture.Y;
            this.Width = subTexture.Width;
            this.Height = subTexture.Height;
        }

        #region Properties
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
        private Int32 x = -1;
        public Int32 X
        {
            get { return x; }
            set
            {
                if (value == x)
                    return;

                x = value;
                NotifyOfPropertyChange(() => X);
            }
        }

        private Int32 y = -1;
        public Int32 Y
        {
            get { return y; }
            set
            {
                if (value == y)
                    return;

                y = value;
                NotifyOfPropertyChange(() => Y);
            }
        }

        private Int32 width = -1;
        public Int32 Width
        {
            get { return width; }
            set
            {
                if (value == width)
                    return;

                width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        private Int32 height = -1;
        public Int32 Height
        {
            get { return height; }
            set
            {
                if (value == height)
                    return;

                height = value;
                NotifyOfPropertyChange(() => Height);
            }
        }
        #endregion
    }
}
