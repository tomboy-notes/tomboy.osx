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
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	[Register ("InsensitiveTextView")]
	public class InsensitiveTextView : NSTextView
	{
		public InsensitiveTextView (IntPtr handle) : base (handle) {
		}

		[Export ("initWithCoder:")]
		public InsensitiveTextView (NSCoder coder) : base (coder) {
		}

		public override void MouseDown (NSEvent theEvent) {
			Superview.MouseDown (theEvent);
		}

		public override void ScrollWheel (NSEvent theEvent) {
			Superview.ScrollWheel (theEvent);
		}
	}

	public partial class NoteCollectionViewItem : MonoMac.AppKit.NSView{
		bool selected;

		public NoteCollectionViewItem (IntPtr handle) : base (handle) {
			Initialize ();
		}

		[Export ("initWithCoder:")]
		public NoteCollectionViewItem (NSCoder coder) : base (coder) {
			Initialize ();
		}

		void Initialize () {
		}

		public NSTextField TextField {
			get {
				return textField;
			}
		}

		public NSTextView ContentTextView {
			get {
				return contentTextView;
			}
		}

		public bool Selected {
			get {
				return selected;
			}
			set {
				if (value == selected)
					return;
				selected = value;
				NeedsDisplay = true;
				Console.WriteLine ("Selection changed");
			}
		}

		public override void DrawRect (System.Drawing.RectangleF dirtyRect) {
			dirtyRect.Inflate (-1, -1);
			if (selected) {
				var color = NSColor.SelectedControl;
				color.Set ();
				NSGraphics.RectFill (dirtyRect);
			}
		}
	}
}