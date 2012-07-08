
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
		public InsensitiveTextView (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public InsensitiveTextView (NSCoder coder) : base (coder)
		{
		}

		public override void MouseDown (NSEvent theEvent)
		{
			Superview.MouseDown (theEvent);
		}

		public override void ScrollWheel (NSEvent theEvent)
		{
			Superview.ScrollWheel (theEvent);
		}
	}

	public partial class NoteCollectionViewItem : MonoMac.AppKit.NSView
	{
		bool selected;

		public NoteCollectionViewItem (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		[Export ("initWithCoder:")]
		public NoteCollectionViewItem (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		void Initialize ()
		{
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

		public override void DrawRect (System.Drawing.RectangleF dirtyRect)
		{
			dirtyRect.Inflate (-1, -1);
			if (selected) {
				var color = NSColor.SelectedControl;
				color.Set ();
				NSGraphics.RectFill (dirtyRect);
			}
		}
	}
}

