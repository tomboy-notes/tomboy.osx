// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tomboy
{
	[Register ("DashboardWindowController")]
	partial class DashboardWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTableView _notesTableView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView _notebooksTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_notesTableView != null) {
				_notesTableView.Dispose ();
				_notesTableView = null;
			}

			if (_notebooksTableView != null) {
				_notebooksTableView.Dispose ();
				_notebooksTableView = null;
			}
		}
	}

	[Register ("DashboardWindow")]
	partial class DashboardWindow
	{
		[Outlet]
		MonoMac.AppKit.NSScrollView _NotesTableView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSScrollView _NotebooksTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_NotesTableView != null) {
				_NotesTableView.Dispose ();
				_NotesTableView = null;
			}

			if (_NotebooksTableView != null) {
				_NotebooksTableView.Dispose ();
				_NotebooksTableView = null;
			}
		}
	}
}
