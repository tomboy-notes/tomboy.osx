
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
		/// Responds to searches in Search Field
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		partial void FindNotes (MonoMac.AppKit.NSSearchField sender)
		{
			this.notes = AppDelegate.NoteEngine.GetNotes (sender.StringValue, true);
			_notesTableView.ReloadData ();
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			_notesTableView.DataSource = new ControlCenterNotesDataSource (this);
			_notebooksTableView.DataSource = new ControlCenterNotebooksDataSource (this.tags);

			// handle users doubleClicking on a note in the list of notes
			_notesTableView.DoubleClick += HandleNoteDoubleClick;
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
			Note note = notes.ElementAt (selectedRow).Value;
			MyDocument myDoc = new MyDocument (note);
			_sharedDocumentController.AddDocument (myDoc);
			myDoc.MakeWindowControllers ();
			myDoc.ShowWindows ();
		}

		partial void NewNoteClicked (NSObject sender)
		{
			_sharedDocumentController.NewDocument (null);
			//FIXME: Should insert data into tableview maybe instead or reloading the whole view?
			_notesTableView.ReloadData ();
		}

		public Dictionary<string, Note> Notes {
			get {
				return this.notes;
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

