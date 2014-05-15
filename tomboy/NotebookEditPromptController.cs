//
// NotebookEditPromptController.cs
//
// Author:
//       Rashid Khan <rashood.khan@gmail.com>
//
// Copyright (c) 2014 Rashid Khan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class NotebookEditPromptController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public NotebookEditPromptController (IntPtr handle) : base (handle) {
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public NotebookEditPromptController (NSCoder coder) : base (coder) {
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public NotebookEditPromptController () : base ("NotebookEditPrompt") {
			Initialize ();
		}
		// Shared initialization code
		void Initialize () {

		}

		[Export ("awakeFromNib:")]
		public override void AwakeFromNib() {
			if (AppDelegate.currentNotebook != null) {
				if (AppDelegate.currentNotebook.Equals ("All Notebooks", StringComparison.Ordinal))
					NotebookNameTextField.Editable = false;
				NotebookNameTextField.StringValue = AppDelegate.currentNotebook;
			}
		}

		#endregion

		partial void UpdateNotebook ( NSObject sender) {
			string updatedName = NotebookNameTextField.StringValue;
			if(updatedName.Equals ("All Notebooks", StringComparison.Ordinal))
				this.Window.Close ();
			else {
				if (AppDelegate.Notebooks.Contains(updatedName)) {
					NSAlert alert = new NSAlert () {
						MessageText = "Notebook Already Exists",
						InformativeText = "The Notebook " + updatedName+ " already exists.",
						AlertStyle = NSAlertStyle.Warning
					};
					alert.AddButton ("OK");
					alert.BeginSheet (this.Window,
						this,
						null,
						IntPtr.Zero);

					NotebookNameTextField.SelectText(this);
				}
				else {
					string oldName = AppDelegate.currentNotebook;
					AppDelegate.Notebooks.Remove (oldName);
					Dictionary<string, Note> allNotes = AppDelegate.NoteEngine.GetNotesForNotebook (oldName);

					foreach(KeyValuePair<string, Note> note in allNotes) {
						note.Value.Notebook = updatedName;
						AppDelegate.NoteEngine.SaveNote (note.Value);
					}


					AppDelegate.currentNotebook = updatedName;
					AppDelegate.Notebooks.Add (updatedName);
					AppDelegate.RefreshNotesWindowController ();

					this.Window.Close ();
				}

			}
		}

		partial void CancelButton (NSObject sender) {
			this.Window.Close ();
		}

		//strongly typed window accessor
		public new NotebookEditPrompt Window {
			get {
				return (NotebookEditPrompt)base.Window;
			}
		}
	}
}

