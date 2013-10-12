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
	[Register ("MyDocument")]
	partial class MyDocument
	{
		[Outlet]
		MonoMac.AppKit.NSMenuItem _dockSynchronize { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSegmentedControl backForwardControl { get; set; }

		[Outlet]
		MonoMac.AppKit.NSWindow myNoteWindow { get; set; }

		[Outlet]
		MonoMac.WebKit.WebView noteWebView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSearchField searchField { get; set; }

		[Action ("_dockNewNote:")]
		partial void _dockNewNote (MonoMac.Foundation.NSObject sender);

		[Action ("AllNotes:")]
		partial void AllNotes (MonoMac.Foundation.NSObject sender);

		[Action ("BackForwardAction:")]
		partial void BackForwardAction (MonoMac.AppKit.NSSegmentedControl sender);

		[Action ("DeleteNote:")]
		partial void DeleteNote (MonoMac.Foundation.NSObject sender);

		[Action ("ShowNotes:")]
		partial void ShowNotes (MonoMac.Foundation.NSObject sender);

		[Action ("StartSearch:")]
		partial void StartSearch (MonoMac.AppKit.NSSearchField sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (_dockSynchronize != null) {
				_dockSynchronize.Dispose ();
				_dockSynchronize = null;
			}

			if (backForwardControl != null) {
				backForwardControl.Dispose ();
				backForwardControl = null;
			}

			if (noteWebView != null) {
				noteWebView.Dispose ();
				noteWebView = null;
			}

			if (searchField != null) {
				searchField.Dispose ();
				searchField = null;
			}

			if (myNoteWindow != null) {
				myNoteWindow.Dispose ();
				myNoteWindow = null;
			}
		}
	}
}
