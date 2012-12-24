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
		private string _xsl_to_path_file = "";
		private string _xsl_from_path_file= "";
		private const string _xsl_from = "Tomboy.transform_from_note.xsl";
		private const string _xsl_to = "Tomboy.transform_to_note.xsl";
		private string _style_sheet_location = Path.Combine (
			Environment.GetFolderPath (Environment.SpecialFolder.Personal), 
			"Library", 
			"Caches", 
			"Tomboy"
			);

		public NoteLegacyTranslator ()
		{
			/* The order of the following methods matter */
			GetAssembly ();
			DetectCustomXSL ();

			if (xslTransformTo == null) {
				xslTransformTo = new XslCompiledTransform (true);
				xslTransformTo.Load (_xsl_to_path_file);
			}

			if (xslTransformFrom == null) {
				xslTransformFrom = new XslCompiledTransform (true);
				xslTransformFrom.Load (_xsl_from_path_file);
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
			XPathDocument doc;
			try {
				doc = new XPathDocument(stringReader);
				xslTransformFrom.Transform(doc, null,xmlTextWriter);
			} catch (Exception e) {
				Logger.Error (e.Message, e, sbNoteText.ToString ());
			}
			stringReader.Close();
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

		// Was getting XSLT crashes until we replaced nbsp.
		// there may be a better way to handle this, but at this time I don't know what that is.
			result = result.Replace ("&nbsp;", "&#160;");
			// run the remaining of the document through t
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
				Logger.Error ("Error accessing Assembly resources!");
			}	
		}

		private void DetectCustomXSL ()
		{
			_xsl_from_path_file = Path.Combine (_style_sheet_location, _xsl_from);
			_xsl_to_path_file = Path.Combine (_style_sheet_location, _xsl_to);

			if (!(Directory.Exists (_style_sheet_location)))
				Directory.CreateDirectory (_style_sheet_location);

			if (!File.Exists (_xsl_from_path_file)) {
				CopyXSLT (_xsl_from, _xsl_from_path_file);
			}

			if (!File.Exists (_xsl_to_path_file)) {
				CopyXSLT (_xsl_to, _xsl_to_path_file);
			}
		}

		/// <summary>
		/// Copies the XSL to the correct location
		/// </summary>
		private void CopyXSLT (string source, string dest)
		{
			Logger.Info ("deploying default Transform {0} to {1}", source, dest);
			Logger.Debug ("Assembly Resource Names {0}", _assembly.GetManifestResourceNames ());
			using (Stream s = _assembly.GetManifestResourceStream(source)) {

				FileStream resourceFile = new FileStream(dest, FileMode.Create);
				
				byte[] b = new byte[s.Length + 1];
				s.Read(b, 0, Convert.ToInt32(s.Length));
				resourceFile.Write(b, 0, Convert.ToInt32(b.Length - 1));
				resourceFile.Flush();
				resourceFile.Close();
				
				resourceFile = null;
			}
		}
	#endregion private methods
	}
}

