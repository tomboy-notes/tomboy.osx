// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

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
