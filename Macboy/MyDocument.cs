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
	public partial class MyDocument : MonoMac.AppKit.NSDocument
	{
		List<string> history = new List<string> ();
		int currentHistoryPosition;

		// A unique identifier for a note
		string currentNoteID;
		Note currentNote;

		// Used as a marker. Are we loading a Note or something else that the Policy Handler should act on
		bool LoadingFromString;

		NSPopover popover;
		NoteLegacyTranslator translator= new NoteLegacyTranslator ();

		public MyDocument (IntPtr handle) : base (handle)
		{
			// Loading new blank note.
			LoadNewNote ();
			// might not need SetDisplayName later when I find how to properly load webkit on new windows. jlj
			SetDisplayName (currentNote.Title);
		}

		[Export ("initWithCoder:")]
		public MyDocument (NSCoder coder) : base (coder)
		{
		}

		public override void WindowControllerDidLoadNib (NSWindowController windowController)
		{
			base.WindowControllerDidLoadNib (windowController);
			UpdateBackForwardSensitivity ();
			noteWebView.FinishedLoad += HandleFinishedLoad;
			noteWebView.DecidePolicyForNavigation += HandleWebViewDecidePolicyForNavigation;
			Editable (true);
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

			if (LoadingFromString){
				WebView.DecideUse (e.DecisionToken);
				return;
			}

			WebView.DecideIgnore (e.DecisionToken);
			LoadNote (currentNoteID, true);
		}

		void HandleFinishedLoad (object sender, MonoMac.WebKit.WebFrameEventArgs e)
		{
			var dom = e.ForFrame.DomDocument;

			if (!string.IsNullOrEmpty (currentNote.Title)) {
				this.WindowForSheet.Title = currentNote.Title + " — Tomboy";
			} else {
				// Update the title of the current page from HTML
				var es = dom.GetElementsByTagName ("title");
				if (es.Count > 0 && !string.IsNullOrWhiteSpace (es [0].TextContent))
					this.WindowForSheet.Title = es [0].TextContent + " — Tomboy";
			}
		}

		void LoadNewNote ()
		{
			// this thing still has issues. The noteWebView is not initialized and I don't know how to get it.
			LoadingFromString = true;
			currentNote = AppDelegate.NoteEngine.NewNote ();
			currentNoteID = currentNote.Uri;

			InvalidateRestorableState ();
			LoadingFromString = false;
		}

		void LoadNote (string newNoteId, bool withHistory = true)
		{
			if (HasUnautosavedChanges)
				SaveData ();

			LoadingFromString = true;
			try {
				// on a crash, the document restore may try to load a key that doesn't exist any more.
				currentNote = AppDelegate.Notes[newNoteId];
			} catch (Exception e) {
				Logger.Error (e.Message, e);
				return;
			}
			if (currentNote == null)
				return;

			currentNoteID = newNoteId;
			InvalidateRestorableState ();

			Logger.Debug ("Note text before Translator {0}", currentNote.Text);
			string content = translator.From (currentNote);
			Logger.Debug ("Note text after Translator {0}", content);

			content = content.Replace (currentNote.Title, "<h1>" + currentNote.Title + "</h1>");
			Logger.Debug ("Note text after title {0}", content);

			// replace the system newlines with HTML new lines
			content = content.Replace ("\n", "<br>"); // strip NewLine LR types.May cause problems. Needs more testing
			noteWebView.MainFrame.LoadHtmlString (content, new NSUrl (AppDelegate.BaseUrlPath));

			// Make the note editable
			Editable (true);

			if (withHistory) {
				if (currentHistoryPosition < history.Count - 1)
					history.RemoveRange (currentHistoryPosition + 1,
					                     history.Count - (currentHistoryPosition + 1));
				history.Add (currentNoteID);
				currentHistoryPosition = history.Count - 1;
			}
			UpdateBackForwardSensitivity ();
			LoadingFromString = false;
			if (popover != null)
				popover.Close ();
			Logger.Debug ("Finished loading Note ID {0} \n Note Body '{1}'", newNoteId, content);
		}

		/// <summary>
		/// Should the Note be editable
		/// </summary>
		/// <param name='editable'>
		/// Editable.
		/// </param>
		void Editable (bool editable)
		{
			noteWebView.Editable = editable; // So that Notes can be Edited
		}

		private void SaveData ()
		{
			if (noteWebView == null)
				return;
			Logger.Info ("Saving Note ID {0}", currentNoteID);
			string results = translator.To (noteWebView.MainFrame.DomDocument);
			if (results == null || results.Length == 0) {
				Logger.Debug ("note content empty or null. Nothing to save for {0}", currentNoteID);
				return;
			}

			string title = GetTitleFromBody ();
			if (title == null) {
				Logger.Debug ("note title null. Nothing to save for {0}", currentNoteID);
				return;
			}
			
			currentNote.Text = results;
			currentNote.Title = title;
			if (this.WindowForSheet != null) // on closing of the Window this will not have a value
				this.WindowForSheet.Title = currentNote.Title + " — Tomboy";
			AppDelegate.NoteEngine.SaveNote (currentNote);

			/*
			 * Very important piece of code.(UpdateChangeCount)
			 * This allows us to trick NSDOcument into believing that we have saved the document
			 */
			UpdateChangeCount (NSDocumentChangeType.Cleared);
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

		/// <summary>
		/// Gets the body as html from the current document
		/// </summary>
		/// <description>This allows you to get other HTML elements from the Note content</description>
		/// <returns>
		/// string : everything inside the <body></body> tags
		/// </returns>
		private DomHtmlElement GetBody ()
		{
			DomNodeList element = noteWebView.MainFrame.DomDocument.GetElementsByTagName ("body");
			//FIXME: Need to make sure that we check for no body
			DomHtmlElement body = (DomHtmlElement)element.FirstOrDefault ();
			return body;
		}

		/// <summary>
		/// Gets the title from body of the note.
		/// It is considered that the title is always the first line of the Note.
		/// </summary>
		/// <returns>
		/// The title from body.
		/// </returns>
		private string GetTitleFromBody ()
		{
			// we do not care about any HTML markup. We just want the raw text.
			string innerText = GetBody ().InnerText;
			int loc = innerText.IndexOf ("\n",0);
			if (loc <= 0)
				return null;
			// crop the text at the first line break.
			return innerText.Substring (0, loc);
		}

		partial void BackForwardAction (MonoMac.AppKit.NSSegmentedControl sender)
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

		partial void StartSearch (MonoMac.AppKit.NSSearchField sender)
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

		public override NSData GetAsData (string documentType, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return null;
		}

		public override void SaveDocument (NSObject delegateObject, MonoMac.ObjCRuntime.Selector didSaveSelector, IntPtr contextInfo)
		{
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
		public override void SaveDocumentTo (NSObject sender)
		{
			Console.WriteLine ("SaveDocumentTo");
			base.SaveDocumentTo (sender);
		}

		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return false;
		}

		public override string WindowNibName { 
			get {
				return "MyDocument";
			}
		}
	}
}

