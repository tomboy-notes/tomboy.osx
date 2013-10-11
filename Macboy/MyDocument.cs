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
using System.Linq;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;

namespace Tomboy
{
	public partial class MyDocument : NSDocument
	{
		List<string> history = new List<string> ();
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
				LoadNote(true);
          
		}

		/// <summary>
		/// Handles the web view decide policy for navigation.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		void HandleWebViewDecidePolicyForNavigation (object sender, WebNavigatioPolicyEventArgs e)
		{
			// Reference for examples of this method in use
			// https://github.com/mono/monomac/commit/efc6e28fc03005638ce2cd217dc6c9281ad9c1c5
            var noteID = e.Request.Url.AbsoluteString;
            LoadNote(noteID, false);
			if (_loadingFromString){
				WebView.DecideUse (e.DecisionToken);
				return;
			}

			WebView.DecideIgnore (e.DecisionToken);

            //FIXME: Not sure why this was in here, but it causes problems if I want WikiLinks to work.
			//LoadNote (currentNoteID, true);
		}

		private void HandleFinishedLoad (object sender, WebFrameEventArgs e)
		{
            var dom = e.ForFrame.DomDocument;
			if (!string.IsNullOrEmpty (currentNote.Title)) {
				WindowForSheet.Title = currentNote.Title + " — Tomboy";
			} else {
				// Update the title of the current page from HTML
				
				var es = dom.GetElementsByTagName ("title");
				if (es.Count > 0 && !string.IsNullOrWhiteSpace (es [0].TextContent))
					WindowForSheet.Title = es [0].TextContent + " — Tomboy";
			}
			// this sets thename of the document, which for example is used in a save operation.
			SetDisplayName (WindowForSheet.Title);
        }

		void LoadNewNote ()
		{
			// this thing still has issues. The noteWebView is not initialized and I don't know how to get it.
			_loadingFromString = true;
			currentNote = AppDelegate.NoteEngine.NewNote ();
			currentNoteID = currentNote.Uri;
            noteWebView.MainFrame.LoadHtmlString ("<h1>" + currentNote.Title + "</h1>", new NSUrl (AppDelegate.BaseUrlPath));
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
				SaveData ();

			_loadingFromString = true;

			if (currentNote == null)
				return;

			InvalidateRestorableState ();
			Logger.Debug ("Note text before Translator {0}", currentNote.Text);
            var content = translator.From(currentNote);
            content = content.Replace(currentNote.Title, "<h1>" + currentNote.Title + "</h1>");
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
				if (string.IsNullOrEmpty(results)) {
					Logger.Debug("note content empty or null. Nothing to save for {0}", currentNoteID);
					return;
				}

                DomNodeList element = noteWebView.MainFrame.DomDocument.GetElementsByTagName ("body");
                //FIXME: Need to make sure that we check for no body
                DomHtmlElement body = (DomHtmlElement)element.FirstOrDefault ();
                string innerText = body.InnerText;
                int loc = innerText.IndexOf ("\n", 0, StringComparison.CurrentCulture);
                // crop the text at the first line break.
                var title = innerText.Substring (0, loc);

                Logger.Debug ("Saving Note Title {0} \n Content {1}", title, results);
				if (title == null) {
					Logger.Debug ("note title null. Nothing to save for {0}", currentNoteID);
					return;
				}
				
				currentNote.Text = results;
				currentNote.Title = title;
                AppDelegate.NoteEngine.SaveNote (currentNote);
				
                if (WindowForSheet != null) // on closing of the Window this will not have a value
					WindowForSheet.Title = currentNote.Title + " — Tomboy";

				if (!currentNote.Title.Equals (DisplayName))
					SetDisplayName (currentNote.Title);

				/*
				 * Very important piece of code.(UpdateChangeCount)
				 * This allows us to trick NSDOcument into believing that we have saved the document
				 */
				UpdateChangeCount (NSDocumentChangeType.Cleared);

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
			LoadNote (AppDelegate.NoteEngine.GetNote (item.Title).Uri, true);
		}

		partial void ShowNotes (NSObject sender)
		{
			popover = new NSPopover ();
			ShowNotesPopupController controller = new ShowNotesPopupController ();
			controller.NoteNodeClicked += (s, e) => LoadNote (e.NoteId, true);
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = controller;
			popover.Show (RectangleF.Empty, sender as NSView, NSRectEdge.MaxYEdge);

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
		void AlertDidEnd (NSAlert alert, int returnCode, IntPtr contextInfo)
		{
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
		public override NSUrl FileUrl {
			get {
				if (HasUnautosavedChanges)
					SaveData ();

				return base.FileUrl;
			}
			set {
				base.FileUrl = value;
			}
		}

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

