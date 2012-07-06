
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MacSuperBoy
{
	public partial class ShowNotesPopup : MonoMac.AppKit.NSView
	{
		public ShowNotesPopup (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public ShowNotesPopup (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		void Initialize ()
		{
		}

		partial void ColumnNumberSliderChange (MonoMac.AppKit.NSSlider sender)
		{
			Console.WriteLine ("SliderChange {0}", sender.IntValue);
			((NoteCollectionView)CollectionView).UpdateColumnCountBasedOnNumber (sender.IntValue);
		}

		public NoteCollectionView CollectionView {
			get {
				return collectionView;
			}
		}
	}
}

