using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy;

namespace Macboy
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();

		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		
		#endregion
		
		#region Actions
		partial void NewNoteButton_Clicked (MonoMac.AppKit.NSButton sender)
		{
			
		}
		
		
		#endregion Actions
		
		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}
		
		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib ()
		{
			Console.WriteLine ("awakeFromNib:");
			tblNotes.Source = new TableNotesDataSource (tblNotes);
			TableNotesDataSource.SelectedNoteChanged += delegate(Note note) {
				Console.WriteLine ("YES I KNOW IT CHANGED NOW {0}", tblNotes.SelectedRow);
				tvNoteBody.InsertText ((NSString)note.Text);
			};
		}	
	}
}

