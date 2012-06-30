using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using Tomboy;

namespace Macboy
{
	class MainClass
	{
		/// <summary>
		/// The tomboy engine.
		/// </summary>
		private static Engine tomboyEngine;

		#region Delegates
		public delegate void NewNoteEventHandler (Note note);
		
		#endregion Delegates
		
		#region Events
		public static event NewNoteEventHandler NewNote;
		#endregion Events
			
		public static Engine GetEngine ()
		{
			return tomboyEngine;
		}
		
		static void Main (string [] args)
		{
			DiskStorage.Instance.SetPath ("/Users/jjennings/Library/Application Support/Tomboy");
			tomboyEngine = new Engine (DiskStorage.Instance);		
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}	

