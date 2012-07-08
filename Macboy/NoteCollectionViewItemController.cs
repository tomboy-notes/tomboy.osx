
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;

namespace Tomboy
{
	public partial class NoteCollectionViewItemController : NSCollectionViewItem
	{
		public NoteCollectionViewItemController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		[Export ("initWithCoder:")]
		public NoteCollectionViewItemController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public NoteCollectionViewItemController () : base ("NoteCollectionViewItem", NSBundle.MainBundle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			TextField = View.TextField;
			ContentTextView = View.ContentTextView;
			View.ContentTextView.NextResponder = View; 
		}

		public new NoteCollectionViewItem View {
			get {
				return (NoteCollectionViewItem)base.View;
			}
		}

		public NSTextView ContentTextView {
			get;
			private set;
		}
	}
}

