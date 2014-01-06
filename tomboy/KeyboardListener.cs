using System;


namespace Tomboy
{
	public class KeyboardListener : MonoMac.WebKit.DomEventListener
	{
		public KeyboardListener ()
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