//
//  DomDocumentListener.cs
//
//  Author:
//       Jared Jennings <jjennings@gnome.org>
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
using System;

namespace Macboy
{
	public class DomDocumentListener : MonoMac.WebKit.DomEventListener
	{
		public DomDocumentListener ()
		{
			timeoutvalue = DateTime.Now.AddSeconds(_time_out_value);
		}

		#region Fields
		/// <summary>
		/// The _time_out_value for how often we save notes
		/// </summary>
		private long _time_out_value = 5;
		/// <summary>
		/// The timeoutvalue.for how often we wait before we save changes
		/// </summary>
		private DateTime timeoutvalue;

		#endregion Fields

		#region Delegates
		public delegate void NoteContentChangedEventHandler ();
		
		#endregion Delegates
		
		#region Events
		public static event NoteContentChangedEventHandler NoteContentChanged;
		#endregion Events

		public override void HandleEvent (MonoMac.WebKit.DomEvent evt)
		{
			if (DateTime.Now > timeoutvalue) {
				// set new timeout
				timeoutvalue = DateTime.Now.AddSeconds (_time_out_value);

				//event received from keypress
				if (NoteContentChanged != null)
					NoteContentChanged ();
			}
		}
	}
}

