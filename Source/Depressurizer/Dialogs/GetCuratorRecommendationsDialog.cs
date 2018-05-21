#region License

//     This file (GetCuratorRecommendationsDialog.cs) is part of Depressurizer.
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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Depressurizer.Core.Enums;
using Depressurizer.Core.Helpers;
using Newtonsoft.Json.Linq;

#endregion

namespace Depressurizer.Dialogs
{
	internal class GetCuratorRecommendationsDialog : CancelableDialog
	{
		#region Fields

		public Dictionary<int, CuratorRecommendation> CuratorRecommendations = new Dictionary<int, CuratorRecommendation>();

		public int TotalCount;

		private readonly long _curatorId;

		#endregion

		#region Constructors and Destructors

		public GetCuratorRecommendationsDialog(long curatorId) : base(GlobalStrings.CDlgCurator_GettingRecommendations, false)
		{
			_curatorId = curatorId;

			SetText(GlobalStrings.CDlgCurator_GettingRecommendations);
		}

		#endregion

		#region Methods

		protected override void Finish()
		{
			if (!Canceled && (CuratorRecommendations.Count > 0) && (Error == null))
			{
				OnJobCompletion();
			}
		}

		protected override void Start()
		{
			string url = GetRecommendationsUrl(0);
			string json;

			using (WebClient webClient = new WebClient())
			{
				webClient.Headers.Set("User-Agent", "Depressurizer");
				webClient.Encoding = Encoding.UTF8;

				json = webClient.DownloadString(url);
			}

			JObject parsedJson = JObject.Parse(json);
			if (int.TryParse(parsedJson["total_count"].ToString(), out TotalCount))
			{
				SetText(GlobalStrings.CDlgCurator_GettingRecommendations + " " + string.Format(GlobalStrings.CDlg_Progress, 0, TotalCount));

				string resultsHtml = parsedJson["results_html"].ToString();
				CuratorRecommendations = CuratorRecommendations.Union(GetCuratorRecommendationsFromPage(resultsHtml)).ToDictionary(k => k.Key, v => v.Value);
				for (int currentPosition = 50; currentPosition < TotalCount; currentPosition += 50)
				{
					SetText(GlobalStrings.CDlgCurator_GettingRecommendations + " " + string.Format(GlobalStrings.CDlg_Progress, currentPosition, TotalCount));
					using (WebClient wc = new WebClient())
					{
						wc.Encoding = Encoding.UTF8;
						json = wc.DownloadString(GetRecommendationsUrl(currentPosition));
					}

					parsedJson = JObject.Parse(json);
					resultsHtml = parsedJson["results_html"].ToString();
					CuratorRecommendations = CuratorRecommendations.Union(GetCuratorRecommendationsFromPage(resultsHtml)).ToDictionary(k => k.Key, v => v.Value);
				}
			}
			else
			{
				Logger.Instance.Error("Error: CDlgCurator: Couldn't determine total count of recommendations");
			}

			if (CuratorRecommendations.Count != TotalCount)
			{
				Logger.Instance.Error("Error: CDlgCurator: Count of recommendations retrieved is different than expected");
			}
			else
			{
				Logger.Instance.Error("Retrieved {0} curator recommendations.", TotalCount);
			}

			Close();
		}

		private static Dictionary<int, CuratorRecommendation> GetCuratorRecommendationsFromPage(string page)
		{
			Dictionary<int, CuratorRecommendation> curatorRecommendations = new Dictionary<int, CuratorRecommendation>();
			Regex curatorRegex = new Regex(@"data-ds-appid=\""(\d+)\"".*?><span class='color_([^']*)", RegexOptions.Singleline | RegexOptions.Compiled);
			MatchCollection matches = curatorRegex.Matches(page);
			if (matches.Count <= 0)
			{
				return curatorRecommendations;
			}

			foreach (Match match in matches)
			{
				CuratorRecommendation recommendation;
				switch (match.Groups[2].Value)
				{
					case "recommended":
						recommendation = CuratorRecommendation.Recommended;
						break;
					case "not_recommended":
						recommendation = CuratorRecommendation.NotRecommended;
						break;
					case "informational":
						recommendation = CuratorRecommendation.Informational;
						break;
					default:
						recommendation = CuratorRecommendation.Error;
						break;
				}

				if (int.TryParse(match.Groups[1].Value, out int id) && (recommendation != CuratorRecommendation.Error))
				{
					curatorRecommendations.Add(id, recommendation);
					Logger.Instance.Verbose("Retrieved recommendation for game " + id + ": " + match.Groups[2].Value);
				}

				if (recommendation == CuratorRecommendation.Error)
				{
					Logger.Instance.Error("Error: For game " + id + ": recommendation recognized as \"" + match.Groups[2].Value + '"');
				}
			}

			return curatorRecommendations;
		}

		private string GetRecommendationsUrl(int start)
		{
			return string.Format(CultureInfo.InvariantCulture, "http://store.steampowered.com/curators/ajaxgetcuratorrecommendations/{0}/?query=&amp;start={1}&amp;count=50", _curatorId, start);
		}

		#endregion
	}
}
