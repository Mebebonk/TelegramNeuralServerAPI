using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Emgu.CV.Dai.OpenVino;

namespace TelegramNeuralServerAPI
{
	internal readonly struct ProcessAssistant
	{
		public readonly Coordinate topLeft;
		public readonly Coordinate bottomRight;

		public readonly int width;
		public readonly int height;

		public readonly int thickness;
		public readonly int borderThickness;

		public readonly int textX;
		public readonly int textY;
		public readonly Point textPoint;

		public ProcessAssistant(Coordinate topLeft, Coordinate bottomRight)
		{
			this.topLeft = topLeft; this.bottomRight = bottomRight;

			width = Math.Abs(topLeft.x - bottomRight.x);
			height = Math.Abs(topLeft.y - bottomRight.y);

			thickness = (int)Math.Ceiling(Math.Min(width, height) * 0.01);
			borderThickness = (int)Math.Ceiling(thickness * 1.5);

			textX = topLeft.x + thickness * 2;
			textY = bottomRight.y - thickness * 2;
			textPoint = new(textX, textY);
		}

		public ProcessAssistant(int tlx, int tly, int brx, int bry) : this(new(tlx, tly), new(brx, bry)) { }
		public ProcessAssistant(BoundingBox box) : this(box.topLeft, box.bottomRight) { }
	}
}
