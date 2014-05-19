
using System;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tomboy
{
	public partial class NoteCollectionItem : NSCollectionViewItem
	{
		public NoteCollectionItem (IntPtr handle) : base (handle) {
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public NoteCollectionItem (NSCoder coder) : base (coder) {
			Initialize ();
		}

		void Initialize () {
		}
	}
}

