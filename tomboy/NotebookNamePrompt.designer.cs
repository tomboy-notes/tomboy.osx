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
	[Register ("NotebookNamePromptController")]
	partial class NotebookNamePromptController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField NotebookName { get; set; }

		[Action ("AddNotebook:")]
		partial void AddNotebook (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (NotebookName != null) {
				NotebookName.Dispose ();
				NotebookName = null;
			}
		}
	}

	[Register ("NotebookNamePrompt")]
	partial class NotebookNamePrompt
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
