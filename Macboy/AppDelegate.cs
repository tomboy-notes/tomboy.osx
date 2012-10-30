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

using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy;

namespace Tomboy
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate ()
		{
			string backup_path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy", "v1");
			// TODO, set it in a generic way
			Tomboy.DiskStorage.Instance.SetPath (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Application Support", "Tomboy"));
			Tomboy.DiskStorage.Instance.SetBackupPath (backup_path);
			if (!Directory.Exists (backup_path))
				Tomboy.DiskStorage.Instance.Backup ();

			NoteEngine = new Engine (Tomboy.DiskStorage.Instance);
			Notes = NoteEngine.GetNotes ();

			// Create our cache directory
			if (!Directory.Exists (BaseUrlPath))
				Directory.CreateDirectory (BaseUrlPath);

			Engine.NoteAdded += HandleNoteAdded;
			ControlCenterController cc = new ControlCenterController ();
			cc.Window.MakeKeyAndOrderFront (this);

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
	}
}

