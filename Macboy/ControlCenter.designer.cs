// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tomboy
{
	[Register ("ControlCenterController")]
	partial class ControlCenterController
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

	[Register ("ControlCenter")]
	partial class ControlCenter
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
