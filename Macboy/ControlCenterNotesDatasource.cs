//
// ControlCenterNotesDatasource.cs
//
// Author:
//       Jared L Jennings <jared@jaredjennings.org>
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
using MonoMac.AppKit;

namespace Tomboy
{
	[Register ("ControlCenterNotesDataSource")]
	public class ControlCenterNotesDataSource : NSTableViewDataSource
	{
		// should represent the Tomboy image, but this isn't loading correctly
		private NSImage image = new NSImage ("Tomboy.tomboy.icns");

		ControlCenterController MyControlCenterController {
			get;
			set;
		}

		public ControlCenterNotesDataSource ()
		{
		}

		public ControlCenterNotesDataSource (ControlCenterController controller)
		{
			if (controller == null)
				throw new ArgumentNullException ("controller");
			MyControlCenterController = controller;
		}

		// This method will be called by the NSTableView control to learn the number of rows to display.
		[Export ("numberOfRowsInTableView:")]
		public int NumberOfRowsInTableView(NSTableView table)
		{
			return MyControlCenterController == null ? 0 : MyControlCenterController.Notes.Count;
		}

		// This method will be called by the control for each column and each row.
		[Export ("tableView:objectValueForTableColumn:row:")]
		public NSObject ObjectValueForTableColumn(NSTableView table, NSTableColumn col, int row)
		{
			// Get the current row index
			var note_at = MyControlCenterController.Notes.Keys.ElementAt (row);
			var colKey = (NSString)col.Identifier.ToString ();
			switch (colKey) {
			case "_notesImageColumn":
				return image; //FIXME: This is not working. An image is not being returned.
			case "modifiedDate":
				return (NSString)MyControlCenterController.Notes[note_at].ChangeDate.ToShortDateString ();
			case "title":
				return (NSString)MyControlCenterController.Notes[note_at].Title;
			default:
				return null;
			}
		}
	}
}

