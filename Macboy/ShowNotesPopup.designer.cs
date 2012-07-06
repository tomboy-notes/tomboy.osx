// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace MacSuperBoy
{
	[Register ("ShowNotesPopup")]
	partial class ShowNotesPopup
	{
		[Outlet]
		MacSuperBoy.NoteCollectionView collectionView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSlider collectionNumberSlider { get; set; }

		[Action ("ColumnNumberSliderChange:")]
		partial void ColumnNumberSliderChange (MonoMac.AppKit.NSSlider sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (collectionView != null) {
				collectionView.Dispose ();
				collectionView = null;
			}

			if (collectionNumberSlider != null) {
				collectionNumberSlider.Dispose ();
				collectionNumberSlider = null;
			}
		}
	}

	[Register ("ShowNotesPopupController")]
	partial class ShowNotesPopupController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
