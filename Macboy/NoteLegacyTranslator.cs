//
//  NoteLegacyTranslator.cs
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
		/// Finds HREF's in the given string. Returns a LinkTem with Name and URL values
		/// </summary>
		/// <param name='text'>
		/// Text.
		/// </param>
		public List<LinkItem> Find (string text)
		{
			List<LinkItem> list = new List<LinkItem> ();

			// 1.
			// Find all matches in file.
			MatchCollection m1 = Regex.Matches (text, @"(<a.*?>.*?</a>)", RegexOptions.Singleline);

			// 2.
			// Loop over each match.
			foreach (Match m in m1) {
				string value = m.Groups [1].Value;
				LinkItem i = new LinkItem ();
				i.WholeHREF = m.Value;

				// 3.
				// Get href attribute.
				Match m2 = Regex.Match (value, @"href=\""(.*?)\""", RegexOptions.Singleline);
				if (m2.Success) {
					i.Href = m2.Groups [1].Value;
				}

				// 4.
				// Remove inner tags from text.
				string t = Regex.Replace (value, @"\s*<.*?>\s*", "", RegexOptions.Singleline);
				i.Text = t;

				list.Add (i);
			}
			return list;
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

		public String TranslateTo (DomDocument domDocument)
		{
			StringBuilder sb = new StringBuilder ();
			StringWriter stringWriter = new StringWriter (sb);
			XmlTextWriter xmlTextWriter = new XmlTextWriter (stringWriter);

			DomNodeList element = domDocument.GetElementsByTagName ("body");
			//DomNodeList elementTitle = domDocument.GetElementsByTagName ("h1");
			DomHtmlElement body = (DomHtmlElement)element.First ();

			/* begin: strip <br> tags from HTML */

			string pattern = @"(<br *\>)";
			Regex r = new Regex (pattern);
			string result = r.Replace (body.OuterHTML, System.Environment.NewLine);

			StringReader stringReader = new StringReader (result);
			XPathDocument doc = new XPathDocument(stringReader);
			stringReader.Close();

			xslTransformTo.Transform(doc, null,xmlTextWriter);
			/* end: strip <br> tags from HTML */
			//result =  xmlTextWriter.ToString ();
			return sb.ToString ();
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

			/* Only copy the file if it doesn't exist
			 * This allows someone to override the default
			 * It also allows someone to rebuild if corrupt
			 */
			if (!File.Exists (Path.Combine (_style_sheet_location, xsl_file_name))) {
				Console.WriteLine ("deploying default Transform {0}", xsl_file_name);
				using (Stream input = _assembly.GetManifestResourceStream(xsl_file_name))
				using (Stream output = File.Create(Path.Combine (_style_sheet_location, xsl_file_name)))
					CopyStream (input, output);
			}
		}
	#endregion private methods
	}
}

