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
        private OpenSetViewModel openSetViewModel;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        public ShellViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            this.eventAggregator = eventAggregator;
            this.windowManager = windowManager;
            DisplayName = "Sprite Animator";
            openSetViewModel = new OpenSetViewModel(eventAggregator, windowManager);
            mainViewModel = new MainViewModel(eventAggregator);
            imageBlobsViewModel = new ImageBlobsViewModel(eventAggregator);
            this.ActivateItem(mainViewModel);
        }

        #region Methods
        public void SelectOpenSet()
        {
            this.ActivateItem(openSetViewModel);
        }
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
