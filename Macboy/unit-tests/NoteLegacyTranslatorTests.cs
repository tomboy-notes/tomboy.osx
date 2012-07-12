//
//  NoteLegacyTranslatorTests.cs
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
using NUnit.Framework;
using System.Text;

using Tomboy;

namespace TomboyMacUnitTests
{
	[TestFixture()]
	public class NoteLegacyTranslatorTests
	{

		[Test()]
		public void TestCase ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("The Title of the Note ");
			sb.Append ("Body of the Note ");
			sb.Append ("<br>");
			sb.Append ("<br>");
			sb.Append ("<a href=\"https://ebusinessdev.\">https://ebusinessdev.</a>");
			sb.Append ("Something we should remember <br>");
			NoteLegacyTranslator translator = new NoteLegacyTranslator ();
			Console.WriteLine ("Translated string: {0}", translator.TranslateHtml (sb.ToString ()));

		}
	}
}

