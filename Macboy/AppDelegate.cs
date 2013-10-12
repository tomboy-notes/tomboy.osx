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

namespace Tomboy
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		ControlCenterController controller;
		private int _maxNotesInMenu = 10;
		// if Macboy is being launched for the first time on a machine that had a previous version (tomboy)
		// make sure we get a copy as we are still in a development release.
		private string backupPathUri = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy", "v1");

		public AppDelegate ()
        {
            // TODO, set it in a generic way
            DiskStorage.Instance.SetPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy"));
            DiskStorage.Instance.SetBackupPath(backupPathUri);

            if (!Directory.Exists(backupPathUri))
                DiskStorage.Instance.Backup(); //FIXME: Need to better handle status messages.

			Logger.Debug ("Backup Path set to {0}", backupPathUri);

			NoteEngine = new Engine (DiskStorage.Instance);

			// Create our cache directory
			if (!Directory.Exists (BaseUrlPath))
				Directory.CreateDirectory (BaseUrlPath);

			// Currently lazy load because otherwise the Dock Menu throws an error about there being no notes.
			if (Notes == null)
				Notes = NoteEngine.GetNotes ();

			Engine.NoteAdded += HandleNoteAdded;
			Engine.NoteRemoved += HandleNoteRemoved;
			Engine.NoteUpdated += HandleNoteUpdated;
		}

        public static bool EnableAutoSync
        {
            get;
            set;
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
			// TODO implement this method
            throw new NotImplementedException ();
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
	}
}

