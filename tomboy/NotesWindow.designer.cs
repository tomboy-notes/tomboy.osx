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
	[Register ("NotesWindowController")]
	partial class NotesWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTableView _notebooksTableView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageCell _notesImage { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView _notesTableView { get; set; }

		[Action ("EditNotebook:")]
		partial void EditNotebook (MonoMac.Foundation.NSObject sender);

		[Action ("searchFieldFindNotes:")]
		partial void FindNotes (MonoMac.AppKit.NSSearchField sender);

		[Action ("NewNotebook:")]
		partial void NewNotebook (MonoMac.Foundation.NSObject sender);

		[Action ("newNotebookButton:")]
		partial void newNotebookButton (MonoMac.Foundation.NSObject sender);

		[Action ("_newNoteButton:")]
		partial void NewNoteClicked (MonoMac.Foundation.NSObject sender);

		[Action ("RemoveNotebook:")]
		partial void RemoveNotebook (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (_notebooksTableView != null) {
				_notebooksTableView.Dispose ();
				_notebooksTableView = null;
			}

			if (_notesImage != null) {
				_notesImage.Dispose ();
				_notesImage = null;
			}

			if (_notesTableView != null) {
				_notesTableView.Dispose ();
				_notesTableView = null;
			}
		}
	}

	[Register ("NotesWindow")]
	partial class NotesWindow
	{
		[Action ("RefreshButton:")]
		partial void RefreshButton (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
