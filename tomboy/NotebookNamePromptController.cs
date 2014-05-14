//
// NotebookNamePromptController.cs
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
	public partial class NotebookNamePromptController : MonoMac.AppKit.NSWindowController
    	{
        	#region Constructors

        	// Called when created from unmanaged code
		public NotebookNamePromptController(IntPtr handle) : base(handle) {
            		Initialize();
        	}
        	// Called when created directly from a XIB file
        	[Export("initWithCoder:")]
		public NotebookNamePromptController(NSCoder coder) : base(coder) {
            		Initialize();
        	}
        	// Call to load from the XIB/NIB file
		public NotebookNamePromptController() : base("NotebookNamePrompt") {
            		Initialize();
        	}
        	// Shared initialization code
		void Initialize() {
        	}

		partial void AddNotebook (NSObject sender) {
            		string notebook = NotebookName.StringValue;

			if (AppDelegate.Notebooks.Contains(notebook, StringComparer.OrdinalIgnoreCase)) {
                		//The notebook already exists, hence should not be added again
                		NSAlert alert = new NSAlert () {
                    			MessageText = "Notebook Already Exists",
                    			InformativeText = "The Notebook " + notebook + " already exists.",
                    			AlertStyle = NSAlertStyle.Warning
                		};
                		alert.AddButton ("OK");
                		alert.BeginSheet (this.Window,
                    			this,
                    			null,
                    			IntPtr.Zero);

                		NotebookName.SelectText(this);
            		}
			else {
                		AppDelegate.Notebooks.Add (notebook);
                		Window.PerformClose (this);
                		AppDelegate.RefreshNotesWindowController ();
            		}
        }

        #endregion

        	//strongly typed window accessor
		public new NotebookNamePrompt Window {
			get {
                		return (NotebookNamePrompt)base.Window;
            		}
        	}
    }
}

