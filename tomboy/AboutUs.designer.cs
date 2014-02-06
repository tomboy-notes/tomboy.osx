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
	[Register ("AboutUsController")]
	partial class AboutUsController
	{
		[Outlet]
		MonoMac.AppKit.NSButtonCell GoToTomboyWebsite { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (GoToTomboyWebsite != null) {
				GoToTomboyWebsite.Dispose ();
				GoToTomboyWebsite = null;
			}
		}
	}

	[Register ("AboutUs")]
	partial class AboutUs
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
