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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using System.Linq;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.WebKit;

namespace Tomboy
{
	public partial class MyDocument : NSDocument
	{
		readonly List<string> history = new List<string> ();
		int currentHistoryPosition;

		// A unique identifier for a note
		string currentNoteID;
		Note currentNote;

		// Used as a marker. Are we loading a Note or something else that the Policy Handler should act on
		private bool _loadingFromString;
       

		NSPopover popover;
		readonly NoteLegacyTranslator translator= new NoteLegacyTranslator ();

		public MyDocument (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MyDocument (NSCoder coder) : base (coder)
		{
		}

		public MyDocument () : base ()
		{
		}

		public MyDocument (Note note) : base ()
		{
			// some contructors might pass in null and not an actual note.
			if (note != null) {
				this.currentNoteID = note.Uri;
				this.currentNote = note;
			}
		}

        public string CurrentNoteID
        {
            get
            {
                return currentNoteID;
            }
        }

		public override void WindowControllerDidLoadNib (NSWindowController windowController)
		{
			base.WindowControllerDidLoadNib (windowController);
			UpdateBackForwardSensitivity ();
			noteWebView.FinishedLoad += HandleFinishedLoad;
			noteWebView.DecidePolicyForNavigation += HandleWebViewDecidePolicyForNavigation;
			Editable (true);

			if (string.IsNullOrEmpty(currentNoteID))
				LoadNewNote();
			else
				LoadNote();
		}

		/// <summary>
		/// Handles the web view decide policy for navigation.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
        /// </param>//WebNavigationPolicyEventArgs
		void HandleWebViewDecidePolicyForNavigation (object sender, WebNavigationPolicyEventArgs e)
		{
			// Reference for examples of this method in use
			// https://github.com/mono/monomac/commit/efc6e28fc03005638ce2cd217dc6c9281ad9c1c5

            var linked_note = e.Request.Url.AbsoluteString;
                        
            if (linked_note != null && linked_note.StartsWith("note:", StringComparison.CurrentCulture))
                OpenNote(e.Request.Url.AbsoluteString);

			if (_loadingFromString){
				WebView.DecideUse (e.DecisionToken);
				return;
			}

			WebView.DecideIgnore (e.DecisionToken);
		}

        private void OpenNote (string title)
        {
            Note note;
            var notes = AppDelegate.NoteEngine.GetNotes();
            notes.TryGetValue(title, out note);
            MyDocument myDoc = new MyDocument (note);
            var _sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
            _sharedDocumentController.AddDocument (myDoc);
            myDoc.MakeWindowControllers ();
            myDoc.ShowWindows ();
        }

		private void HandleFinishedLoad (object sender, WebFrameEventArgs e)
		{
            NoteTitle(currentNote.Title); // this needs to be here once the Window has loaded, otherwise the default window settings will override
        }

        partial void NoteTitleFieldSelector(NSObject sender)
        {
            if (!currentNote.Title.Equals (noteTitleField.Title, StringComparison.CurrentCulture)) {
                Logger.Debug ("Note Title Changing " + noteTitleField.Title);
                NoteTitle (noteTitleField.Title);
            }
        }

        private void NoteTitle (string title)
        {
            Logger.Debug("Setting Note Title to {0}", title);
            if (title == null)
                Logger.Error("NoteTitle cannot be null");

            if (title != null) {
                WindowForSheet.Title = title;
                SetDisplayName (title);
                noteTitleField.TextColor = NSColor.Black;
                noteTitleField.Title = title;
            }
        }

		void LoadNewNote ()
		{
			_loadingFromString = true;
			currentNote = AppDelegate.NoteEngine.NewNote ();
			currentNoteID = currentNote.Uri;
            NoteTitle(currentNote.Title);
			InvalidateRestorableState ();
			_loadingFromString = false;
		}

		void LoadNote (string newNoteId, bool withHistory = true)
		{			
			try {
				// on a crash, the document restore may try to load a key that doesn't exist any more.
				currentNote = AppDelegate.Notes[newNoteId];
				currentNoteID = newNoteId;
				LoadNote (withHistory);
			} catch (Exception e) {
				Logger.Error (e.Message, e);
				return;
			}
		}

        string WikiLinks (string body)
        {
            var finished_results = body;
            foreach (var item in AppDelegate.NoteEngine.GetNotes())
            {
                if (item.Value.Uri.Equals (currentNoteID)) // do not want to highlight our own note
                    continue;
                finished_results = finished_results.Replace(item.Value.Title, "<a href='" + item.Value.Uri + "'>" + item.Value.Title + "</a>");
            }
            return finished_results;
        }

		void LoadNote (bool withHistory = true)
        {
            if (HasUnautosavedChanges)
                SaveData();

            _loadingFromString = true;

            if (currentNote == null)
                return;

            InvalidateRestorableState();
            Logger.Debug("Translating Note:{0} \n Note text before Translator {1}", currentNoteID, currentNote.Text);
            var content = translator.From(currentNote);
            var beginIndx = content.IndexOf(currentNote.Title, StringComparison.CurrentCulture);

            if (beginIndx != -1) // it's possible that some notes do not have any content
            {
                var len = currentNote.Title.Length;
                content = content.Remove(beginIndx, (len + 1)); // +1 to remove the NewLine char after the title
            }
			// replace the system newlines with HTML new lines
			content = content.Replace ("\n", "<br>"); // strip NewLine LR types.May cause problems. Needs more testing
            noteWebView.MainFrame.LoadHtmlString (WikiLinks(content), new NSUrl (AppDelegate.BaseUrlPath));

			if (withHistory) {
				if (currentHistoryPosition < history.Count - 1)
					history.RemoveRange (currentHistoryPosition + 1,
					                     history.Count - (currentHistoryPosition + 1));
				history.Add (currentNoteID);
				currentHistoryPosition = history.Count - 1;
			}

			// Make the note editable
			Editable (true);
			UpdateBackForwardSensitivity ();
			_loadingFromString = false;

			if (popover != null)
				popover.Close ();

            Logger.Debug("Finished loading Note ID {0} \n Note Body '{1}'", currentNoteID, content);
		}

        private void UpdateLinks()
        {
            _loadingFromString = true;
            var content = translator.From(currentNote);
            var beginIndx = content.IndexOf(currentNote.Title, StringComparison.CurrentCulture);

            if (beginIndx != -1)
            {
                var len = currentNote.Title.Length;
                content = content.Remove(beginIndx, (len + 1));
            }
            content = content.Replace("\n", "<br>");

            noteWebView.MainFrame.LoadHtmlString(WikiLinks(content), null);
            _loadingFromString = false;
        }

		/// <summary>
		/// Should the Note be editable
		/// </summary>
		/// <param name='editable'>
		/// Editable.
		/// </param>
		private void Editable (bool editable)
		{
			noteWebView.Editable = editable; // So that Notes can be Edited
		}

		private void SaveData ()
		{
			if (noteWebView == null)
				return;

			Logger.Info ("Saving Note ID {0}", currentNoteID);

			try {
				string results = translator.To (noteWebView.MainFrame.DomDocument);
                if (string.IsNullOrEmpty(results) || currentNote.Title == null) {
					Logger.Debug("note content empty or null. Nothing to save for {0}", currentNoteID);
					return;
				}
                currentNote.Title = noteTitleField.Title;
                currentNote.Text = noteTitleField.Title;//FIXME Need to see if we should actually add the title to the contents.
                currentNote.Text += Environment.NewLine; 
				currentNote.Text += results;
                AppDelegate.NoteEngine.SaveNote (currentNote);

				if (!currentNote.Title.Equals (DisplayName))
					SetDisplayName (currentNote.Title);

                UpdateLinks();
				/*
				 * Very important piece of code.(UpdateChangeCount)
				 * This allows us to trick NSDOcument into believing that we have saved the document
				 */
				UpdateChangeCount (NSDocumentChangeType.Cleared);
                //LoadNote();
			} catch (Exception e) {
				Logger.Error ("Failed to Save Note {0}", e);
			}
		}

		/*public override bool HasUnautosavedChanges {
			get {
				return false;
			}
		}

		public override bool IsDocumentEdited {
			get {
				return false;
			}
		}*/

		partial void BackForwardAction (NSSegmentedControl sender)
		{
			var selected = sender.SelectedSegment;

			if (selected == 0)
				LoadNote (history[--currentHistoryPosition], false);
			else
				LoadNote (history[++currentHistoryPosition], false);

			sender.SetSelected (false, 0);
			sender.SetSelected (false, 1);
			UpdateBackForwardSensitivity ();
		}

		void UpdateBackForwardSensitivity ()
		{
			bool canGoBack = history.Count > 0 && currentHistoryPosition > 0;
			bool canGoForward = history.Count > 0 && currentHistoryPosition < history.Count - 1;
			backForwardControl.SetEnabled (canGoBack, 0);
			backForwardControl.SetEnabled (canGoForward, 1);
		}

		partial void StartSearch (NSSearchField sender)
		{
			var noteResults = AppDelegate.NoteEngine.GetNotes (sender.StringValue, true);
			NSMenu noteSearchMenu = new NSMenu ("Search Results");
			var action = new MonoMac.ObjCRuntime.Selector ("searchResultSelected");
			foreach (var name in noteResults.Values.Select (n => n.Title))
				noteSearchMenu.AddItem (name, action, string.Empty);
			Logger.Debug (sender.Frame.ToString ());
			Logger.Debug (sender.Superview.Frame.ToString ());
			Logger.Debug (sender.Superview.Superview.Frame.ToString ());
			NSEvent evt = NSEvent.OtherEvent (NSEventType.ApplicationDefined,
			                                  new PointF (sender.Frame.Left, sender.Frame.Top),
			                                  (NSEventModifierMask)0,
			                                  0,
			                                  sender.Window.WindowNumber,
			                                  sender.Window.GraphicsContext,
			                                  (short)NSEventType.ApplicationDefined,
			                                  0, 0);
			NSMenu.PopUpContextMenu (noteSearchMenu, evt, searchField);
		}

		[Export ("searchResultSelected")]
		void SearchResultSelected (NSObject sender)
		{
			NSMenuItem item = (NSMenuItem)sender;
			LoadNote(AppDelegate.NoteEngine.GetNote(item.Title).Uri);
		}

        /// <summary>
        /// Opens the ALL Notes Window
        /// </summary>
        /// <param name="sender">Sender.</param>
        partial void AllNotes (NSObject sender)
        {
            if (AppDelegate.controller == null)
                AppDelegate.controller = new NotesWindowController ();

            AppDelegate.controller.Window.MakeKeyAndOrderFront (this);
        }

		partial void ShowNotes (NSObject sender)
		{
			popover = new NSPopover ();
			ShowNotesPopupController controller = new ShowNotesPopupController ();
			controller.NoteNodeClicked += (s, e) => LoadNote(e.NoteId);
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = controller;
			popover.Show (RectangleF.Empty, sender as NSView, NSRectEdge.MaxYEdge);

		}

        partial void AddBulletPoint (NSObject sender)
        {
            _loadingFromString = true;
            var content = translator.From(currentNote);
            Console.WriteLine(content.Length);
            var beginIndx = content.IndexOf(currentNote.Title, StringComparison.CurrentCulture);

            if (beginIndx != -1 && content.Length > currentNote.Title.Length)
            {
                var len = currentNote.Title.Length;
                content = content.Remove(beginIndx, (len + 1));
            }

            string con = content.Insert(content.Length,"<ul><li>");
            noteWebView.MainFrame.LoadHtmlString(con,null);
            _loadingFromString = false;
        }

		partial void DeleteNote (NSObject sender)
		{
			NSAlert alert = new NSAlert () {
				MessageText = "Really delete this note?",
				InformativeText = "You are about to delete this note, this operation cannot be undone",
				AlertStyle = NSAlertStyle.Warning
			};
			alert.AddButton ("OK");
			alert.AddButton ("Cancel");
			alert.BeginSheet (WindowForSheet,
			                  this,
			                  new MonoMac.ObjCRuntime.Selector ("alertDidEnd:returnCode:contextInfo:"),
			                  IntPtr.Zero);
		}

		[Export ("alertDidEnd:returnCode:contextInfo:")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        void AlertDidEnd (NSAlert alert, int returnCode, IntPtr contextInfo)
		{
            if (alert == null)
                throw new ArgumentNullException("alert");
			if (((NSAlertButtonReturn)returnCode) == NSAlertButtonReturn.First) {
				AppDelegate.NoteEngine.DeleteNote (currentNote);
				currentNote = null;
				currentNoteID = null;
				Close ();
			}
		}

		public override void EncodeRestorableState (NSCoder coder)
		{
			base.EncodeRestorableState (coder);
			if (!string.IsNullOrEmpty (currentNoteID))
				coder.Encode (new NSString (currentNoteID), "savedNoteId");
		}

		public override void RestoreState (NSCoder coder)
		{
			base.RestoreState (coder);

			if (!coder.ContainsKey ("savedNoteId"))
				return;

			var id = (NSString)coder.DecodeObject ("savedNoteId");

			if (!string.IsNullOrEmpty (id))
				LoadNote (id);
		}

		public override NSData GetAsData (string typeName, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			Logger.Debug ("ReadFromData {0}", outError);
			return null;
		}

		public override void SaveDocument (NSObject delegateObject, MonoMac.ObjCRuntime.Selector didSaveSelector, IntPtr contextInfo)
		{
			Logger.Debug ("Not sure what this is doing yet SaveDocument {0}", delegateObject.GetType ());
			SaveData ();
		}

		// This is called every time the document is saved.
		// this is a default thing of NSDocument.
		/*public override NSUrl FileUrl {
			get {
				if (HasUnautosavedChanges)
					SaveData ();

				return base.FileUrl;
			}
			set {
				base.FileUrl = value;
			}
		}*/

		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			Logger.Debug ("ReadFromData error:{0}", outError);
			return false;
		}

		public override void SaveDocument (NSObject sender)
		{
			SaveData ();
			// this appears to be working when ctrl + s is hit.
		}
		public override void SaveDocumentAs (NSObject sender)
		{
			SaveData ();
		}

		public override void CanCloseDocument (NSObject delegateObject, MonoMac.ObjCRuntime.Selector shouldCloseSelector, IntPtr contextInfo)
		{
			SaveData ();
			// we must call the base class again otherwise the Window will not close.
			base.CanCloseDocument (delegateObject, shouldCloseSelector, contextInfo);
		}

		public override string WindowNibName { 
			get {
				return "MyDocument";
			}
		}
	}
}

