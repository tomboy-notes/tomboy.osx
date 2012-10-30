
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class ControlCenterController : MonoMac.AppKit.NSWindowController
	{

		private Dictionary <string, Note> notes;
		private List <Tags.Tag> tags;

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
			this.tags = AppDelegate.NoteEngine.GetTags ();
			Tags.Tag systemTag = new Tags.Tag ("All Notebooks");
			this.tags.Add (systemTag);
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			_notesTableView.DataSource = new ControlCenterNotesDataSource (notes);
			_notebooksTableView.DataSource = new ControlCenterNotebooksDataSource (this.tags);
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

