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

