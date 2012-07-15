
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy;

namespace Tomboy
{
	public partial class ShowNotesPopupController : MonoMac.AppKit.NSViewController
	{
		public event EventHandler<NoteEventArgs> NoteNodeClicked;

		public ShowNotesPopupController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public ShowNotesPopupController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public ShowNotesPopupController () : base ("ShowNotesPopup", NSBundle.MainBundle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			View.CollectionView.ItemPrototype = new NoteCollectionViewItemController ();
			View.CollectionView.Content = NotesToNSObject (AppDelegate.Notes.OrderByDescending( x => x.Value.ChangeDate ).Take (20).ToDictionary(d => d.Key, d => d.Value));
			View.CollectionView.NoteSelected += (s, e) => NoteNodeClicked (s, e);
			//View.CollectionView.UpdateColumnCountBasedOnNumber (3);
		}

		NSObject[] NotesToNSObject (Dictionary<string, Note> notes)
		{
			return notes.Values
				.Select (n => (NSObject)new CocoaNoteAdapter () { Note = n })
				.ToArray ();
		}
		
		public new ShowNotesPopup View {
			get {
				return (ShowNotesPopup)base.View;
			}
		}
	}
}

