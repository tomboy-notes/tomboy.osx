// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tomboy
{
	[Register ("NoteCollectionItem")]
	partial class NoteCollectionItem
	{
		[Outlet]
		MonoMac.AppKit.NSImageView imageView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField titleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imageView != null) {
				imageView.Dispose ();
				imageView = null;
			}

			if (titleLabel != null) {
				titleLabel.Dispose ();
				titleLabel = null;
			}
		}
	}
}
