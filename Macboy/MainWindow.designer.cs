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

		[Outlet]
		MonoMac.AppKit.NSTableView tblNotes { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableColumn tblColmnNoteTitle { get; set; }

		[Outlet]
		MonoMac.WebKit.WebView noteWebView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSearchField searchField { get; set; }

		[Outlet]
		Macboy.MainWindow mainWindow { get; set; }

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

			if (tblNotes != null) {
				tblNotes.Dispose ();
				tblNotes = null;
			}

			if (tblColmnNoteTitle != null) {
				tblColmnNoteTitle.Dispose ();
				tblColmnNoteTitle = null;
			}

			if (noteWebView != null) {
				noteWebView.Dispose ();
				noteWebView = null;
			}

			if (searchField != null) {
				searchField.Dispose ();
				searchField = null;
			}

			if (mainWindow != null) {
				mainWindow.Dispose ();
				mainWindow = null;
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
