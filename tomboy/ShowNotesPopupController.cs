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
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;

namespace Tomboy
{
	public partial class ShowNotesPopupController : MonoMac.AppKit.NSViewController
	{
		public event EventHandler<NoteEventArgs> NoteNodeClicked;

		public ShowNotesPopupController (IntPtr handle) : base (handle) {
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public ShowNotesPopupController (NSCoder coder) : base (coder) {
			Initialize ();
		}

		public ShowNotesPopupController () : base ("ShowNotesPopup", NSBundle.MainBundle) {
			Initialize ();
		}

		void Initialize () {
			View.CollectionView.ItemPrototype = new NoteCollectionViewItemController ();
			View.CollectionView.Content = NotesToNSObject (AppDelegate.Notes.OrderByDescending( x => x.Value.ChangeDate ).Take (20).ToDictionary(d => d.Key, d => d.Value));
			View.CollectionView.NoteSelected += (s, e) => NoteNodeClicked (s, e);
			//View.CollectionView.UpdateColumnCountBasedOnNumber (3);
		}

		NSObject[] NotesToNSObject (Dictionary<string, Note> notes) {
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

