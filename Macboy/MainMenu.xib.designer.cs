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
	[Register ("AppDelegate")]
	partial class AppDelegate
	{
		[Outlet]
		MonoMac.AppKit.NSMenu dockMenu { get; set; }

		[Action ("deleteNote:")]
		partial void DeleteNote (MonoMac.Foundation.NSObject sender);

		[Action ("aboutTomboy:")]
		partial void MenuClickedAboutTomboy (MonoMac.Foundation.NSObject sender);

		[Action ("menuNewNote:")]
		partial void MenuClickedNewNote (MonoMac.Foundation.NSObject sender);

		[Action ("menuSearchNotes:")]
		partial void MenuClickedSearchNotes (MonoMac.Foundation.NSObject sender);

		[Action ("OpenDashboard:")]
		partial void OpenDashboard (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (dockMenu != null) {
				dockMenu.Dispose ();
				dockMenu = null;
			}
		}
	}
}
