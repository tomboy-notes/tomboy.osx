
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace MacSuperBoy
{
	public partial class NoteCollectionItem : NSCollectionViewItem
	{
		public NoteCollectionItem (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public NoteCollectionItem (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
	}
}

