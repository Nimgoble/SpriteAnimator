using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Xml.Serialization;

using Caliburn.Micro;
using SpriteAnimator.Models;
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
        private List<ITexturePacker> texturePackers = new List<ITexturePacker>();
		#endregion

		public ConcatAtlasesViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
		{
			this.eventAggregator = eventAggregator;
			this.windowManager = windowManager;
			defaultImage = new BitmapImage(new Uri(@"pack://application:,,,/Content/default.png"));
			this.eventAggregator.Subscribe(this);
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

			OtherAtlas = loadVM.LoadFile(file);
			ProcessAtlases();
		}
		
		public bool CanLoadAtlasToConcat { get { return this.CurrentAtlas != null; } }

		public void SaveResult()
		{
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "(*.png, *.xml)|*.png;*.xml";

            if(saveFileDialog.ShowDialog() == true)
            {
                var filePath = Path.GetDirectoryName(saveFileDialog.FileName);
                var nakedFileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                var atlasFilePath = Path.Combine(filePath, nakedFileName + ".xml");
                var imageFileName = Path.Combine(nakedFileName + ".png");
                var imageFilePath = Path.Combine(filePath, imageFileName);
                resultAtlas.ImagePath = imageFileName;

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                using (FileStream stream = new FileStream(atlasFilePath, FileMode.OpenOrCreate))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TextureAtlas));
                    serializer.Serialize(stream, resultAtlas.ToTextureAtlas(), ns);
                }
                ImageUtil.SaveImage(resultAtlas.Image, imageFilePath);
            }
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
            var newResultAtlas = new TextureAtlasViewModel();
            arguments.OrderBy(x => x.Name).ToList().ForEach
            (
                x =>
                {
                    var subTexture = new SubTexture()
                    {
                        Height = x.DestinationRect.Height,
                        Width = x.DestinationRect.Width,
                        X = x.DestinationRect.X,
                        Y = x.DestinationRect.Y,
                        Name = x.Name
                    };
                    newResultAtlas.SubTextures.Add(new SubTextureViewModel(subTexture));
                }
            );
            newResultAtlas.Image = newResult;
            ResultAtlas = newResultAtlas;
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

        private TextureAtlasViewModel otherAtlas = null;
        public TextureAtlasViewModel OtherAtlas
        {
            get { return otherAtlas; }
            set
            {
                if (value == otherAtlas)
                    return;
                otherAtlas = value;
                OtherImage = (otherAtlas != null) ? otherAtlas.Image : defaultImage;
                NotifyOfPropertyChange(() => OtherAtlas);
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

        private TextureAtlasViewModel resultAtlas = null;
        public TextureAtlasViewModel ResultAtlas
        {
            get { return resultAtlas; }
            set
            {
                if (value == resultAtlas)
                    return;
                resultAtlas = value;
                ResultImage = (resultAtlas != null) ? (resultAtlas.Image ?? defaultImage) : defaultImage;
                NotifyOfPropertyChange(() => ResultAtlas);
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

        #endregion
    }
}

