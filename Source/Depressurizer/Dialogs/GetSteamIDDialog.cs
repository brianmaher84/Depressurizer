#region License

//     This file (GetSteamIDDialog.cs) is part of Depressurizer.
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
using System.Globalization;
using System.Net;
using System.Xml;
using Depressurizer.Core.Helpers;
using Depressurizer.Properties;

#endregion

namespace Depressurizer.Dialogs
{
	internal class GetSteamIDDialog : CancelableDialog
	{
		#region Fields

		private readonly string _customUrlName;

		#endregion

		#region Constructors and Destructors

		public GetSteamIDDialog(string customUrl) : base(Resources.GetSteamIDDialog_Title, false)
		{
			SteamID = 0;
			Success = false;
			_customUrlName = customUrl;

			SetText(Resources.GetSteamIDDialog_Status);
		}

		#endregion

		#region Public Properties

		public long SteamID { get; private set; }

		public bool Success { get; private set; }

		#endregion

		#region Methods

		protected override void Finish()
		{
			if (Canceled)
			{
				return;
			}

			OnJobCompletion();
		}

		protected override void Start()
		{
			XmlDocument document = new XmlDocument();

			try
			{
				string url = string.Format(CultureInfo.InvariantCulture, "http://steamcommunity.com/id/{0}?xml=1", _customUrlName);
				Logger.Instance.Info("Attempting to download XML profile page for custom URL name {0}: {1}", _customUrlName, url);

				WebRequest webRequest = WebRequest.Create(url);
				using (WebResponse webResponse = webRequest.GetResponse())
				{
					document.Load(webResponse.GetResponseStream() ?? throw new InvalidOperationException());
				}

				Logger.Instance.Info("Successfully downloaded XML profile");
			}
			catch (Exception e)
			{
				Program.Logger.Exception("Exception when downloading XML profile:", e);
				throw new ApplicationException(Resources.GetSteamIDDialog_Error + e.Message, e);
			}

			XmlNode node = document.SelectSingleNode("/profile/steamID64");
			if (node != null)
			{
				Success = long.TryParse(node.InnerText, out long steamID);
				if (Success)
				{
					SteamID = steamID;
				}
			}

			Close();
		}

		#endregion
	}
}
