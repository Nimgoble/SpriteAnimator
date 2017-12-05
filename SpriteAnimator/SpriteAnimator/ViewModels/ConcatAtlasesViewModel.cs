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

namespace SpriteAnimator.ViewModels
{
    public class ConcatAtlasesViewModel : Screen, IHandle<AtlasLoadedEvent>
	{
		#region Private Members
		private readonly IEventAggregator eventAggregator;
		private readonly IWindowManager windowManager;
		private BitmapImage defaultImage;
		private TextureAtlasViewModel otherAtlas;
		#endregion

		public ConcatAtlasesViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			this.eventAggregator = eventAggregator;
			this.windowManager = windowManager;
			defaultImage = new BitmapImage(new Uri(@"pack://application:,,,/Content/default.png"));
			this.eventAggregator.Subscribe(this);
			Algorithms = new List<MaxRectsBinPack.FreeRectChoiceHeuristic>();
			Algorithms.AddRange(new[] { MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule, MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule });
			GrowDirections = new List<MaxRectsBinPack.GrowDirection>();
			GrowDirections.AddRange(new[] { MaxRectsBinPack.GrowDirection.DoNotGrow, MaxRectsBinPack.GrowDirection.Horizontal, MaxRectsBinPack.GrowDirection.Vertical });
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
			var dst = preserveSourceOrder ? (from st in currentAtlas.SubTextures select st.Bounds).ToList() : null;
			MaxRectsBinPack packer = new MaxRectsBinPack((int)currentImage.PixelWidth, (int)currentImage.PixelHeight, false, dst);
			var listOfSubTexturesToChange = new List<Tuple<SubTextureViewModel, BitmapSource>>();
			if (!PreserveSourceOrder)
			{
				foreach(var st in currentAtlas.SubTextures)
					listOfSubTexturesToChange.Add(new Tuple<SubTextureViewModel, BitmapSource>(st, currentAtlas.Image));
			}

			foreach (var st in otherAtlas.SubTextures)
				listOfSubTexturesToChange.Add(new Tuple<SubTextureViewModel, BitmapSource>(st, otherAtlas.Image));
			var src = (from st in otherAtlas.SubTextures select st.Bounds).ToList();
			Dictionary<SubTextureViewModel, Rect> moveLocations = new Dictionary<SubTextureViewModel, Rect>();
			var arguments = new List<StitchImageArguments>();
			foreach (var item in listOfSubTexturesToChange)
			{
				var st = item.Item1;
				var result = packer.Insert(st.Bounds.Width, st.Bounds.Height, SelectedAlgorithm, selectedGrowDirection);
				if (result == null)
					continue;
				var argument = new StitchImageArguments()
				{
					Name = st.Name,
					Source = new CroppedBitmap(item.Item2, st.Bounds.ToInt32Rect()),
					SourceRect = st.Bounds.ToInt32Rect(),
					DestinationRect = result.ToInt32Rect()
				};
				arguments.Add(argument);
			}
			if(PreserveSourceOrder)
			{
				var currentImageRect = new System.Windows.Int32Rect(0, 0, currentImage.PixelWidth, currentImage.PixelHeight);
				arguments.Add(new StitchImageArguments() { Source = currentImage, DestinationRect = currentImageRect, SourceRect = currentImageRect });
			}
			var wiggleRoom = 1000;
			var newResult = ImageUtil.StitchImages(packer.binWidth + wiggleRoom, packer.binHeight + wiggleRoom, currentImage.DpiX, currentImage.DpiY, currentImage.Format, arguments);
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

		public List<MaxRectsBinPack.FreeRectChoiceHeuristic> Algorithms { get; set; }
		private MaxRectsBinPack.FreeRectChoiceHeuristic selectedAlgorithm = MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit;
		public MaxRectsBinPack.FreeRectChoiceHeuristic SelectedAlgorithm
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

		public List<MaxRectsBinPack.GrowDirection> GrowDirections { get; set; }
		private MaxRectsBinPack.GrowDirection selectedGrowDirection = MaxRectsBinPack.GrowDirection.DoNotGrow;
		public MaxRectsBinPack.GrowDirection SelectedGrowDirection
		{
			get { return selectedGrowDirection; }
			set
			{
				if (value == selectedGrowDirection)
					return;
				selectedGrowDirection = value;
				NotifyOfPropertyChange(() => SelectedGrowDirection);
				ProcessAtlases();
			}
		}

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

