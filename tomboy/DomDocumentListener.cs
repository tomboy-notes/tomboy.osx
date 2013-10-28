//
// LicenseHeader.cs
//
// Author:
//       Jared Jennings <jjennings@gnome.org>
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

namespace tomboy
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

