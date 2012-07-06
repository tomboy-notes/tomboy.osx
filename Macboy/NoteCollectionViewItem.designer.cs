// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace MacSuperBoy
{
	[Register ("NoteCollectionViewItem")]
	partial class NoteCollectionViewItem
	{
		[Outlet]
		MonoMac.AppKit.NSTextField textField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextView contentTextView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (textField != null) {
				textField.Dispose ();
				textField = null;
			}

			if (contentTextView != null) {
				contentTextView.Dispose ();
				contentTextView = null;
			}
		}
	}

	[Register ("NoteCollectionViewItemController")]
	partial class NoteCollectionViewItemController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
