using System;

using MonoMac.AppKit;
using MonoMac.Foundation;

using Tomboy;

namespace Tomboy
{
	interface ICocoaNoteAdapter
	{
		NSString Title { get; }
		NSString HtmlContent { get; }
	}

	public class CocoaNoteAdapter : NSObject, ICocoaNoteAdapter
	{
		public Note Note {
			get;
			set;
		}

		public NSString Title {
			get {
				return new NSString (Note == null ? "(Untitled)" : Note.Title);
			}
		}

		public NSString HtmlContent {
			get {
				return new NSString (Note == null ? "(empty note)" : Note.Text);
			}
		}
	}
}

