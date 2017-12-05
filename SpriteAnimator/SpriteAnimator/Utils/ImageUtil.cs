using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace SpriteAnimator.Utils
{
	public class StitchImageArguments
	{
		public String Name { get; set; }
		public BitmapSource Source { get; set; }
		public Int32Rect SourceRect { get; set; }
		public Int32Rect DestinationRect { get; set; }
	}
    public static class ImageUtil
    {
		public static BitmapImage StitchImages(int width, int height, double dpiX, double dpiY, PixelFormat format, List<StitchImageArguments> arguments)
		{
			var maxHeight = (from arg in arguments select arg.DestinationRect.Height + arg.DestinationRect.Y).OrderByDescending(x => x).First();
			var maxWidth = (from arg in arguments select arg.DestinationRect.Width + arg.DestinationRect.X).OrderByDescending(x => x).First();
			var blankCanvas = new WriteableBitmap(maxWidth, maxHeight, dpiX, dpiY, format, null);
			foreach(var argument in arguments)
			{
				var image = argument.Source;
				if (!image.IsFrozen)
					image.Freeze();
				var stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
				var size = image.PixelHeight * stride;
				var buffer = new byte[size];
				image.CopyPixels(buffer, stride, 0);
				blankCanvas.WritePixels(new Int32Rect(0, 0, argument.SourceRect.Width, argument.SourceRect.Height), buffer, stride, argument.DestinationRect.X, argument.DestinationRect.Y);
			}
			blankCanvas.Freeze();
			return ConvertSourceToImage(blankCanvas, "png");
		}
		public static BitmapSource StitchBitmaps(BitmapSource b1, BitmapSource b2)
		{
			if (b1.Format != b2.Format)
			{
				throw new ArgumentException("All input bitmaps must have the same pixel format");
			}

			var width = Math.Max(b1.PixelWidth, b2.PixelWidth);
			var height = b1.PixelHeight + b2.PixelHeight; //Math.Max(, b3.PixelHeight);
			var wb = new WriteableBitmap(width, height, b1.DpiX, b1.DpiY, b1.Format, null);
			var stride1 = (b1.PixelWidth * b1.Format.BitsPerPixel + 7) / 8;
			var stride2 = (b2.PixelWidth * b2.Format.BitsPerPixel + 7) / 8;
			//var stride3 = (b3.PixelWidth * b3.Format.BitsPerPixel + 7) / 8;
			var size = b1.PixelHeight * stride1;
			size = Math.Max(size, b2.PixelHeight * stride2);
			//size = Math.Max(size, b3.PixelHeight * stride3);

			var buffer = new byte[size];
			b1.CopyPixels(buffer, stride1, 0);
			wb.WritePixels(
				new Int32Rect(0, 0, b1.PixelWidth, b1.PixelHeight),
				buffer, stride1, 0);

			b2.CopyPixels(buffer, stride2, 0);
			wb.WritePixels(
				new Int32Rect(0, b1.PixelHeight, b2.PixelWidth, b2.PixelHeight),
				buffer, stride2, 0);

			//b3.CopyPixels(buffer, stride3, 0);
			//wb.WritePixels(
			//	new Int32Rect(b2.PixelWidth, b1.PixelHeight, b3.PixelWidth, b3.PixelHeight),
			//	buffer, stride3, 0);

			return wb;
		}
		public static BitmapImage ConvertSourceToImage(BitmapSource source, string extension)
		{
			BitmapEncoder encoder;
			string lowerExtension = extension.ToLower();
			if (lowerExtension.Contains("jpeg") || lowerExtension.Contains("jpg"))
				encoder = new JpegBitmapEncoder();
			else if (lowerExtension.Contains("png"))
				encoder = new PngBitmapEncoder();
			else
				return null;

			MemoryStream memoryStream = new MemoryStream();
			BitmapImage bImg = new BitmapImage();

			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(memoryStream);

			memoryStream.Position = 0;
			bImg.BeginInit();
			bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
			bImg.EndInit();
			bImg.Freeze();
			//memoryStream.Close();
			return bImg;
		}

		public static void SaveImage(BitmapSource source, string fileName)
		{
			BitmapEncoder encoder;
			string lowerExtension = Path.GetExtension(fileName).ToLower();
			if (lowerExtension.Contains("jpeg") || lowerExtension.Contains("jpg"))
				encoder = new JpegBitmapEncoder();
			else if (lowerExtension.Contains("png"))
				encoder = new PngBitmapEncoder();
			else
				return;

			encoder.Frames.Add(BitmapFrame.Create(source));
			using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
			{
				encoder.Save(fileStream);
			}
		}
	}
}
