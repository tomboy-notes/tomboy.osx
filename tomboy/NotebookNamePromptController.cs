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
        public NotebookNamePromptController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public NotebookNamePromptController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        // Call to load from the XIB/NIB file
        public NotebookNamePromptController() : base("NotebookNamePrompt")
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
        }

        partial void AddNotebook (NSObject sender)
        {
            string notebook = NotebookName.StringValue;

            if (AppDelegate.Notebooks.Contains(notebook, StringComparer.OrdinalIgnoreCase))
            {
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
            else
            {
                AppDelegate.Notebooks.Add (notebook);
                Window.PerformClose (this);
                AppDelegate.RefreshNotesWindowController ();
            }

            //this.Window.Close();
        }

        #endregion

        //strongly typed window accessor
        public new NotebookNamePrompt Window
        {
            get
            {
                return (NotebookNamePrompt)base.Window;
            }
        }
    }
}

