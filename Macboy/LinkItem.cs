//
//  LinkItem.cs
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

namespace Tomboy
{
	/// <summary>
	/// Link item.
	/// </summary>
	/// <description>Represents a link types in Tomboy Notes</description>
	public struct LinkItem
	{
		/// <summary>
		/// The href.example: http://MyURL
		/// </summary>
		public string Href;
		/// <summary>
		/// The text.example: "My URL"
		/// </summary>
		public string Text;
		/// <summary>
		/// The whole HRE.example: <a HREF="http://MyURL">My URL</a>
		/// </summary>
		public string WholeHREF;

		public override string ToString()
		{
			return Href + "\n\t" + Text;
		}
	}
}

