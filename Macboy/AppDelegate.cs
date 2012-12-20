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
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		ControlCenterController controller;
		private int max_notes_in_menu = 10;

		public AppDelegate ()
		{
			string backup_path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy", "v1");
			// TODO, set it in a generic way
			Tomboy.DiskStorage.Instance.SetPath (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy"));
			Tomboy.DiskStorage.Instance.SetBackupPath (backup_path);
			if (!Directory.Exists (backup_path))
				Tomboy.DiskStorage.Instance.Backup ();

			NoteEngine = new Engine (Tomboy.DiskStorage.Instance);

			// Create our cache directory
			if (!Directory.Exists (BaseUrlPath))
				Directory.CreateDirectory (BaseUrlPath);

			// Currently lazy load because otherwise the Dock Menu throws an error about there being no notes.
			if (Notes == null)
				Notes = NoteEngine.GetNotes ();

			Engine.NoteAdded += HandleNoteAdded;
			Engine.NoteRemoved += HandleNoteRemoved;
		}

		[Export ("awakeFromNib:")]
		public override void AwakeFromNib ()
		{
			BuildDockMenuNotes ();
		}

		/// <summary>
		/// Builds the dock menu notes, currently populating the Menu with Notes. ALL NOTES
		/// </summary>
		void BuildDockMenuNotes ()
		{
			if (Notes != null || Notes.Count > 0) {
				if (Notes.Count < max_notes_in_menu)
					max_notes_in_menu = Notes.Count;
				for (int i = 0; i < max_notes_in_menu; i++) {
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
			Console.WriteLine ("AppDelegate Handling Note {0} removed", note.Title);
		}

		public static string BaseUrlPath {
			get {
				return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				                     "Library", "Cache", "Tomboy");
			}
		}

		public static Tomboy.Engine NoteEngine {
			get;
			set;
		}

		public static Dictionary<string, Note> Notes {
			get;
			set;
		}
		void HandleNoteAdded (Note note)
		{
			Logger.Debug ("Handling Note Added {0}", note.Title);

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
			Console.WriteLine ("Tomboy is an opensource Note application");
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

