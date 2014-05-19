// Author:
//       Jared Jennings <jjennings@gnome.org>
//       Jérémie Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2012 Jared Jennings 2012
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
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
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

		public NoteCollectionView () : base () {
			Initialize ();
		}

		public NoteCollectionView (IntPtr ptr) : base (ptr) {
			Initialize ();
		}

		void Initialize () {
			Selectable = true;

			//UpdateColumnCountBasedOnNumber (3);
		}

		public override NSCollectionViewItem NewItemForRepresentedObject (NSObject obj) {
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

		public void UpdateColumnCountBasedOnNumber (int number) {

			switch (number) {
				case 1:
					MaxNumberOfColumns = 5;
					break;
				case 2:
					MaxNumberOfColumns = 4;
					break;
				case 3:
					MaxNumberOfColumns = 3;
					break;
				case 4:
					MaxNumberOfColumns = 2;
					break;
				case 5:
					MaxNumberOfColumns = 1;
					break;
			}
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