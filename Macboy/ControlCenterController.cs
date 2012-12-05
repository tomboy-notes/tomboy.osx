
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class ControlCenterController : MonoMac.AppKit.NSWindowController
	{

		Dictionary <string, Note> notes;
		List <Tags.Tag> tags;
		NSDocumentController _sharedDocumentController;

		#region Constructors
		
		// Called when created from unmanaged code
		public ControlCenterController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public ControlCenterController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public ControlCenterController () : base ("ControlCenter")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
			this.notes = AppDelegate.NoteEngine.GetNotes ();
			//TODO: Tags are not working properly
			this.tags = AppDelegate.NoteEngine.GetTags ();
			Tags.Tag systemTag = new Tags.Tag ("All Notebooks");
			this.tags.Add (systemTag);
			_sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
		}

		#endregion

		#region private methods

		/// <summary>
		/// Handles the text did change in search notes field
		/// </summary>
		/// <param name='obj'>
		/// Object.
		/// </param>
		void HandleTextDidChange(NSNotification obj)
		{
			
			// As per the documentation: 
			//  Use the key "NSFieldEditor" to obtain the field editor from the userInfo 
			//	dictionary of the notification object
			NSTextView textView = (NSTextView)obj.UserInfo.ObjectForKey ((NSString) "NSFieldEditor");
			Console.WriteLine ("DidTextChange {0}", textView.Value);
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			_notesTableView.DataSource = new ControlCenterNotesDataSource (this);
			// handle users doubleClicking on a note in the list of notes
			_notesTableView.DoubleClick += HandleNoteDoubleClick;
			_notebooksTableView.DataSource = new ControlCenterNotebooksDataSource (this.tags);


			// handle search notes
			_searchNotes.Changed += delegate (object sender, EventArgs e) {
				HandleTextDidChange ((NSNotification) sender);
			};
		}

		/// <summary>
		/// Handles the note double click which a Note in the Notes list is Double-clicked causing an open action
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		void HandleNoteDoubleClick (object sender, EventArgs e)
		{
			int selectedRow = _notesTableView.SelectedRow;
			var selectObj = _notesTableView.GetCell (1, selectedRow);

			Console.WriteLine ("Selected value {0}", selectObj.ObjectValue);
			Note note = notes.ElementAt (selectedRow).Value;

			MyDocument myDoc = new MyDocument (note);
			_sharedDocumentController.AddDocument (myDoc);
			myDoc.MakeWindowControllers ();
			myDoc.ShowWindows ();
		}

		partial void NewNoteClicked (NSObject sender)
		{
			_sharedDocumentController.NewDocument (null);
		}

		public Dictionary<string, Note> Notes {
			get {
				return notes;
			}
		}

		#endregion
		
		//strongly typed window accessor
		public new ControlCenter Window {
			get {
				return (ControlCenter)base.Window;
			}
		}
	}
}

