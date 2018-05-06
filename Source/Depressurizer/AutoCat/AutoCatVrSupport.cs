﻿#region License

//     This file (AutoCatVrSupport.cs) is part of Depressurizer.
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
using System.Linq;
using System.Xml;
using Depressurizer.Core.Enums;
using Depressurizer.Core.Models;
using Rallion;

#endregion

namespace Depressurizer
{
	public class AutoCatVrSupport : AutoCat
	{
		#region Constants

		// Serialization constants
		public const string TypeIdString = "AutoCatVrSupport";
		private const string XmlNameFilter = "Filter";
		private const string XmlNameFlag = "Flag";
		private const string XmlNameHeadsetsList = "Headsets";
		private const string XmlNameInputList = "Input";

		private const string XmlNameName = "Name";
		private const string XmlNamePlayAreaList = "PlayArea";
		private const string XmlNamePrefix = "Prefix";

		#endregion

		#region Fields

		public VRSupport IncludedVrSupportFlags;

		#endregion

		#region Constructors and Destructors

		public AutoCatVrSupport(string name, string filter = null, string prefix = null, List<string> headsets = null, List<string> input = null, List<string> playArea = null, bool selected = false) : base(name)
		{
			Filter = filter;
			Prefix = prefix;

			if (headsets != null)
			{
				IncludedVrSupportFlags.Headsets.Clear();
				IncludedVrSupportFlags.Headsets.AddRange(headsets);
			}

			if (input != null)
			{
				IncludedVrSupportFlags.Input.Clear();
				IncludedVrSupportFlags.Input.AddRange(input);
			}

			if (playArea != null)
			{
				IncludedVrSupportFlags.PlayArea.Clear();
				IncludedVrSupportFlags.PlayArea.AddRange(playArea);
			}

			Selected = selected;
		}

		protected AutoCatVrSupport(AutoCatVrSupport other) : base(other)
		{
			Filter = other.Filter;
			Prefix = other.Prefix;
			IncludedVrSupportFlags = other.IncludedVrSupportFlags;
			Selected = other.Selected;
		}

		//XmlSerializer requires a parameterless constructor
		private AutoCatVrSupport() { }

		#endregion

		#region Public Properties

		public override AutoCatType AutoCatType => AutoCatType.VrSupport;

		// AutoCat configuration
		public string Prefix { get; set; }

		#endregion

		#region Public Methods and Operators

		public static AutoCatVrSupport LoadFromXmlElement(XmlElement xElement)
		{
			string name = XmlUtil.GetStringFromNode(xElement[XmlNameName], TypeIdString);
			string filter = XmlUtil.GetStringFromNode(xElement[XmlNameFilter], null);
			string prefix = XmlUtil.GetStringFromNode(xElement[XmlNamePrefix], null);
			List<string> headsetsList = new List<string>();
			List<string> inputList = new List<string>();
			List<string> playAreaList = new List<string>();

			XmlElement headset = xElement[XmlNameHeadsetsList];
			XmlElement input = xElement[XmlNameInputList];
			XmlElement playArea = xElement[XmlNamePlayAreaList];

			XmlNodeList headsetElements = headset?.SelectNodes(XmlNameFlag);
			if (headsetElements != null)
			{
				for (int i = 0; i < headsetElements.Count; i++)
				{
					XmlNode n = headsetElements[i];
					if (XmlUtil.TryGetStringFromNode(n, out string flag))
					{
						headsetsList.Add(flag);
					}
				}
			}

			XmlNodeList inputElements = input?.SelectNodes(XmlNameFlag);
			if (inputElements != null)
			{
				for (int i = 0; i < inputElements.Count; i++)
				{
					XmlNode n = inputElements[i];
					if (XmlUtil.TryGetStringFromNode(n, out string flag))
					{
						inputList.Add(flag);
					}
				}
			}

			XmlNodeList playAreaElements = playArea?.SelectNodes(XmlNameFlag);
			if (playAreaElements != null)
			{
				for (int i = 0; i < playAreaElements.Count; i++)
				{
					XmlNode n = playAreaElements[i];
					if (XmlUtil.TryGetStringFromNode(n, out string flag))
					{
						playAreaList.Add(flag);
					}
				}
			}

			return new AutoCatVrSupport(name, filter, prefix, headsetsList, inputList, playAreaList);
		}

		public override AutoCatResult CategorizeGame(GameInfo game, Filter filter)
		{
			if (games == null)
			{
				Program.Logger.Error(GlobalStrings.Log_AutoCat_GamelistNull);
				throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameList);
			}

			if (db == null)
			{
				Program.Logger.Error(GlobalStrings.Log_AutoCat_DBNull);
				throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameDB);
			}

			if (game == null)
			{
				Program.Logger.Error(GlobalStrings.Log_AutoCat_GameNull);
				return AutoCatResult.Failure;
			}

			if (!db.Contains(game.Id) || db.Games[game.Id].LastStoreScrape == 0)
			{
				return AutoCatResult.NotInDatabase;
			}

			if (!game.IncludeGame(filter))
			{
				return AutoCatResult.Filtered;
			}

			VRSupport vrSupport = db.GetVRSupport(game.Id);
			IEnumerable<string> headsets = vrSupport.Headsets.Intersect(IncludedVrSupportFlags.Headsets);
			IEnumerable<string> input = vrSupport.Input.Intersect(IncludedVrSupportFlags.Input);
			IEnumerable<string> playArea = vrSupport.PlayArea.Intersect(IncludedVrSupportFlags.PlayArea);

			foreach (string catString in headsets)
			{
				Category c = games.GetCategory(GetProcessedString(catString));
				game.AddCategory(c);
			}

			foreach (string catString in input)
			{
				Category c = games.GetCategory(GetProcessedString(catString));
				game.AddCategory(c);
			}

			foreach (string catString in playArea)
			{
				Category c = games.GetCategory(GetProcessedString(catString));
				game.AddCategory(c);
			}

			return AutoCatResult.Success;
		}

		public override AutoCat Clone()
		{
			return new AutoCatVrSupport(this);
		}

		public override void WriteToXml(XmlWriter writer)
		{
			writer.WriteStartElement(TypeIdString);

			writer.WriteElementString(XmlNameName, Name);
			if (Filter != null)
			{
				writer.WriteElementString(XmlNameFilter, Filter);
			}

			if (Prefix != null)
			{
				writer.WriteElementString(XmlNamePrefix, Prefix);
			}

			writer.WriteStartElement(XmlNameHeadsetsList);

			foreach (string s in IncludedVrSupportFlags.Headsets)
			{
				writer.WriteElementString(XmlNameFlag, s);
			}

			writer.WriteEndElement(); // VR Headsets list

			writer.WriteStartElement(XmlNameInputList);

			foreach (string s in IncludedVrSupportFlags.Input)
			{
				writer.WriteElementString(XmlNameFlag, s);
			}

			writer.WriteEndElement(); // VR Input list

			writer.WriteStartElement(XmlNamePlayAreaList);

			foreach (string s in IncludedVrSupportFlags.PlayArea)
			{
				writer.WriteElementString(XmlNameFlag, s);
			}

			writer.WriteEndElement(); // VR Play Area list
			writer.WriteEndElement(); // type ID string
		}

		#endregion

		#region Methods

		private string GetProcessedString(string baseString)
		{
			if (string.IsNullOrEmpty(Prefix))
			{
				return baseString;
			}

			return Prefix + baseString;
		}

		#endregion
	}
}
