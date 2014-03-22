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

		public static SettingsSync settings;

        public static NotesWindowController controller;
		AboutUsController aboutUs;
        private int _maxNotesInMenu = 10;
		//Maintains the current count of Notes added to Dock
		private int dockMenuNoteCounter = 0;
		//Maximum Notes which can be added to the Dock is 10.
		private const int MAXNOTES = 10;

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

			settings = SettingsSync.Read();

            //getSycnedNotes();

            //SyncNotes(null);
         

		}

        private void getSycnedNotes(){
            var sync_storage = new DiskStorage();
            sync_storage.SetPath(settings.syncURL);

            var sync_engine = new Engine(sync_storage);

            Dictionary<String,Note> synced_notes = sync_engine.GetNotes ();

            for (int i = 0; i < synced_notes.Count; i++)
            {
                String title = synced_notes.Keys.ElementAt(i);
                Note note = synced_notes[title];

                if (!Notes.ContainsKey(title))
                {
                    Notes.Add(title, note);
                }
                else
                {
                    DateTime last_change_date = note.ChangeDate;
                    String date = last_change_date.ToString();
                    title = title + "_" + date;

                    Console.WriteLine(title);
                    Notes.Add(title, note);

                }

            }
		}

        public static bool EnableAutoSync
        {
            get;
            set;
        }

        partial void SyncNotes(NSObject sender)
        {
			var dest_manifest_path = Path.Combine (settings.syncURL, "manifest.xml");
			SyncManifest dest_manifest;
			if (!File.Exists (dest_manifest_path))
				SyncManifest.Write (dest_manifest_path, new SyncManifest ());
			dest_manifest = SyncManifest.Read (dest_manifest_path);
			var dest_storage = new DiskStorage ();
			dest_storage.SetPath (settings.syncURL);
			var dest_engine = new Engine (dest_storage);

			var client = new FilesystemSyncClient (NoteEngine, manifestTracker.Manifest);
			var server = new FilesystemSyncServer (dest_engine, dest_manifest);
			var sync_manager = new SyncManager(client, server);
			sync_manager.DoSync ();
			RefreshNotesWindowController();
			// write back the dest manifest
			SyncManifest.Write (dest_manifest_path, dest_manifest);

        }
		public override void FinishedLaunching (NSObject notification)
		{
			//moving from nibFinishedLoading may address a few issues with crashes.
			BuildDockMenuNotes ();

			if (controller == null)
				controller = new NotesWindowController();
			controller.Window.MakeMainWindow();
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
			if (dockMenu == null || Notes.Count == 0)
				return;

			ArrangeDateWise();
		}

		/// <summary>
		/// Arranges the Dock Menu with lastest change date.
		/// Last 10 modified Notes are shown up in the Dock Menu.
		/// </summary>
		void ArrangeDateWise(){
			if (Notes != null || Notes.Count > 0)
			{
				int count = Notes.Count;
				Dictionary<DateTime,Note> dateDict = new Dictionary<DateTime, Note>();

				for (int i = 0; i < count; i++)
				{
					Note temp = Notes.Values.ElementAt(i);
					if(!dateDict.ContainsKey(temp.ChangeDate))
						dateDict.Add(temp.ChangeDate, temp);
				}

				var dateList = dateDict.Keys.ToList();
				dateList.Sort();

				if (dateDict.Count >= MAXNOTES)
					for (int i = 0; i < MAXNOTES && dockMenuNoteCounter <= MAXNOTES; i++)
					{
						var item = new NSMenuItem();
						DateTime date = dateList.ElementAt(dateDict.Count - i - 1);
						item.Title = dateDict[date].Title;
						item.Activated += HandleActivated;
						dockMenu.AddItem(item);
						dockMenuNoteCounter += 1;
					}
				else
					for (int i = 0; i < dateDict.Count; i++)
					{
						var item = new NSMenuItem();
						DateTime date = dateList.ElementAt(dateDict.Count - i - 1);
						item.Title = dateDict[date].Title;
						item.Activated += HandleActivated;
						dockMenu.AddItem(item);
						dockMenuNoteCounter += 1;
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
				dockMenuNoteCounter -= 1;
            }

			RefreshNotesWindowController();
		}

		void HandleNoteUpdated (Note note)
		{
			Logger.Debug ("AppDelegate Handling Note {0} updated", note.Title);
			Notes.Remove (note.Uri); //FIXME: Why is this being removed and then added again?
			Notes.Add (note.Uri, note);
			try {
				UpdateDock();
                RefreshNotesWindowController();
			} catch (Exception e) {
				Logger.Error ("Failed to update Dock Menu {0}", e);
			}
		}

		/// <summary>
		/// Updates the Dock Menu when the Notes are modified
		/// </summary>
		private void UpdateDock(){
			int count = dockMenuNoteCounter;
			while (count > 0)
			{
				/*Using the Index 4, becase 0 to 3 are reserved for About Us,
				 * Search Notes, New Note and horizontal bar*/
				dockMenu.RemoveItemAt(4);
				dockMenuNoteCounter -= 1;
				count--;
			}

			ArrangeDateWise();
			RefreshNotesWindowController();
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
				dockMenuNoteCounter += 1;
				RefreshNotesWindowController();
			} catch (Exception e) {
				Logger.Error ("Failed to add item from Dock Menu {0}", e);
			}
		}

        public void RefreshNotesWindowController()
        {
		    if (controller == null)
				controller = new NotesWindowController();
            controller.UpdateNotesTable();
		}

		partial void OpenDashboard (NSObject sender)
		{
			if (controller == null)
				controller = new NotesWindowController ();
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
				controller = new NotesWindowController ();
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

