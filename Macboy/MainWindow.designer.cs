// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Macboy
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton NewNoteOutlet { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField MyTextField { get; set; }

		[Action ("NewNoteButton_Clicked:")]
		partial void NewNoteButton_Clicked (MonoMac.AppKit.NSButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (NewNoteOutlet != null) {
				NewNoteOutlet.Dispose ();
				NewNoteOutlet = null;
			}

			if (MyTextField != null) {
				MyTextField.Dispose ();
				MyTextField = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
