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
			

				string homeDir = System.Environment.GetEnvironmentVariable("HOME");
				string settingsDir = System.IO.Path.Combine(homeDir,".tomboy");

				//CreateDirectory checks if the folder exists, if it does, then no exceptions are thrown
				System.IO.Directory.CreateDirectory(settingsDir);

				string settingsFile = System.IO.Path.Combine(settingsDir,"syncSettings.txt");

				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(settingsFile)){
					writer.WriteLine(AppDelegate.FilesystemSyncPath);
				}

			}								                 

        }

        // This method will be called automatically when the main window "wakes up".
        [Export ("awakeFromNib:")]
        public override void AwakeFromNib()
        {
            // set according to AppDelegate, which later should be a preference.
            // FIXME: Turn into a system setting.
            EnableAutoSyncing.Enabled = AppDelegate.EnableAutoSync;

			//Loads the Settings which was saved
			string homeDir = System.Environment.GetEnvironmentVariable("HOME");
			string settingsDir = System.IO.Path.Combine(homeDir,".tomboy");
			string settingsFile = System.IO.Path.Combine(settingsDir,"syncSettings.txt");

			if (System.IO.File.Exists(settingsFile)){
				using (System.IO.StreamReader reader = new System.IO.StreamReader(settingsFile)){
					string syncPath = reader.ReadLine();

					AppDelegate.FilesystemSyncPath = syncPath;
					SyncPathTextField.StringValue = syncPath.ToString();
				}
			}
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

