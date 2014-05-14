//
// NoteSync.cs
//
// Author:
//       Timo Dörr <timo@latecrew.de>
//
// Copyright (c) 2013 Timo Dörr
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
using System.Xml;
using Tomboy.Sync;

namespace Tomboy
{

	public class ManifestTracker : IDisposable
	{
		public SyncManifest Manifest;
		private string path;

		public ManifestTracker (Engine engine, string path) {
			this.path = path;

			if (!File.Exists (path)) {
				Manifest = new SyncManifest ();
				using (var output = new FileStream (path, FileMode.Create)) {
					SyncManifest.Write (Manifest, output);
				}
				foreach (Note note in engine.GetNotes ().Values) {
					Manifest.NoteRevisions [note.Guid] = Manifest.LastSyncRevision + 1;
				}
				Flush ();
			}

			using (var input = new FileStream (path, FileMode.Open)) {
				this.Manifest = SyncManifest.Read (input);
                		input.Close();
			}
			engine.NoteAdded += (Note note) => {
				Console.WriteLine ("Note added");
				Manifest.NoteRevisions [note.Guid] = Manifest.LastSyncRevision + 1;
			};

			engine.NoteUpdated += (Note note) => {
				Console.WriteLine ("Note updated");
				Manifest.NoteRevisions [note.Guid] = Manifest.LastSyncRevision + 1;
			};

			engine.NoteRemoved += (Note note) => {
				Console.WriteLine ("Note removed: " + note.Guid);
				Manifest.NoteDeletions.Add (note.Guid, note.Title);
			};
		}

		private void Flush () {
			// write out the manifest to our xml file
			using (var output = new FileStream (path, FileMode.Create)) {
				SyncManifest.Write (Manifest, output);
			}
		}

		public void Dispose () {
			Flush ();
		}
	}
}