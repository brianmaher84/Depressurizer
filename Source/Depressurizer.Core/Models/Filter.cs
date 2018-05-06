#region License

//     This file (Filter.cs) is part of Depressurizer.
//     Copyright (C) 2018  Martijn Vegter
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Xml;

#endregion

namespace Depressurizer.Core.Models
{
	/// <summary>
	///     Depressurizer Filter
	/// </summary>
	public sealed class Filter : IComparable
	{
		#region Constants

		private const string TypeIdString = "Filter";
		private const string XmlNameAllow = "Allow";
		private const string XmlNameExclude = "Exclude";
		private const string XmlNameHidden = "Hidden";
		private const string XmlNameName = "Name";
		private const string XmlNameRequire = "Require";
		private const string XmlNameUncategorized = "Uncategorized";
		private const string XmlNameVR = "VR";

		#endregion

		#region Fields

		private SortedSet<Category> _allow;

		private SortedSet<Category> _exclude;

		private SortedSet<Category> _require;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		///     Create a new Filter
		/// </summary>
		/// <param name="name">Name of the Filter</param>
		public Filter(string name)
		{
			Name = name;
			Allow = new SortedSet<Category>();
			Require = new SortedSet<Category>();
			Exclude = new SortedSet<Category>();
		}

		#endregion

		#region Public Properties

		public SortedSet<Category> Allow
		{
			get => _allow;
			set => _allow = new SortedSet<Category>(value);
		}

		public SortedSet<Category> Exclude
		{
			get => _exclude;
			set => _exclude = new SortedSet<Category>(value);
		}

		public int Hidden { get; set; } = -1;

		/// <summary>
		///     Filter Name
		/// </summary>
		public string Name { get; set; } = null;

		public SortedSet<Category> Require
		{
			get => _require;
			set => _require = new SortedSet<Category>(value);
		}

		public int Uncategorized { get; set; } = -1;

		public int VR { get; set; } = -1;

		#endregion

		#region Public Methods and Operators

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is Filter otherFilter))
			{
				throw new ArgumentException("Object is not a Filter");
			}

			return string.CompareOrdinal(Name, otherFilter.Name);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}

		public void WriteToXml(XmlWriter writer)
		{
			writer.WriteStartElement(TypeIdString);

			writer.WriteElementString(XmlNameName, Name);
			writer.WriteElementString(XmlNameUncategorized, Uncategorized.ToString());
			writer.WriteElementString(XmlNameHidden, Hidden.ToString());
			writer.WriteElementString(XmlNameVR, VR.ToString());

			foreach (Category category in Allow)
			{
				writer.WriteElementString(XmlNameAllow, category.Name);
			}

			foreach (Category category in Require)
			{
				writer.WriteElementString(XmlNameRequire, category.Name);
			}

			foreach (Category category in Exclude)
			{
				writer.WriteElementString(XmlNameExclude, category.Name);
			}

			writer.WriteEndElement();
		}

		#endregion
	}
}
