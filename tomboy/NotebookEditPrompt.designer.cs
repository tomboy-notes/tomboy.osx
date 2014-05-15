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
	[Register ("NotebookEditPromptController")]
	partial class NotebookEditPromptController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField NotebookNameTextField { get; set; }

		[Action ("CancelButton:")]
		partial void CancelButton (MonoMac.Foundation.NSObject sender);

		[Action ("UpdateNotebook:")]
		partial void UpdateNotebook (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (NotebookNameTextField != null) {
				NotebookNameTextField.Dispose ();
				NotebookNameTextField = null;
			}
		}
	}

	[Register ("NotebookEditPrompt")]
	partial class NotebookEditPrompt
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
