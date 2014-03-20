using System;
using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy.Sync;

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
				//AppDelegate.FilesystemSyncPath = openPanel.DirectoryUrl.Path;

				AppDelegate.settings.syncURL = openPanel.DirectoryUrl.Path;
				SettingsSync.Write(AppDelegate.settings);

				statusField.StringValue = "Sync path set at "+AppDelegate.settings.syncURL;
			}

        }

        partial void setExportNotesPath(NSObject sender)
        {
            var openPanel = new NSOpenPanel();
            openPanel.ReleasedWhenClosed = true;
            openPanel.CanChooseDirectories = true;
            openPanel.CanChooseFiles = false;
            openPanel.CanCreateDirectories = false;
            openPanel.Prompt = "Select Existing Notes Directory";

            var result = openPanel.RunModal();
            if (result == 1)
            {
                ExportPathTextField.StringValue = openPanel.DirectoryUrl.Path;
            }

        }

        // This method will be called automatically when the main window "wakes up".
        [Export ("awakeFromNib:")]
        public override void AwakeFromNib()
        {
            // set according to AppDelegate, which later should be a preference.
            // FIXME: Turn into a system setting.
			SyncPathTextField.StringValue = AppDelegate.settings.syncURL;
			EnableAutoSyncing.Enabled = true;
			//Console.WriteLine(AppDelegate.settings.autoSync.ToString());
        }

        partial void EnableAutoSyncingAction(NSObject sender)
        {
            if (EnableAutoSyncing.Enabled)
				AppDelegate.settings.autoSync = true;
            else
				AppDelegate.settings.autoSync = false;

			SettingsSync.Write(AppDelegate.settings);
			//Console.WriteLine("Enabled Auto Sync - ");
			//Console.WriteLine(AppDelegate.settings.autoSync.ToString());
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

