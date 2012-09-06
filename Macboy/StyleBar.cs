// Author:
//       Jared Jennings <jjennings@gnome.org>
//       Jérémie Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2012 Jared Jennings 2012
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
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

