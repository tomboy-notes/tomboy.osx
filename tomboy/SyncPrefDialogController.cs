//
// SyncPrefDialogController.cs
//
// Author:
//       Rashid Khan <rashood.khan@gmail.com>
//
// Copyright (c) 2014 Rashid Khan
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
using MonoMac.Foundation;
using MonoMac.AppKit;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

using Tomboy;
using Tomboy.OAuth;
using Tomboy.Sync;
using Tomboy.Sync.Filesystem;
using Tomboy.Sync.Web;

using System.Security.Cryptography.X509Certificates;

namespace Tomboy
{
	public partial class SyncPrefDialogController : MonoMac.AppKit.NSWindowController
    	{

        	#region Constructors

        	// Called when created from unmanaged code
		public SyncPrefDialogController(IntPtr handle) : base (handle) {
            		Initialize();
        	}
        	// Called when created directly from a XIB file
        	[Export("initWithCoder:")]
		public SyncPrefDialogController(NSCoder coder) : base (coder) {
            		Initialize();
        	}
        	// Call to load from the XIB/NIB file
		public SyncPrefDialogController() : base ("SyncPrefDialog") {
            		Initialize();
        	}
        	// Shared initialization code
		void Initialize () {
        	}

        	#endregion

		partial void SetSyncPath(NSObject sender) {
			bool webSync = false;

			if (!String.IsNullOrEmpty (AppDelegate.settings.webSyncURL) || !String.IsNullOrWhiteSpace (AppDelegate.settings.webSyncURL)) {
				webSync = true;
				NSAlert syncWarning = new NSAlert() {
					MessageText = "Web Sync Found",
					InformativeText = "Setting the File System Sync Path will override the Web Sync Authorization",
					AlertStyle = NSAlertStyle.Informational
				};
				syncWarning.AddButton ("OK");
				syncWarning.BeginSheet (this.Window,this,null,IntPtr.Zero);
			}

            		var openPanel = new NSOpenPanel();
            		openPanel.ReleasedWhenClosed = true;
            		openPanel.CanChooseDirectories = true;
            		openPanel.CanChooseFiles = false;
            		openPanel.CanCreateDirectories = true;
            		openPanel.Prompt = "Select Directory";

            		var result = openPanel.RunModal();
            		if (result == 1) {
                		SyncPathTextField.Cell.Title = openPanel.DirectoryUrl.Path;
				//AppDelegate.FilesystemSyncPath = openPanel.DirectoryUrl.Path;

				AppDelegate.settings.syncURL = openPanel.DirectoryUrl.Path;


                		NSAlert alert = new NSAlert () {
                    			MessageText = "File System Sync",
                    			InformativeText = "File System Sync path has been set at:\n"+AppDelegate.settings.syncURL,
                    			AlertStyle = NSAlertStyle.Warning
                		};
                		alert.AddButton ("OK");
                		alert.BeginSheet (this.Window,
                    			this,
                    			null,
					IntPtr.Zero);

				if (webSync) {
					AppDelegate.settings.webSyncURL = String.Empty;
					AppDelegate.settings.token = String.Empty;
					AppDelegate.settings.secret = String.Empty;
				}

				try {
					if (AppDelegate.settings != null){
						SettingsSync.Write(AppDelegate.settings);
						Console.WriteLine ("WRITTEN PROPERLY");
					}
				} catch (NullReferenceException ex) {
					Console.WriteLine ("ERROR - "+ex.Message);
				}
			}
        	}

		partial void SetExportNotesPath (NSButton sender) {
            		var openPanel = new NSOpenPanel();
            		openPanel.ReleasedWhenClosed = true;
            		openPanel.CanChooseDirectories = true;
            		openPanel.CanChooseFiles = false;
            		openPanel.CanCreateDirectories = false;
            		openPanel.Prompt = "Select Existing Notes Directory";

            		var result = openPanel.RunModal();
			if (result == 1) {
                		ExportPathTextField.Cell.Title = openPanel.DirectoryUrl.Path;
            		}

        	}

		partial void ExportNotesAction(NSObject sender) {
            		if (ExportPathTextField.StringValue != null){
                		string rootDirectory = ExportPathTextField.StringValue;
				ExportNotes.Export(rootDirectory, AppDelegate.NoteEngine);

                	NSAlert alert = new NSAlert () {
                    		MessageText = "Note Imported",
                    		InformativeText = "All the notes have been imported to local storage. Please restart the Tomboy to see your old notes",
                    		AlertStyle = NSAlertStyle.Warning
                	};
                	alert.AddButton ("OK");
                	alert.BeginSheet (this.Window,
                    		this,
                    		null,
                    		IntPtr.Zero);
            		}

        	}
           
        	// This method will be called automatically when the main window "wakes up".
        	[Export ("awakeFromNib:")]
		public override void AwakeFromNib() {
            		// set according to AppDelegate, which later should be a preference.
            		// FIXME: Turn into a system setting.
			if (AppDelegate.settings.syncURL != null)
				SyncPathTextField.StringValue = AppDelegate.settings.syncURL;
			EnableAutoSyncing.Enabled = true;
			//Console.WriteLine(AppDelegate.settings.autoSync.ToString());
        	}

		partial void EnableAutoSyncingAction (NSObject sender) {
            		if (EnableAutoSyncing.Enabled)
				AppDelegate.settings.autoSync = true;
            		else
				AppDelegate.settings.autoSync = false;

			SettingsSync.Write(AppDelegate.settings);
			//Console.WriteLine("Enabled Auto Sync - ");
			//Console.WriteLine(AppDelegate.settings.autoSync.ToString());
        	}

