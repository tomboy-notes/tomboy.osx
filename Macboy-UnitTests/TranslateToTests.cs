//
//  TranslateToTests.cs
//
//  Author:
//       Jared Jennings <jjennings@gnome.org>
//
//  Copyright (c) 2012 Jared Jennings 2012
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

namespace Tomboy
{
	[TestFixture()]
	public class TranslateToTests
	{
		string ExampleNote = "<body style=\"word-wrap: break-word; -webkit-nbsp-mode: space; -webkit-line-break: after-white-space; \">" +
			"<h1>My Example on OSX</h1>" +
			"My first new line<div>" +
			"Second new line</div>" +
			"<div>Third new line</div>" +
			"<div><br></div>" +
			"<div>Fourth</div>" +
			"</body>";
		Note note;

		[SetUp]
		public void Init ()
		{
			note = new Note ("note:/tomboy/junkNoteGUID");
		}
	}
}

