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

		void LoadNote (string newNoteId, bool withHistory = true)
		{
			LoadingFromString = true;
			Logger.Info ("Trying to load note {0}", newNoteId);
			var note = AppDelegate.Notes[newNoteId];
			if (note == null)
				return;
			currentNote = note;
			currentNoteID = newNoteId;
			InvalidateRestorableState ();
			noteWebView.MainFrame.LoadHtmlString (note.Text.Replace (Environment.NewLine, "<br />"),
			                                      new NSUrl (AppDelegate.BaseUrlPath));
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
			NoteLegacyTranslator translator = new NoteLegacyTranslator ();
			var element = noteWebView.MainFrame.DomDocument.GetElementsByTagName ("body");
			DomHtmlElement body = (DomHtmlElement)element.First ();

			Console.WriteLine ("Note ID {0}", currentNoteID);
			Console.WriteLine ("Note Content {0}", body.InnerHTML);
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
			LoadNote (AppDelegate.NoteEngine.GetNote (item.Title).Uri, false);
		}

		partial void ShowNotes (NSObject sender)
		{
			using (NSPopover popover = new NSPopover ()) {
				var controller = new ShowNotesPopupController ();
				controller.NoteNodeClicked += (s, e) => LoadNote (e.NoteId);
				popover.Behavior = NSPopoverBehavior.Transient;
				popover.ContentViewController = controller;
				popover.Show (RectangleF.Empty, sender as NSView, NSRectEdge.MaxYEdge);
			}
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
			Logger.Info ("Saving Note: {0}", currentNote.Title);
			SaveData ();
			//AppDelegate.NoteEngine.SaveNote (currentNote);
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

