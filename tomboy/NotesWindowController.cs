//
// NotesWindowController.cs
//
// Author:
//       Jared L Jennings <jared@jaredjennings.org>
//
// Copyright (c) 2012 jjennings
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
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class NotesWindowController : NSWindowController
	{
		List<KeyValuePair<string, Note>> notes;
		List <Tags.Tag> tags;
		NSDocumentController _sharedDocumentController;
        NotebookNamePromptController notebookNamePrompt;

		#region Constructors
		
		// Called when created from unmanaged code
		public NotesWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public NotesWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public NotesWindowController () : base ("NotesWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
			SortNotesIntoOrder (AppDelegate.Notes);
			//TODO: Tags are not working properly
			this.tags = AppDelegate.NoteEngine.GetTags ();
			Tags.Tag systemTag = new Tags.Tag ("All Notebooks");
			this.tags.Add (systemTag);

            if(!AppDelegate.Notebooks.Contains("All Notebooks"))
                AppDelegate.Notebooks.Add("All Notebooks");

			_sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			AppDelegate.NoteEngine.NoteRemoved += HandleNoteRemoved;
			AppDelegate.NoteEngine.NoteAdded += HandleNoteAdded;
			AppDelegate.NoteEngine.NoteUpdated += HandleNoteUpdated;
		}

		public int GetNoteCount ()
		{
			return notes.Count;
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			_notesTableView.DataSource = new NotesWindowNotesDatasource (this);
            _notebooksTableView.DataSource = new NotesWindowNotebooksDataSource (AppDelegate.Notebooks);
			
            HandleNotebookAdded();

			// handle users doubleClicking on a note in the list of notes
			_notesTableView.DoubleClick += HandleNoteDoubleClick;
            _notebooksTableView.DoubleClick += HandleNotebookDoubleClick;
		}

		/// <summary>
		/// Gets the note at elementAt.
		/// </summary>
		/// <returns>
		/// The <see cref="Tomboy.Note"/>.
		/// </returns>
		/// <param name='elementAt'>
		/// Element at.
		/// </param>
		public Note GetNoteAt (int elementAt)
		{
			return notes.ElementAt (elementAt).Value;
		}

        public void UpdateNotesTable()
        {
            Dictionary<string, Note> results = new Dictionary<string, Note>();
            results = AppDelegate.NoteEngine.GetNotesForNotebook(AppDelegate.currentNotebook);
            SortNotesIntoOrder(results);
        }

        public void UpdateNotebooksTable()
        {
            HandleNotebookAdded();
        }

		#endregion

		#region private methods
		
		void HandleNoteUpdated (Note note)
		{
			_notesTableView.ReloadData ();
		}

        void HandleNotebookAdded()
        {
            _notebooksTableView.ReloadData();
        }
		
		void HandleNoteAdded (Note note)
		{
            //_notesTableView.ReloadData ();
		}
		
		void HandleNoteRemoved (Note note)
		{
            int index = notes.FindIndex(f => f.Key == note.Uri);
            if (index != -1)
            {
                notes.RemoveAt(index);
                _notesTableView.ReloadData();
            }
		}

		/// <summary>
		/// Responds to searches in Search Field
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		partial void FindNotes (MonoMac.AppKit.NSSearchField sender)
		{
			SortNotesIntoOrder (AppDelegate.NoteEngine.GetNotes (sender.StringValue, true));
			_notesTableView.ReloadData ();
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
			if (e == null)
				throw new ArgumentNullException ("e");
			if (sender == null)
				throw new ArgumentNullException ("sender");

			int selectedRow = _notesTableView.SelectedRow;
			if (selectedRow == -1) {
				Logger.Debug ("No Note selected in tableview");
				return;
			}

			MyDocument myDoc = new MyDocument (notes.ElementAt (selectedRow).Value);
			_sharedDocumentController.AddDocument (myDoc);
			myDoc.MakeWindowControllers ();
			myDoc.ShowWindows ();
		}

        void HandleNotebookDoubleClick (object sender, EventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (sender == null)
                throw new ArgumentNullException("sender");

            int selectedRow = _notebooksTableView.SelectedRow;
            Console.WriteLine("The selected row number is " + selectedRow + " and value is " + AppDelegate.Notebooks.ElementAt(selectedRow));

            AppDelegate.currentNotebook = AppDelegate.Notebooks.ElementAt(selectedRow);
            Console.WriteLine("The notebook name is " + AppDelegate.currentNotebook);
            Dictionary<string, Note> results = new Dictionary<string, Note>();

            results = AppDelegate.NoteEngine.GetNotesForNotebook(AppDelegate.currentNotebook);
            SortNotesIntoOrder(results);

            _notesTableView.ReloadData ();

        }

		partial void NewNoteClicked (NSObject sender)
		{
			_sharedDocumentController.NewDocument (null);
			//FIXME: Should insert data into tableview maybe instead or reloading the whole view?
			_notesTableView.ReloadData ();
		}

        partial void newNotebookButton (NSObject sender)
        {

            notebookNamePrompt = new NotebookNamePromptController();
            notebookNamePrompt.Window.MakeKeyAndOrderFront(this);

        }
            
		/// <summary>
		/// Sorts the notes into order.
		/// </summary>
		/// <description>This should be called when loading notes or anything that interacts with the 
		/// local notes list</description>
		/// <param name='notes'>
		/// Notes.
		/// </param>
		void SortNotesIntoOrder (Dictionary <string, Note> notes)
		{
			this.notes = notes.ToList ().OrderByDescending(x => x.Value.ChangeDate).ToList();
		}

		#endregion
		
		//strongly typed window accessor
		public new NotesWindow Window {
			get {
				return (NotesWindow)base.Window;
			}
		}
	}
}

