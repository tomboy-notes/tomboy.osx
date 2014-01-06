using System;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
    public partial class SyncPrefDialogController : MonoMac.AppKit.NSWindowController
    {

        #region Constructors

        // Called when created from unmanaged code
        public SyncPrefDialogController(IntPtr handle) : base (handle)
        {
            Initialize();
        }
        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SyncPrefDialogController(NSCoder coder) : base (coder)
        {
            Initialize();
        }
        // Call to load from the XIB/NIB file
        public SyncPrefDialogController() : base ("SyncPrefDialog")
        {
            Initialize();
        }
        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        partial void SetSyncPath(NSObject sender)
        {
            var openPanel = new NSOpenPanel();
            openPanel.ReleasedWhenClosed = true;
            openPanel.CanChooseDirectories = true;
            openPanel.CanChooseFiles = false;
            openPanel.CanCreateDirectories = true;
            openPanel.Prompt = "Select Directory";

            var result = openPanel.RunModal();
            if (result == 1) {
                SyncPathTextField.Cell.Title = openPanel.DirectoryUrl.Path;
				AppDelegate.FilesystemSyncPath = openPanel.DirectoryUrl.Path;
			}

        }

        // This method will be called automatically when the main window "wakes up".
        [Export ("awakeFromNib:")]
        public override void AwakeFromNib()
        {
            // set according to AppDelegate, which later should be a preference.
            // FIXME: Turn into a system setting.
            EnableAutoSyncing.Enabled = AppDelegate.EnableAutoSync;
        }

        partial void EnableAutoSyncingAction(NSObject sender)
        {
            if (EnableAutoSyncing.Enabled)
                AppDelegate.EnableAutoSync = true;
            else
                AppDelegate.EnableAutoSync = false;
        }

        partial void StartFileSync(NSObject sender)
        {
            Console.WriteLine ("FAKE SYNC");
        }

        //strongly typed window accessor
        public new SyncPrefDialog Window
        {
            get
            {
                return (SyncPrefDialog)base.Window;
            }
        }
    }
}

