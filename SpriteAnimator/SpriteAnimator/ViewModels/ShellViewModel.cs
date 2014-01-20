using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

namespace SpriteAnimator.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private MainViewModel mainViewModel;
        private ImageBlobsViewModel imageBlobsViewModel;
        private readonly IEventAggregator eventAggregator;
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DisplayName = "Sprite Animator";
            mainViewModel = new MainViewModel(eventAggregator);
            imageBlobsViewModel = new ImageBlobsViewModel(eventAggregator);
            this.ActivateItem(mainViewModel);

        }

        #region Methods
        public void SelectMain()
        {
            this.ActivateItem(mainViewModel);
        }

        public void SelectImageBlobs()
        {
            this.ActivateItem(imageBlobsViewModel);
        }
        #endregion
    }
}
