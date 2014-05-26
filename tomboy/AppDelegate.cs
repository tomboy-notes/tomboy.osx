//
// LicenseHeader.cs
//
// Author:
//      Jared Jennings <jjennings@gnome.org>
//	Rashid Khan <rashood.khan@gmail.com>
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
		private DiskStorage noteStorage;
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

		//Stores the current Notebook selected
        	public static string currentNotebook;

		// if tomboy is being launched for the first time on a machine that had a previous version (tomboy)
		// make sure we get a copy as we are still in a development release.
		private string backupPathUri = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy", "v1");

		/// <summary>
		/// Initializes a new instance of the <see cref="Tomboy.AppDelegate"/> class.
		/// </summary>
		public AppDelegate () {
			var storage_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy");
            		// TODO, set it in a generic way
            		noteStorage = new DiskStorage (storage_path);
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

            		Notebooks = new List<string>();
            		currentNotebook = "All Notebooks";
            		PopulateNotebookList();
		}

		/// <summary>
		/// Syncs the notes.
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void SyncNotes(NSObject sender) {
			var dest_manifest_path = Path.Combine (settings.syncURL, "manifest.xml");
			SyncManifest dest_manifest;
			if (!File.Exists (dest_manifest_path)) {
				using (var output = new FileStream (dest_manifest_path, FileMode.Create)) {
					SyncManifest.Write (new SyncManifest (), output);
				}
			}
			using (var input = new FileStream (dest_manifest_path, FileMode.Open)) {
				dest_manifest = SyncManifest.Read (input);
			}
			var dest_storage = new DiskStorage (settings.syncURL);
			var dest_engine = new Engine (dest_storage);

			var client = new FilesystemSyncClient (NoteEngine, manifestTracker.Manifest);
			var server = new FilesystemSyncServer (dest_engine, dest_manifest);
			var sync_manager = new SyncManager(client, server);
			sync_manager.DoSync ();
			RefreshNotesWindowController();
			// write back the dest manifest
		        using (var output = new FileStream (dest_manifest_path, FileMode.Create)) {
				SyncManifest.Write (dest_manifest, output);
			}

        	}

		/// <summary>
		/// It is called when the application has finished launching
		/// </summary>
		/// <param name="notification">Notification.</param>
		public override void FinishedLaunching (NSObject notification) {
			//moving from nibFinishedLoading may address a few issues with crashes.
			BuildDockMenuNotes ();

			if (controller == null)
				controller = new NotesWindowController();
			controller.Window.MakeMainWindow();
		}

		/// <summary>
		/// Generates the preference window
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void Preferences(NSObject sender) {
            		var prefC = new SyncPrefDialogController ();
            		prefC.Window.MakeKeyAndOrderFront (this);
        	}

		/// <summary>
		/// Builds the dock menu notes, currently populating the Menu with Notes. ALL NOTES
		/// </summary>
		void BuildDockMenuNotes () {
			if (dockMenu == null || Notes.Count == 0)
				return;

			ArrangeDateWise();
		}

		/// <summary>
		/// Arranges the Dock Menu with lastest change date.
		/// Last 10 modified Notes are shown up in the Dock Menu.
		/// </summary>
		void ArrangeDateWise () {
			if (Notes != null || Notes.Count > 0) {
				int count = Notes.Count;
				Dictionary<DateTime,Note> dateDict = new Dictionary<DateTime, Note>();

				for (int i = 0; i < count; i++) {
					Note temp = Notes.Values.ElementAt(i);
					if(!dateDict.ContainsKey(temp.ChangeDate))
						dateDict.Add(temp.ChangeDate, temp);
				}

				var dateList = dateDict.Keys.ToList();
				dateList.Sort();

				if (dateDict.Count >= MAXNOTES)
					for (int i = 0; i < MAXNOTES && dockMenuNoteCounter <= MAXNOTES; i++) {
						var item = new NSMenuItem();
						DateTime date = dateList.ElementAt(dateDict.Count - i - 1);
						item.Title = dateDict[date].Title;
						item.Activated += HandleActivated;
						dockMenu.AddItem(item);
						dockMenuNoteCounter += 1;
					}
				else
					for (int i = 0; i < dateDict.Count; i++) {
						var item = new NSMenuItem();
						DateTime date = dateList.ElementAt(dateDict.Count - i - 1);
						item.Title = dateDict[date].Title;
						item.Activated += HandleActivated;
						dockMenu.AddItem(item);
						dockMenuNoteCounter += 1;
					}

			}

		}

		/// <summary>
		/// Populates the notebook list with existing notebooks
		/// </summary>
		void PopulateNotebookList () {
            		Notebooks.Add("All Notebooks");
			foreach (KeyValuePair<string, Note> note in Notes) {
				if (note.Value.Notebook != null && !note.Value.Notebook.Equals ("All Notebooks",StringComparison.OrdinalIgnoreCase))
					if (!Notebooks.Contains (note.Value.Notebook))
						Notebooks.Add (note.Value.Notebook);
            		}
        	}

		/// <summary>
		/// Handles the activated.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void HandleActivated (object sender, EventArgs e) {
			NSMenuItem item = (NSMenuItem)sender;
			OpenNote (item.Title);
		}

		/// <summary>
		/// Opens the note that was selected from the Dock Menu
		/// </summary>
		/// <param name='title'>
		/// Title.
		/// </param>
		void OpenNote (string title) {
			var note = NoteEngine.GetNote (title);
			MyDocument myDoc = new MyDocument (note);
			var _sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			_sharedDocumentController.AddDocument (myDoc);
			myDoc.MakeWindowControllers ();
			myDoc.ShowWindows ();
		}

		/// <summary>
		/// Handles the note removed.
		/// </summary>
		/// <param name="note">Note.</param>
		void HandleNoteRemoved (Note note) {
			Logger.Debug ("AppDelegate Handling Note {0} removed", note.Title);
			Notes.Remove (note.Uri);
            		using (NSMenuItem item = dockMenu.ItemWithTitle(note.Title)) {
                		if (item != null)
                    		dockMenu.RemoveItem(item);
				dockMenuNoteCounter -= 1;
            		}

			RefreshNotesWindowController();
		}

		/// <summary>
		/// Handles the note updated.
		/// </summary>
		/// <param name="note">Note.</param>
		void HandleNoteUpdated (Note note) {
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
		/// Handles the note added.
		/// </summary>
		/// <param name="note">Note.</param>
		void HandleNoteAdded (Note note) {
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

		/// <summary>
		/// Updates the Dock Menu when the Notes are modified
		/// </summary>
		private void UpdateDock () {
			int count = dockMenuNoteCounter;
			while (count > 0) {
				/*Using the Index 4, becase 0 to 3 are reserved for About Us,
				 * Search Notes, New Note and horizontal bar*/
				dockMenu.RemoveItemAt (4);
				dockMenuNoteCounter -= 1;
				count--;
			}

			ArrangeDateWise ();
			RefreshNotesWindowController ();
		}

		/// <summary>
		/// Gets the base URL path.
		/// </summary>
		/// <value>The base URL path.</value>
		public static string BaseUrlPath {
			get {
				String SpecialFolderCache = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				                     "Library", "Caches", "Tomboy");
				Logger.Debug ("cache directory set to {0}", SpecialFolderCache);
				return SpecialFolderCache;
			}
		}

		/// <summary>
		/// Gets or sets the note engine.
		/// </summary>
		/// <value>The note engine.</value>
		public static Engine NoteEngine {
			get;
			set;
		}

		/// <summary>
		/// Maintains the notes to be shown in the Table.
		/// </summary>
		/// <value>The notes.</value>
		public static Dictionary<string, Note> Notes {
			get;
			set;
		}

		/// <summary>
		/// Maintains the list of Notebooks created
		/// </summary>
		/// <value>The notebooks.</value>
		public static List<string> Notebooks {
			get;
			set;
        	}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Tomboy.AppDelegate"/> enable auto sync.
		/// </summary>
		/// <value><c>true</c> if enable auto sync; otherwise, <c>false</c>.</value>
		public static bool EnableAutoSync {
			get;
			set;
		}
			
        	/// <summary>
        	/// Refreshs the notes window controller.
        	/// Method refreshes the table in Notes Window, to update each time
		/// some notes are updated, deleted or added.
		/// </summary>
		public static void RefreshNotesWindowController () {
		    if (controller == null)
				controller = new NotesWindowController();
            		controller.UpdateNotesTable();
            		controller.UpdateNotebooksTable();
		}

		public static void NotebookSingleClick () {
			controller.UpdateTableSingleClick ();
		}

		/// <summary>
		/// Opens the dashboard.
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void OpenDashboard (NSObject sender) {
			if (controller == null)
				controller = new NotesWindowController ();
			controller.Window.MakeKeyAndOrderFront (this);
		}

		/// <summary>
		/// Applications the should handle reopen.
		/// </summary>
		/// <returns><c>true</c>, if should handle reopen was applicationed, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="hasVisibleWindows">If set to <c>true</c> has visible windows.</param>
		public override bool ApplicationShouldHandleReopen (NSApplication sender, bool hasVisibleWindows) {
			return true;
		}

		/// <summary>
		/// Applications the should open untitled file.
		/// </summary>
		/// <returns><c>true</c>, if should open untitled file was applicationed, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		public override bool ApplicationShouldOpenUntitledFile (NSApplication sender) {
			return true;
		}

		/// <summary>
		/// Applications the open untitled file.
		/// </summary>
		/// <returns><c>true</c>, if open untitled file was applicationed, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		public override bool ApplicationOpenUntitledFile (NSApplication sender) {
			return true;
		}

		/// <summary>
		/// Menus the clicked about tomboy.
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void MenuClickedAboutTomboy (NSObject sender) {
			if(aboutUs == null)
				aboutUs = new AboutUsController();
			aboutUs.Window.MakeKeyAndOrderFront(this);
		}

		/// <summary>
		/// Menus the clicked new note.
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void MenuClickedNewNote (NSObject sender) {
			var _sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			_sharedDocumentController.NewDocument (null);
		}

		/// <summary>
		/// Menus the clicked search notes.
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void MenuClickedSearchNotes (NSObject sender) {
			OpenDashboard (sender);
		}

		#region private methods
		/// <summary>
		/// Loads the dashboard window.
		/// </summary>
		private void LoadDashboardWindow () {
			if (controller == null)
				controller = new NotesWindowController ();
			controller.Window.MakeKeyAndOrderFront (this);
		}

		#endregion private methods

		/// <summary>
		/// Releases all resource used by the <see cref="Tomboy.AppDelegate"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Tomboy.AppDelegate"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Tomboy.AppDelegate"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Tomboy.AppDelegate"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Tomboy.AppDelegate"/> was occupying.</remarks>
		public new void Dispose () {
			base.Dispose ();
			manifestTracker.Dispose ();
		}
	}
}

