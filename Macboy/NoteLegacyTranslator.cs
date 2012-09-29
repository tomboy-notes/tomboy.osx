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
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.WebKit;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Tomboy
{
	/// <summary>
	/// Note legacy translator which handles text formating between Pango and othe formats.
	/// </summary>
	public class NoteLegacyTranslator
	{
		private XslCompiledTransform xslTransformTo;
		private XslCompiledTransform xslTransformFrom;
		private Assembly _assembly;
		private const string _xsl_transform_to = "Tomboy.Tomboy.transform_to_note.xsl";
		private const string _xsl_transform_from = "Tomboy.Tomboy.transform_from_note.xsl";
		private string _style_sheet_location = "";

		public NoteLegacyTranslator ()
		{
			/* The order of the following methods matter */
			GetAssembly ();
			LoadPaths ();
			// if the stylesheets don't exist, copy them to the hd.
			CopyXSLT (_xsl_transform_to);
			CopyXSLT (_xsl_transform_from);

			if (xslTransformTo == null) {
				xslTransformTo = new XslCompiledTransform (true);
				xslTransformTo.Load (Path.Combine (_style_sheet_location, _xsl_transform_to));
			}

			if (xslTransformFrom == null) {
				xslTransformFrom = new XslCompiledTransform (true);
				xslTransformFrom.Load (Path.Combine (_style_sheet_location, _xsl_transform_from));
			}

			/* end of orderness */
		}

		/// <summary>
		/// Translate the note content from legacy XML format to WebKit
		/// </summary>
		/// <param name='text'>
		/// Text.
		/// </param>
		public string From (Note note)
		{
			/*
			 * I'm sure there must be a better way to do this.
			 * Basically the translator needs to be on this side of the library
			 * because when the sync service runs, it needs notes to be in their
			 * original format.
			 * 
			 * It seems very cludgy to do what I am doing here, but I don't know of another way.
			 * Aug 21, 2012 JJennings
			 */
			StringBuilder sb = new StringBuilder ();
			StringWriter stringWriter = new StringWriter (sb);
			XmlTextWriter xmlTextWriter = new XmlTextWriter (stringWriter);
			StringBuilder sbNoteText = new StringBuilder ();
			sbNoteText.AppendLine ("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			sbNoteText.AppendLine ("<note version=\"0.3\" " +
				"xmlns:link=\"http://beatniksoftware.com/tomboy/link\" " +
				"xmlns:size=\"http://beatniksoftware.com/tomboy/size\" " +
				"xmlns=\"http://beatniksoftware.com/tomboy\">");
			sbNoteText.AppendLine ("<text xml:space=\"preserve\">");
			sbNoteText.AppendLine ("<note-content version=\"1\">");
			sbNoteText.Append (note.Text);
			sbNoteText.AppendLine ("</note-content>");
			sbNoteText.AppendLine ("</text>");
			sbNoteText.AppendLine ("</note>");

			StringReader stringReader = new StringReader (sbNoteText.ToString ());
			XPathDocument doc = new XPathDocument(stringReader);
			stringReader.Close();

			xslTransformFrom.Transform(doc, null,xmlTextWriter);

			return sb.ToString ().Trim ();
		}

		/// <summary>
		/// Translates to.traditional Tomboy Note formats
		/// </summary>
		/// <returns>
		/// A string representation of the Note in the Linux traditional Linux format
		/// </returns>
		/// <param name='domDocument'>
		/// DOM document.
		/// </param>
		public String To (DomDocument domDocument)
		{
			StringBuilder sb = new StringBuilder ();
			StringWriter stringWriter = new StringWriter (sb);
			XmlTextWriter xmlTextWriter = new XmlTextWriter (stringWriter);

			DomNodeList element = domDocument.GetElementsByTagName ("body");
			//DomNodeList elementTitle = domDocument.GetElementsByTagName ("h1");
			DomHtmlElement body = (DomHtmlElement)element.First ();
			string pattern = @"(<br *\>)";
			Regex r = new Regex (pattern);
			// br needs to end in proper xml, so we are replacing <br> with <br />
			string result = r.Replace (body.OuterHTML, "<br />");

			// run the remaining of the document through the xslt
			StringReader stringReader = new StringReader (result);
			XPathDocument doc = new XPathDocument(stringReader);
			stringReader.Close();
			try {
				xslTransformTo.Transform(doc, null,xmlTextWriter);
			} catch (System.Xml.XmlException e) {
				Logger.Error (e.Message, body.OuterHTML);
				throw new System.Xml.XmlException ("XSLT transform failed to handle the note");
			}

			return sb.ToString ();;
		}

		#region private methods

		private void GetAssembly ()
		{
			try {
				_assembly = Assembly.GetExecutingAssembly ();
			} catch {
				Console.WriteLine ("Error accessing resources!");
			}	
		}

		/// <summary>
		/// Loads the paths where Tomboy stores Stylesheets
		/// </summary>
		private void LoadPaths ()
		{
			_style_sheet_location = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Caches", "Tomboy");
		}

		/// <summary>
		/// Copies a stream from one location to another..
		/// </summary>
		/// <param name='input'>
		/// Input.
		/// </param>
		/// <param name='output'>
		/// Output.
		/// </param>
		private void CopyStream (Stream input, Stream output)
		{
			// Insert null checking here for production
			byte[] buffer = new byte[8192];

			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) {
				output.Write (buffer, 0, bytesRead);
			}
		}

		/// <summary>
		/// Copies the XSL to the correct location
		/// </summary>
		private void CopyXSLT (string xsl_file_name)
		{
			if (!Directory.Exists (_style_sheet_location))
				Directory.CreateDirectory (_style_sheet_location);
			string destFile = Path.Combine (_style_sheet_location, xsl_file_name);
			/* Only copy the file if it doesn't exist
			 * This allows someone to override the default
			 * It also allows someone to rebuild if corrupt
			 */
			if (!File.Exists (destFile)) {
				Console.WriteLine ("deploying default Transform {0}", destFile);
				using (Stream input = _assembly.GetManifestResourceStream(xsl_file_name))
				using (Stream output = File.Create(destFile))
					CopyStream (input, output);
			}
		}
	#endregion private methods
	}
}

