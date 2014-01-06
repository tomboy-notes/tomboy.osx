
using System;
using MonoMac.Foundation;

namespace Tomboy
{
	public partial class DashboardWindow : MonoMac.AppKit.NSWindow
	{

		#region Constructors
		
		// Called when created from unmanaged code
		public DashboardWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public DashboardWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}

		// This method will be called automatically when the main window "wakes up".
		[Export ("awakeFromNib:")]
		public override void AwakeFromNib()
		{
			Console.WriteLine("awakeFromNib:");
			//_NotesTableView.DataSource = new DashboardNotesTableViewDataSource();
		}
		
		#endregion
	}
}

