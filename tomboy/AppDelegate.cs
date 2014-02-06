//
// LicenseHeader.cs
//
// Author:
//       Jared Jennings <jjennings@gnome.org>
//
// Copyright (c) 2012 Jared Jennings 2012
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
using System.IO;
using System.Linq;

using MonoMac.AppKit;
using MonoMac.Foundation;
using Tomboy.Sync;
using Tomboy.Sync.Filesystem;

namespace Tomboy
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		private IStorage noteStorage;
		private ManifestTracker manifestTracker;

		// TODO this should not go here
		public static string FilesystemSyncPath;

		ControlCenterController controller;
		AboutUsController aboutUs;
		private int _maxNotesInMenu = 10;
		// if tomboy is being launched for the first time on a machine that had a previous version (tomboy)
		// make sure we get a copy as we are still in a development release.
		private string backupPathUri = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy", "v1");

		public AppDelegate ()
        {
			var storage_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy");
            // TODO, set it in a generic way
			noteStorage = new DiskStorage ();
            noteStorage.SetPath(storage_path);
            noteStorage.SetBackupPath(backupPathUri);

            if (!Directory.Exists(backupPathUri))
                noteStorage.Backup(); //FIXME: Need to better handle status messages.

			Logger.Debug ("Backup Path set to {0}", backupPathUri);

			NoteEngine = new Engine (noteStorage);

			// keep track of note for syncing
			// TODO move this into an Add-in one day
			var manifest_path = Path.Combine (storage_path, "manifest.xml");
			manifestTracker = new ManifestTracker (NoteEngine, manifest_path);

			// Create our cache directory
			if (!Directory.Exists (BaseUrlPath))
				Directory.CreateDirectory (BaseUrlPath);

			// Currently lazy load because otherwise the Dock Menu throws an error about there being no notes.
			if (Notes == null)
				Notes = NoteEngine.GetNotes ();

			NoteEngine.NoteAdded += HandleNoteAdded;
			NoteEngine.NoteRemoved += HandleNoteRemoved;
			NoteEngine.NoteUpdated += HandleNoteUpdated;
		}

        public static bool EnableAutoSync
        {
            get;
            set;
        }

        partial void SyncNotes(NSObject sender)
        {
			var dest_manifest_path = Path.Combine (AppDelegate.FilesystemSyncPath, "manifest.xml");
			SyncManifest dest_manifest;
			if (!File.Exists (dest_manifest_path))
				SyncManifest.Write (dest_manifest_path, new SyncManifest ());
			dest_manifest = SyncManifest.Read (dest_manifest_path);
			var dest_storage = new DiskStorage ();
			dest_storage.SetPath (AppDelegate.FilesystemSyncPath);
			var dest_engine = new Engine (dest_storage);

			var client = new FilesystemSyncClient (NoteEngine, manifestTracker.Manifest);
			var server = new FilesystemSyncServer (dest_engine, dest_manifest);
			var sync_manager = new SyncManager(client, server);
			sync_manager.DoSync ();

			// write back the dest manifest
			SyncManifest.Write (dest_manifest_path, dest_manifest);

        }
		public override void FinishedLaunching (NSObject notification)
		{
			//moving from nibFinishedLoading may address a few issues with crashes.
			//BuildDockMenuNotes ();
		}

        partial void Preferences(NSObject sender)
        {
            var prefC = new SyncPrefDialogController ();
            prefC.Window.MakeKeyAndOrderFront (this);
        }

		/// <summary>
		/// Builds the dock menu notes, currently populating the Menu with Notes. ALL NOTES
		/// </summary>
		void BuildDockMenuNotes ()
		{
			if (dockMenu == null)
				return;

			dockMenu.RemoveAllItems ();

			if (Notes != null || Notes.Count > 0) {
				if (Notes.Count < _maxNotesInMenu)
					_maxNotesInMenu = Notes.Count;
				for (int i = 0; i < _maxNotesInMenu; i++) {
					var item = new NSMenuItem ();
					var key_at = Notes.Keys.ElementAt (i);
					item.Title = Notes[key_at].Title;
					item.Activated += HandleActivated;
					dockMenu.AddItem (item);
				}
			}
		}

		void HandleActivated (object sender, EventArgs e)
		{
			NSMenuItem item = (NSMenuItem)sender;
			OpenNote (item.Title);
		}

		/// <summary>
		/// Opens the note that was selected from the Dock Menu
		/// </summary>
		/// <param name='title'>
		/// Title.
		/// </param>
		void OpenNote (string title)
		{
			var note = NoteEngine.GetNote (title);
			MyDocument myDoc = new MyDocument (note);
			var _sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			_sharedDocumentController.AddDocument (myDoc);
			myDoc.MakeWindowControllers ();
			myDoc.ShowWindows ();
		}

		void HandleNoteRemoved (Note note)
		{
			Logger.Debug ("AppDelegate Handling Note {0} removed", note.Title);
			Notes.Remove (note.Uri);
            using (NSMenuItem item = dockMenu.ItemWithTitle(note.Title)) {
                if (item != null)
                    dockMenu.RemoveItem(item);
            }
		}

		void HandleNoteUpdated (Note note)
		{
			Logger.Debug ("AppDelegate Handling Note {0} updated", note.Title);
			Notes.Remove (note.Uri); //FIXME: Why is this being removed and then added again?
			Notes.Add (note.Uri, note);
			try {
				//BuildDockMenuNotes ();
			} catch (Exception e) {
				Logger.Error ("Failed to update Dock Menu {0}", e);
			}
		}

		public static string BaseUrlPath {
			get {
				String SpecialFolderCache = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				                     "Library", "Cache", "Tomboy");
				Logger.Debug ("cache directory set to {0}", SpecialFolderCache);
				return SpecialFolderCache;
			}
		}

		public static Engine NoteEngine {
			get;
			set;
		}

		public static Dictionary<string, Note> Notes {
			get;
			set;
		}
		void HandleNoteAdded (Note note)
		{
			Logger.Debug ("AppDelegate Handling Note Added {0}", note.Title);

			try {
				NSMenuItem item = new NSMenuItem ();
				item.Title = note.Title;
				item.Activated += HandleActivated;
				dockMenu.AddItem (item);
			} catch (Exception e) {
				Logger.Error ("Failed to add item from Dock Menu {0}", e);
			}
		}

		partial void OpenDashboard (NSObject sender)
		{
			if (controller == null)
				controller = new ControlCenterController ();
			controller.Window.MakeKeyAndOrderFront (this);
		}

		public override bool ApplicationShouldHandleReopen (NSApplication sender, bool hasVisibleWindows)
		{
			return true;
		}

		public override bool ApplicationShouldOpenUntitledFile (NSApplication sender)
		{
			return true;
		}

		public override bool ApplicationOpenUntitledFile (NSApplication sender)
		{
			return true;
		}

		partial void MenuClickedAboutTomboy (NSObject sender)
		{
			if(aboutUs == null)
				aboutUs = new AboutUsController();
			aboutUs.Window.MakeKeyAndOrderFront(this);
		}

		partial void MenuClickedNewNote (NSObject sender)
		{
			var _sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			_sharedDocumentController.NewDocument (null);
		}

		partial void MenuClickedSearchNotes (NSObject sender)
		{
			OpenDashboard (sender);
		}

		#region private methods
		private void LoadDashboardWindow ()
		{
			if (controller == null)
				controller = new ControlCenterController ();
			controller.Window.MakeKeyAndOrderFront (this);
		}

		#endregion private methods

		public new void Dispose ()
		{
			base.Dispose ();
			manifestTracker.Dispose ();
		}
	}
}

