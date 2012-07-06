using System;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MacSuperBoy
{
	public class NoteEventArgs : EventArgs
	{
		public string NoteId { get; set; }
	}

	[Register ("NoteCollectionView")]
	public class NoteCollectionView : NSCollectionView
	{
		NoteCollectionViewItem previousSelection;
		public event EventHandler<NoteEventArgs> NoteSelected;

		public NoteCollectionView () : base ()
		{
			Initialize ();
		}

		public NoteCollectionView (IntPtr ptr) : base (ptr)
		{
			Initialize ();
		}

		void Initialize ()
		{
			Selectable = true;

			//UpdateColumnCountBasedOnNumber (3);
		}

		public override NSCollectionViewItem NewItemForRepresentedObject (NSObject obj)
		{
			var item = base.NewItemForRepresentedObject (obj) as NoteCollectionViewItemController;
			item.RepresentedObject = obj;

			var dummyNote = obj as CocoaNoteAdapter;
			//item.ImageView.Image = dummyNote.Snapshot;
			item.TextField.StringValue = dummyNote.Title;
			NSDictionary attrDict;
			var attrString = new NSAttributedString (NSData.FromString (dummyNote.HtmlContent),
			                                         new NSUrl ("http://foo.com"),
			                                         out attrDict);
			item.ContentTextView.TextStorage.SetString (attrString);
			return item;
		}

		public void UpdateColumnCountBasedOnNumber (int number)
		{
			MaxNumberOfColumns = number;
			var viewSize = FittingSize;
			var width = viewSize.Width / MaxNumberOfColumns;
			MinItemSize = new SizeF (width, MinItemSize.Height);
			MaxItemSize = new SizeF (width, MaxItemSize.Height);
		}

		public override NSIndexSet SelectionIndexes {
			get {
				return base.SelectionIndexes;
			}
			set {
				if (previousSelection != null)
					previousSelection.Selected = false;
				var selectedItem = ItemAtIndex ((int)value.FirstIndex);
				var viewItem = ((NoteCollectionViewItem)selectedItem.View);
				viewItem.Selected = true;
				previousSelection = viewItem;
				base.SelectionIndexes = value;
				if (NoteSelected != null)
					NoteSelected (this, new NoteEventArgs {
						NoteId = ((CocoaNoteAdapter)selectedItem.RepresentedObject).Note.Uri
					});
			}
		}
	}
}

