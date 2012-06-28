//
//  TableSourceNotes.cs
//
//  Author:
//       jjennings <jjennings@gnome.org>
//
//  Copyright (c) 2012 jjennings
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Macboy
{
	public class TableNotesDataSource : NSTableViewDataSource
	{
		public TableNotesDataSource ()
		{
			
		}
		
		
		// This method will be called by the NSTableView control to learn the number of rows to display.
		[Export ("numberOfRowsInTableView:")]
		public int NumberOfRowsInTableView (NSTableView table)
		{
			// We just return a static 2. We will have two rows.
			return 2;
		}
		
		/// <summary>
		/// Gets the object value. Hopefully this will handle the Notes into the view.
		/// </summary>
		/// <returns>
		/// The object value.
		/// </returns>
		/// <param name='tableView'>
		/// Table view.
		/// </param>
		/// <param name='tableColumn'>
		/// Table column.
		/// </param>
		/// <param name='row'>
		/// Row.
		/// </param>
		public override MonoMac.Foundation.NSObject GetObjectValue (MonoMac.AppKit.NSTableView tableView, 
		                                                            MonoMac.AppKit.NSTableColumn tableColumn, 
		                                                            int row)
		{
			//noteTitle
			NSString valueKey = null;
			if (tableColumn.Identifier != null)
				valueKey = (NSString)tableColumn.Identifier.ToString ();
			System.Console.WriteLine ("valueKey:{0}", valueKey);
							
			switch (valueKey)
			{
				case "noteTitle":
					return (NSString)"My Note";
				case "colNoteModifiedDate":
					return (NSString)"2012-06-28";
				
			}
			throw new System.Exception(string.Format("Incorrect value requested '{0}'", valueKey));
		}
		
		
		
		
		// This method will be called by the control for each column and each row.
		[Export ("tableView:objectValueForTableColumn:row:")]
		public NSObject ObjectValueForTableColumn (NSTableView table, NSTableColumn col, int row)
		{
			
			switch (row) {
			case 0:
                // We will write "Hello" in the first row...
				return new NSString ("Hello");
			case 1:
                // ...and "World" in the second.
				return new NSString ("World");
			// Note that NSTableView requires an NSString, which we create with new NSString("bla").
			default:
                // We need a default value.
				return null;
			}
		}       	        
	}
}