		partial void StartFileSync(NSObject sender) {
            		Console.WriteLine ("FAKE SYNC");
        	}

		partial void Authenticate (NSObject sender) {

			if (!String.IsNullOrEmpty (AppDelegate.settings.syncURL) || !String.IsNullOrWhiteSpace (AppDelegate.settings.syncURL)) {
				NSAlert alert = new NSAlert () {
					MessageText = "File System Sync Found",
					InformativeText = "The File System Sync option would be overriden with Rainy Web Sync.",
					AlertStyle = NSAlertStyle.Warning
				};
				alert.AddButton ("Override File System Sync");
				alert.AddButton ("Cancel");
				alert.BeginSheet (this.Window,
					this,
					new MonoMac.ObjCRuntime.Selector ("alertDidEnd:returnCode:contextInfo:"),
					IntPtr.Zero);

				AppDelegate.settings.syncURL = "";
				SettingsSync.Write (AppDelegate.settings);
			} else {
				SyncPrefDialogController.AuthorizeAction (this.Window, SyncURL.StringValue);
			}
		}

		[Export ("alertDidEnd:returnCode:contextInfo:")]
		void AlertDidEnd (NSAlert alert, int returnCode, IntPtr contextInfo) {
			if (alert == null)
				throw new ArgumentNullException("alert");
			if (((NSAlertButtonReturn)returnCode) == NSAlertButtonReturn.First) {
				AppDelegate.settings.syncURL = String.Empty;
				SettingsSync.Write (AppDelegate.settings);
				SyncPrefDialogController.AuthorizeAction (this.Window, SyncURL.StringValue);
			}
		}

		public static void AuthorizeAction (NSWindow window, String serverURL) {

			if (String.IsNullOrEmpty (serverURL) || String.IsNullOrWhiteSpace (serverURL)) {
				NSAlert alert = new NSAlert () {
					MessageText = "Incorrect URL",
					InformativeText = "The Sync URL cannot be empty",
					AlertStyle = NSAlertStyle.Warning
				};
				alert.AddButton ("OK");
				alert.BeginSheet (window,
					null,
					null,
					IntPtr.Zero);

				//SyncURL.StringValue = "";
				return;

			} else {
				if (!serverURL.EndsWith ("/"))
					serverURL += "/";
			}

			HttpListener listener = new HttpListener ();
			string callbackURL = "http://localhost:9001/";
			listener.Prefixes.Add (callbackURL);
			listener.Start ();

			var callback_delegate = new OAuthAuthorizationCallback ( url => {
				Process.Start (url);

				// wait (block) until the HttpListener has received a request 
				var context = listener.GetContext ();

				// if we reach here the authentication has most likely been successfull and we have the
				// oauth_identifier in the request url query as a query parameter
				var request_url = context.Request.Url;
				string oauth_verifier = System.Web.HttpUtility.ParseQueryString (request_url.Query).Get("oauth_verifier");

				if (string.IsNullOrEmpty (oauth_verifier)) {
					// authentication failed or error
					context.Response.StatusCode = 500;
					context.Response.StatusDescription = "Error";
					context.Response.Close();
					throw new ArgumentException ("oauth_verifier");
				} else {
					// authentication successfull
					context.Response.StatusCode = 200;
					using (var writer = new StreamWriter (context.Response.OutputStream)) {
						writer.WriteLine("<h1>Authorization successfull!</h1>Go back to the Tomboy application window.");
					}
					context.Response.Close();
					return oauth_verifier;
				}
			});

			try{
				//FIXME: see http://mono-project.com/UsingTrustedRootsRespectfully for SSL warning
				ServicePointManager.CertificatePolicy = new DummyCertificateManager ();

				IOAuthToken access_token = WebSyncServer.PerformTokenExchange (serverURL, callbackURL, callback_delegate);

				AppDelegate.settings.webSyncURL = serverURL;
				AppDelegate.settings.token = access_token.Token;
				AppDelegate.settings.secret = access_token.Secret;

				SettingsSync.Write (AppDelegate.settings);

				Console.WriteLine ("Received token {0} with secret key {1}",access_token.Token, access_token.Secret);

				listener.Stop ();

				NSAlert success = new NSAlert () {
					MessageText = "Authentication Successful",
					InformativeText = "The authentication with the server has been successful. You can sync with the web server now.",
					AlertStyle = NSAlertStyle.Informational
				};
				success.AddButton ("OK");
				success.BeginSheet (window,
					window,
					null,
					IntPtr.Zero);
				return;
			} catch (Exception ex) {

				if (ex is WebException || ex is System.Runtime.Serialization.SerializationException) {

					NSAlert alert = new NSAlert () {
						MessageText = "Incorrect URL",
						InformativeText = "The URL entered " + serverURL + " is not valid for syncing",
						AlertStyle = NSAlertStyle.Warning
					};
					alert.AddButton ("OK");
					alert.BeginSheet (window,
						null,
						null,
						IntPtr.Zero);

					listener.Abort ();

					return;
				} else {
					NSAlert alert = new NSAlert () {
						MessageText = "Some Issue has occured",
						InformativeText = "Something does not seem right!",
						AlertStyle = NSAlertStyle.Warning
					};
					alert.AddButton ("OK");
					alert.BeginSheet (window,
						null,
						null,
						IntPtr.Zero);

					listener.Abort ();
				}
			}
		}

        	//strongly typed window accessor
		public new SyncPrefDialog Window {
			get {
                		return (SyncPrefDialog)base.Window;
            		}
        	}
    	}

	public class DummyCertificateManager : ICertificatePolicy
	{
		public bool CheckValidationResult (ServicePoint sp, X509Certificate certificate, WebRequest request, int error) {
			return true;
		}
	}
}

