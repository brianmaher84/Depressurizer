#region License

//     This file (UpdateProfileDialog.cs) is part of Depressurizer.
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
using Depressurizer.Core;
using Depressurizer.Core.Enums;

#endregion

namespace Depressurizer.Dialogs
{
	internal class UpdateProfileDialog : CancelableDialog
	{
		#region Fields

		private readonly bool _custom;

		private readonly string _customUrl;

		private readonly GameList _data;

		private readonly SortedSet<int> _ignore;

		private readonly bool _overwrite;

		private readonly long _steamId;

		private XmlDocument _document;

		private string _htmlDoc;

		#endregion

		#region Constructors and Destructors

		public UpdateProfileDialog(GameList data, long accountId, bool overwrite, SortedSet<int> ignore) : base(GlobalStrings.CDlgUpdateProfile_UpdatingGameList, true)
		{
			_custom = false;
			_steamId = accountId;

			Added = 0;
			Fetched = 0;
			UseHtml = false;
			Failover = false;

			_data = data;

			_overwrite = overwrite;
			_ignore = ignore;

			SetText(GlobalStrings.CDlgFetch_DownloadingGameList);
		}

		public UpdateProfileDialog(GameList data, string customUrl, bool overwrite, SortedSet<int> ignore) : base(GlobalStrings.CDlgUpdateProfile_UpdatingGameList, true)
		{
			_custom = true;
			_customUrl = customUrl;

			Added = 0;
			Fetched = 0;
			UseHtml = false;
			Failover = false;

			_data = data;

			_overwrite = overwrite;
			_ignore = ignore;

			SetText(GlobalStrings.CDlgFetch_DownloadingGameList);
		}

		#endregion

		#region Public Properties

		public int Added { get; private set; }

		public bool Failover { get; private set; }

		public int Fetched { get; private set; }

		public int Removed { get; private set; }

		public bool UseHtml { get; private set; }

		#endregion

		#region Methods

		protected void FetchHtml()
		{
			UseHtml = true;
			_htmlDoc = _custom ? GameList.FetchHtmlGameList(_customUrl) : GameList.FetchHtmlGameList(_steamId);
		}

		protected void FetchXml()
		{
			UseHtml = false;
			_document = _custom ? GameList.FetchXmlGameList(_customUrl) : GameList.FetchXmlGameList(_steamId);
		}

		protected void FetchXmlPref()
		{


			try
			{
				FetchXml();
				return;
			}
			catch (Exception) { }

			Failover = true;
			FetchHtml();
		}

		protected override void Finish()
		{
			if (Canceled || (Error != null))
			{
				return;
			}

			if (UseHtml ? _htmlDoc == null : _document == null)
			{
				return;
			}

			SetText(GlobalStrings.CDlgFetch_FinishingDownload);
			if (UseHtml)
			{
				Fetched = _data.IntegrateHtmlGameList(_htmlDoc, _overwrite, _ignore, out int newItems);
				Added = newItems;
			}
			else
			{
				Fetched = _data.IntegrateXmlGameList(_document, _overwrite, _ignore, out int newItems);
				Added = newItems;
			}

			OnJobCompletion();
		}

		protected override void Start()
		{
			Added = 0;
			Fetched = 0;

			switch (Settings.Instance.ListSource)
			{
				case GameListSource.XmlPreferred:
					FetchXmlPref();
					break;
				case GameListSource.XmlOnly:
					FetchXml();
					break;
				case GameListSource.WebsiteOnly:
					FetchHtml();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			OnJobCompletion();
		}

		#endregion
	}
}
