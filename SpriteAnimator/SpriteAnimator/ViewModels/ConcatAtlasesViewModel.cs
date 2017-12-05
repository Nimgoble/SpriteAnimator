using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AForge.Imaging;

using Caliburn.Micro;
using SpriteAnimator.Events;
using SpriteAnimator.Utils;
using SpriteAnimator.Wrappers;

namespace SpriteAnimator.ViewModels
{
    public class ConcatAtlasesViewModel : Screen, IHandle<AtlasLoadedEvent>
	{
		#region Private Members
		private readonly IEventAggregator eventAggregator;
		private readonly IWindowManager windowManager;
		private BitmapImage defaultImage;
		private TextureAtlasViewModel otherAtlas;
        private List<ITexturePacker> texturePackers = new List<ITexturePacker>();
		#endregion

		public ConcatAtlasesViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			this.eventAggregator = eventAggregator;
			this.windowManager = windowManager;
			defaultImage = new BitmapImage(new Uri(@"pack://application:,,,/Content/default.png"));
			this.eventAggregator.Subscribe(this);
			//Algorithms = new List<MaxRectsBinPack.FreeRectChoiceHeuristic>();
			//Algorithms.AddRange(new[] { MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule, MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule });
			//GrowDirections = new List<MaxRectsBinPack.GrowDirection>();
			//GrowDirections.AddRange(new[] { MaxRectsBinPack.GrowDirection.DoNotGrow, MaxRectsBinPack.GrowDirection.Horizontal, MaxRectsBinPack.GrowDirection.Vertical });
            texturePackers.Add(new MaxRectsBinTexturePacker());
            texturePackers.Add(new ChevyRayTexturePacker());
            SelectedTexturePacker = texturePackers.First();
        }

		#region Methods
		public void LoadAtlasToConcat()
		{
			var loadVM = new OpenSetViewModel(eventAggregator, windowManager);
			var file = loadVM.PromptChooseFile();
			if (string.IsNullOrEmpty(file))
				return;

			otherAtlas = loadVM.LoadFile(file);
			OtherImage = otherAtlas.Image;
			ProcessAtlases();
		}
		
		public bool CanLoadAtlasToConcat { get { return this.CurrentAtlas != null; } }

		public void SaveResult()
		{

		}

		private void ProcessAtlases()
		{
            if (selectedTexturePacker == null || currentAtlas == null || otherAtlas == null)
                return;
            selectedTexturePacker.Reset();
            selectedTexturePacker.SetPackingMethod(this.selectedAlgorithm);
            resultRectangles.Clear();
			var listOfSubTexturesToChange = new List<Tuple<SubTextureViewModel, BitmapSource>>();
			if (!PreserveSourceOrder)
			{
				foreach(var st in currentAtlas.SubTextures)
					listOfSubTexturesToChange.Add(new Tuple<SubTextureViewModel, BitmapSource>(st, currentAtlas.Image));
			}
			foreach (var st in otherAtlas.SubTextures)
				listOfSubTexturesToChange.Add(new Tuple<SubTextureViewModel, BitmapSource>(st, otherAtlas.Image));

			var arguments = new List<StitchImageArguments>();
            if(PreserveSourceOrder)
            {
                var currentImageRect = new Rect(0, 0, currentImage.PixelWidth, currentImage.PixelHeight);
                var result = selectedTexturePacker.Pack(currentImageRect);
                resultRectangles.Add(result);
                arguments.Add(new StitchImageArguments() { Source = currentImage, DestinationRect = result.ToInt32Rect(), SourceRect = currentImageRect.ToInt32Rect() });
            }

            var ordered = (orderByAreaDescending) ? listOfSubTexturesToChange.OrderByDescending(x => x.Item1.Bounds.Area).ToList() : listOfSubTexturesToChange.OrderBy(x => x.Item1.Bounds.Area).ToList();

            if (!ConcatAllAtOnce)
            {
                foreach (var item in ordered)
                {
                    var st = item.Item1;
                    var result = selectedTexturePacker.Pack(st.Bounds);
                    if (result == null)
                        continue;
                    resultRectangles.Add(result);
                    var argument = new StitchImageArguments()
                    {
                        Name = st.Name,
                        Source = new CroppedBitmap(item.Item2, st.Bounds.ToInt32Rect()),
                        SourceRect = st.Bounds.ToInt32Rect(),
                        DestinationRect = result.ToInt32Rect()
                    };
                    arguments.Add(argument);
                }
            }
            else
            {
                var results = selectedTexturePacker.PackAll(ordered.Select(x => x.Item1.Bounds)).ToList();
                for (int i = 0; i < ordered.Count; ++i)
                {
                    var item = ordered[i];
                    var st = item.Item1;
                    var result = results[i];
                    if (result == null)
                        continue;
                    resultRectangles.Add(result);
                    var argument = new StitchImageArguments()
                    {
                        Name = st.Name,
                        Source = new CroppedBitmap(item.Item2, st.Bounds.ToInt32Rect()),
                        SourceRect = st.Bounds.ToInt32Rect(),
                        DestinationRect = result.ToInt32Rect()
                    };
                    arguments.Add(argument);
                }
            }
            
            var newResult = ImageUtil.StitchImages(currentImage.DpiX, currentImage.DpiY, currentImage.Format, arguments);
			ResultImage = newResult;
		}
		#endregion

		#region IHandle
		public void Handle(AtlasLoadedEvent ev)
		{
			CurrentAtlas = ev.Atlas;
		}
		#endregion

