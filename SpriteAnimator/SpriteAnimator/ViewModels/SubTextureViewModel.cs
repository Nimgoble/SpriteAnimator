using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

using SpriteAnimator.Models;
using SpriteAnimator.Utils;

namespace SpriteAnimator.ViewModels
{
    public class SubTextureViewModel : Screen
    {
		private Rect bounds;
        public SubTextureViewModel(SubTexture subTexture)
        {
            this.Name = subTexture.Name;
			this.bounds = new Rect(subTexture.X, subTexture.Y, subTexture.Width, subTexture.Height);
        }

        public SubTexture ToSubTexture()
        {
            return new SubTexture()
            {
                Name = Name,
                Height = Height,
                Width = Width,
                X = X,
                Y = Y
            };
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
        public Int32 X
        {
            get { return bounds.X; }
            set
            {
                if (value == bounds.X)
                    return;

                bounds.X = value;
                NotifyOfPropertyChange(() => X);
            }
        }
        public Int32 Y
        {
            get { return bounds.Y; }
            set
            {
                if (value == bounds.Y)
                    return;

				bounds.Y = value;
                NotifyOfPropertyChange(() => Y);
            }
        }
        public Int32 Width
        {
            get { return bounds.Width; }
            set
            {
                if (value == bounds.Width)
                    return;

				bounds.Width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }
        public Int32 Height
        {
            get { return bounds.Height; }
            set
            {
                if (value == bounds.Height)
                    return;

				bounds.Height = value;
                NotifyOfPropertyChange(() => Height);
            }
        }

		public Rect Bounds
		{
			get { return bounds; }
			set
			{
				if (value == null)
					return;
				this.bounds = value;
				NotifyOfPropertyChange(() => Bounds);
			}
		}
        #endregion
    }
}
