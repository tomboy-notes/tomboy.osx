using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MacSuperBoy
{
	[Register ("StyleBar")]
	public class StyleBar : NSView
	{
		public StyleBar () : base ()
		{
			Initialize ();
		}

		public StyleBar (IntPtr ptr) : base (ptr)
		{
			Initialize ();
		}

		void Initialize ()
		{

		}

		public override void DrawRect (System.Drawing.RectangleF dirtyRect)
		{
			var bounds = Bounds;
			if (dirtyRect.Height < bounds.Height)
				return;
			var bottomBorder = new RectangleF (dirtyRect.X,
			                                   dirtyRect.Y,
			                                   dirtyRect.Width,
			                                   dirtyRect.Y + .2f);
			NSColor.Black.Set ();
			var path = NSBezierPath.FromRect (bottomBorder);
			path.Fill ();
		}
	}
}