		#region Properties
		private TextureAtlasViewModel currentAtlas = null;
		public TextureAtlasViewModel CurrentAtlas
		{
			get { return currentAtlas; }
			set
			{
				if (value == currentAtlas)
					return;

				currentAtlas = value;
				NotifyOfPropertyChange(() => CurrentAtlas);
				CurrentImage = (currentAtlas != null) ? currentAtlas.Image : defaultImage;
				NotifyOfPropertyChange(() => CanLoadAtlasToConcat);
			}
		}

		private BitmapImage currentImage = null;
		public BitmapImage CurrentImage
		{
			get { return currentImage; }
			set
			{
				if (value == currentImage)
					return;

				currentImage = value;
				NotifyOfPropertyChange(() => CurrentImage);
				ResultImage = currentImage.Clone();
			}
		}

		private BitmapImage otherImage = null;
		public BitmapImage OtherImage
		{
			get { return otherImage; }
			set
			{
				if (value == otherImage)
					return;

				otherImage = value;
				NotifyOfPropertyChange(() => OtherImage);
			}
		}

		private BitmapImage resultImage = null;
		public BitmapImage ResultImage
		{
			get { return resultImage; }
			set
			{
				if (value == resultImage)
					return;
				resultImage = value;
				NotifyOfPropertyChange(() => ResultImage);
			}
		}

        private List<PackingMethod> defaultAlgorithms = new List<PackingMethod>();
        public List<PackingMethod> Algorithms
        {
            get
            {
                return selectedTexturePacker == null ? defaultAlgorithms : selectedTexturePacker.AvailablePackingMethods.ToList();
            }
        }
		private PackingMethod selectedAlgorithm = PackingMethod.BestAreaFit;
		public PackingMethod SelectedAlgorithm
		{
			get { return selectedAlgorithm; }
			set
			{
				if (value == selectedAlgorithm)
					return;
				selectedAlgorithm = value;
				NotifyOfPropertyChange(() => SelectedAlgorithm);
				ProcessAtlases();
			}
		}
        public bool HasPackingAlgorithms { get { return selectedTexturePacker != null && selectedTexturePacker.AvailablePackingMethods.Any(); } }

		private bool preserveSourceOrder = true;
		public bool PreserveSourceOrder
		{
			get { return preserveSourceOrder; }
			set
			{
				if (value == preserveSourceOrder)
					return;
				preserveSourceOrder = value;
				NotifyOfPropertyChange(() => PreserveSourceOrder);
				ProcessAtlases();
			}
		}

        private bool concatAllAtOnce = false;
        public bool ConcatAllAtOnce
        {
            get { return concatAllAtOnce; }
            set
            {
                if (value == concatAllAtOnce)
                    return;
                concatAllAtOnce = value;
                NotifyOfPropertyChange(() => ConcatAllAtOnce);
                ProcessAtlases();
            }
        }

        private bool orderByAreaDescending = false;
        public bool OrderByAreaDescending
        {
            get { return orderByAreaDescending; }
            set
            {
                if (value == orderByAreaDescending)
                    return;
                orderByAreaDescending = value;
                NotifyOfPropertyChange(() => OrderByAreaDescending);
                ProcessAtlases();
            }
        }

        //public List<MaxRectsBinPack.GrowDirection> GrowDirections { get; set; }
        //private MaxRectsBinPack.GrowDirection selectedGrowDirection = MaxRectsBinPack.GrowDirection.DoNotGrow;
        //public MaxRectsBinPack.GrowDirection SelectedGrowDirection
        //{
        //	get { return selectedGrowDirection; }
        //	set
        //	{
        //		if (value == selectedGrowDirection)
        //			return;
        //		selectedGrowDirection = value;
        //		NotifyOfPropertyChange(() => SelectedGrowDirection);
        //		ProcessAtlases();
        //	}
        //}

        public List<ITexturePacker> TexturePackers { get { return texturePackers; } }
        private ITexturePacker selectedTexturePacker;
        public ITexturePacker SelectedTexturePacker
        {
            get { return selectedTexturePacker; }
            set
            {
                if (value == selectedTexturePacker)
                    return;
                selectedTexturePacker = value;
                NotifyOfPropertyChange(() => SelectedTexturePacker);
                NotifyOfPropertyChange(() => Algorithms);
                NotifyOfPropertyChange(() => HasPackingAlgorithms);
                SelectedAlgorithm = PackingMethod.BestAreaFit;
                ProcessAtlases();
            }
        }

        private ObservableCollection<Rect> resultRectangles = new ObservableCollection<Rect>();
        public ObservableCollection<Rect> ResultRectangles { get { return resultRectangles; } }

        //private ObservableCollection<Blob> blobs = new ObservableCollection<Blob>();
        //public ObservableCollection<Blob> Blobs
        //{
        //	get { return blobs; }
        //	set
        //	{
        //		if (value == blobs)
        //			return;

        //		blobs = value;
        //		NotifyOfPropertyChange(() => Blobs);
        //	}
        //}

        //private ObservableCollection<Blob> selectedBlobs = new ObservableCollection<Blob>();
        //public ObservableCollection<Blob> SelectedBlobs
        //{
        //	get { return selectedBlobs; }
        //	set
        //	{
        //		if (value == selectedBlobs)
        //			return;

        //		selectedBlobs = value;
        //		NotifyOfPropertyChange(() => SelectedBlobs);
        //	}
        //}

        #endregion
    }
}

