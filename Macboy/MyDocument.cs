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

		/// <summary>
		/// Sets the Note title, which is not only stored in the Window Title, but also in the note body
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		private void SetContentTitle (Note note)
		{
			/* The Note title is also contained in the Note Body */
			string noteText = note.Text.Replace (note.Title, "<h1>" + note.Title + "</h1>");
			note.Text = noteText;
		}

		void LoadNote (string newNoteId, bool withHistory = true)
		{
			LoadingFromString = true;
			var note = AppDelegate.Notes[newNoteId];
			if (note == null)
				return;
			currentNote = note;
			currentNoteID = newNoteId;
			InvalidateRestorableState ();
			note.Text = translator.From (note);
			SetContentTitle (note);
			// replace the system newlines with HTML new lines
			note.Text = note.Text.Replace ("\n", "<br>"); // strip NewLine LR types.May cause problems. Needs more testing
			Logger.Debug ("Loading Note ID {0} \n Note Body '{1}'", newNoteId, currentNote.Text);
			noteWebView.MainFrame.LoadHtmlString (currentNote.Text, new NSUrl (AppDelegate.BaseUrlPath));

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
			Logger.Info ("Saving Note ID {0}", currentNoteID);
			string results = translator.TranslateTo (noteWebView.MainFrame.DomDocument);
			currentNote.Text = results;
			AppDelegate.NoteEngine.SaveNote (currentNote);
		}

		void SaveNewNote ()
		{
			Note newNote = AppDelegate.NoteEngine.NewNote ();
			string content = GetBodyAsHtml ();
			string noteTitle = GetTitleFromBody ();
			/* The Note title is also contained in the Note Body */
			newNote.Title = noteTitle;
			/* Set the Note Title so that it appears as a Title in The Content of the Note */
			newNote.Text = content.Replace (noteTitle, "<h1>" + noteTitle + "</h1>");
			AppDelegate.NoteEngine.SaveNote (newNote);
			LoadNote (newNote.Uri, true);
		}

		/// <summary>
		/// Gets the body as html from the current document
		/// </summary>
		/// <description>This allows you to get other HTML elements from the Note content</description>
		/// <returns>
		/// string : everything inside the <body></body> tags
		/// </returns>
		private string GetBodyAsHtml ()
		{
			DomNodeList element = noteWebView.MainFrame.DomDocument.GetElementsByTagName ("body");
			//FIXME: Need to make sure that we check for no body
			DomHtmlElement body = (DomHtmlElement)element.FirstOrDefault ();
			return body.InnerHTML;
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
			string content = GetBodyAsHtml ();
			//FIXME: Need to see if the Note already contains an <h1>
			int indx = content.IndexOf ("<br>");
			return content.Substring (0, (indx)).Replace ("<div>", "");
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

		public override void SaveDocument (NSObject sender)
		{
			if (String.IsNullOrEmpty (currentNoteID))
				SaveNewNote ();
			else
				SaveData ();
		}

		public override void SaveDocumentTo (NSObject sender)
		{
			Console.WriteLine ("SaveDocumentTo");
			base.SaveDocumentTo (sender);
		}

		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			Console.WriteLine ("ReadFromData");
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

