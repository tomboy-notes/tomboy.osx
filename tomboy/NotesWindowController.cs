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
		NotebookEditPromptController notebookEditPrompt;

		#region Constructors
		
		// Called when created from unmanaged code
		public NotesWindowController (IntPtr handle) : base (handle) {
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public NotesWindowController (NSCoder coder) : base (coder) {
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public NotesWindowController () : base ("NotesWindow") {
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize () {
			SortNotesIntoOrder (AppDelegate.Notes);
			//TODO: Tags are not working properly
			this.tags = AppDelegate.NoteEngine.GetTags ();
			Tags.Tag systemTag = new Tags.Tag ("All Notebooks");
			this.tags.Add (systemTag);

			_sharedDocumentController = (NSDocumentController)NSDocumentController.SharedDocumentController;
			AppDelegate.NoteEngine.NoteRemoved += HandleNoteRemoved;
			AppDelegate.NoteEngine.NoteAdded += HandleNoteAdded;
			AppDelegate.NoteEngine.NoteUpdated += HandleNoteUpdated;
		}

		public int GetNoteCount () {
			return notes.Count;
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib () {
			_notesTableView.DataSource = new NotesWindowNotesDatasource (this);
            		_notebooksTableView.DataSource = new NotesWindowNotebooksDataSource (AppDelegate.Notebooks);
			_notebooksTableView.SelectRow (0, false);
			_notebooksTableView.Delegate = new NotesWindowNotebooksViewDelegate (_notebooksTableView);
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
		public Note GetNoteAt (int elementAt) {

			if (elementAt < notes.Count)
				return notes.ElementAt (elementAt).Value;
			else
				return null;

		}

		public void UpdateNotesTable() {
			_notesTableView.ReloadData ();
            		Dictionary<string, Note> results = new Dictionary<string, Note>();
            		results = AppDelegate.NoteEngine.GetNotesForNotebook(AppDelegate.currentNotebook);
            		SortNotesIntoOrder(results);
        	}

		public void UpdateNotebooksTable() {
			Console.WriteLine("Current notebook index is "+AppDelegate.Notebooks.IndexOf(AppDelegate.currentNotebook));
			_notebooksTableView.SelectRow (AppDelegate.Notebooks.IndexOf(AppDelegate.currentNotebook), false);
			HandleNotebookAdded ();
        	}

		public void UpdateTableSingleClick () {
			Dictionary<string, Note> results = new Dictionary<string, Note> ();
			results = AppDelegate.NoteEngine.GetNotesForNotebook (AppDelegate.currentNotebook);

			SortNotesIntoOrder (results);
			_notesTableView.ReloadData ();
		}


		#endregion

		#region private methods
		
		void HandleNoteUpdated (Note note) {
			_notesTableView.ReloadData ();
		}

		void HandleNotebookAdded() {
            		_notebooksTableView.ReloadData();
        	}
		
		void HandleNoteAdded (Note note) {
            		//_notesTableView.ReloadData ();
		}
		
		void HandleNoteRemoved (Note note) {
            		int index = notes.FindIndex(f => f.Key == note.Uri);
			if (index != -1) {
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
		partial void FindNotes (MonoMac.AppKit.NSSearchField sender) {
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
		void HandleNoteDoubleClick (object sender, EventArgs e) {
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

		void HandleNotebookDoubleClick (object sender, EventArgs e) {
            		if (e == null)
                		throw new ArgumentNullException("e");
            		if (sender == null)
                		throw new ArgumentNullException("sender");

			if (_notebooksTableView.SelectedRow == 0) {
				NSAlert alert = new NSAlert () {
					MessageText = "Notebook Cannot Be Edited",
					InformativeText = "You cannot edit 'All Notebooks' selection.",
					AlertStyle = NSAlertStyle.Warning
				};
				alert.AddButton ("OK");
				alert.BeginSheet (this.Window,
					this,
					null,
					IntPtr.Zero);	
			} else {
				notebookEditPrompt = new NotebookEditPromptController ();
				notebookEditPrompt.Window.MakeKeyAndOrderFront (this);
			}
        	}

		partial void NewNoteClicked (NSObject sender) {
			_sharedDocumentController.NewDocument (null);
			//FIXME: Should insert data into tableview maybe instead or reloading the whole view?
			_notesTableView.ReloadData ();
		}

		partial void newNotebookButton (NSObject sender) {

            		notebookNamePrompt = new NotebookNamePromptController();
            		notebookNamePrompt.Window.MakeKeyAndOrderFront(this);

  		}

		partial void RemoveNotebook (NSObject sender) {
			int selectedRow = _notebooksTableView.SelectedRow;
			if(selectedRow == 0) {
				//Cannot delete the All Notebooks Row
				NSAlert alert = new NSAlert () {
					MessageText = "Notebook Cannot Be Deleted",
					InformativeText = "You cannot delete 'All Notebooks' selection.",
					AlertStyle = NSAlertStyle.Warning
				};
				alert.AddButton ("OK");
				alert.BeginSheet (this.Window,
					this,
					null,
					IntPtr.Zero);
			} else if(selectedRow != -1) {
				NSAlert alert = new NSAlert () {
					MessageText = "Really delete notebook?",
					InformativeText = "You are about to delete Notebook "
						+ AppDelegate.Notebooks.ElementAt(selectedRow) 
						+ ". This operation could not be un-done.",
					AlertStyle = NSAlertStyle.Warning
				};
				alert.AddButton ("OK");
				alert.AddButton ("Cancel");
				alert.BeginSheet (this.Window,
					this,
					new MonoMac.ObjCRuntime.Selector ("alertDidEnd:returnCode:contextInfo:"),
					IntPtr.Zero);
			}
		}

		[Export ("alertDidEnd:returnCode:contextInfo:")]
		void AlertDidEnd (NSAlert alert, int returnCode, IntPtr contextInfo) {
			if (alert == null)
				throw new ArgumentNullException("alert");
			if (((NSAlertButtonReturn)returnCode) == NSAlertButtonReturn.First) {
				int selectedRow = _notebooksTableView.SelectedRow;

				//Deleting selected notebook should move all the notes to All Notebooks tab
				Dictionary <string, Note> result = new Dictionary<string, Note>();
				result = AppDelegate.NoteEngine.GetNotesForNotebook (AppDelegate.Notebooks.ElementAt(selectedRow));

				foreach(KeyValuePair<string,Note> note in result) {
					note.Value.RemoveNotebook ();
					AppDelegate.NoteEngine.SaveNote(note.Value);
				}
					
				AppDelegate.Notebooks.Remove (AppDelegate.currentNotebook);
				AppDelegate.currentNotebook = AppDelegate.Notebooks.ElementAt (0);
				Dictionary<string, Note> allNotes = AppDelegate.NoteEngine.GetNotesForNotebook (AppDelegate.currentNotebook);
				_notebooksTableView.SelectRow (0, false);
				SortNotesIntoOrder (allNotes);
				_notesTableView.ReloadData ();
				_notebooksTableView.ReloadData ();


			}
		}

		partial void EditNotebook (NSObject sender) {
			notebookEditPrompt = new NotebookEditPromptController ();
			notebookEditPrompt.Window.MakeKeyAndOrderFront (this);
		}
            
		/// <summary>
		/// Sorts the notes into order.
		/// </summary>
		/// <description>This should be called when loading notes or anything that interacts with the 
		/// local notes list</description>
		/// <param name='notes'>
		/// Notes.
		/// </param>
		void SortNotesIntoOrder (Dictionary <string, Note> notes) {
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