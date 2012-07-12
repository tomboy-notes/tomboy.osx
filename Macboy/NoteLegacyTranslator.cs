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
using System.Text.RegularExpressions;

namespace Tomboy
{
	/// <summary>
	/// Note legacy translator which handles text formating between Pango and othe formats.
	/// </summary>
	public class NoteLegacyTranslator
	{

		public NoteLegacyTranslator ()
		{
		}

		public String TranslateHtml (string html)
		{
			string pattern = @"(<br *\>)";
			// Instantiate the regular expression object.
			Regex r = new Regex(pattern);

			string result = r.Replace (html, System.Environment.NewLine);
			for (int pos = 0; pos < result.Length; pos++) {
				char ch = result[pos];

				if (ch == Convert.ToChar ("<") && Convert.ToChar (result[pos + 1]) == Convert.ToChar ("a")) {
					Console.WriteLine ("Found a match for < at pos:{0}, which gets us {1}", pos, result[pos]);
					for (int posValue = 0; posValue < result.Length; posValue++) {

					}

				}
			}

			return result;

			/* StringBuilder sb = new StringBuilder (html);

			for (int ctr = 0; ctr < sb.Length; ctr++) {
				char ch = sb[ctr];

				if (char.ToUpperInvariant (ch) == char.ToUpperInvariant (Convert.ToChar ("<"))) {
					Console.WriteLine ("Found a match for <");
				}
			}
			return sb.ToString ();
			 */
		}
	}
}

