using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class ExtendedImage : Image<Rgb, byte>
	{
		public ImageInfo ImageInfo { get; private set; }

		public ExtendedImage(byte[,,] data, ImageInfo imageInfo) : base(data)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(Mat mat, ImageInfo imageInfo) : base(mat)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(string fileName, ImageInfo imageInfo) : base(fileName)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(Size size, ImageInfo imageInfo) : base(size)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(Image<Gray, byte>[] channels, ImageInfo imageInfo) : base(channels)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(int width, int height, ImageInfo imageInfo) : base(width, height)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(SerializationInfo info, StreamingContext context, ImageInfo imageInfo) : base(info, context)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(int width, int height, Rgb value, ImageInfo imageInfo) : base(width, height, value)
		{
			this.ImageInfo = imageInfo;
		}

		public ExtendedImage(int width, int height, int stride, nint scan0, ImageInfo imageInfo) : base(width, height, stride, scan0)
		{
			this.ImageInfo = imageInfo;
		}

		protected ExtendedImage()
		{
			this.ImageInfo = new("");
		}
	}
}
