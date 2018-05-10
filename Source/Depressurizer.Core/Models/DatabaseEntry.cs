#region License

//     This file (DatabaseEntry.cs) is part of Depressurizer.
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Depressurizer.Core.Enums;
using Depressurizer.Core.Helpers;

#endregion

namespace Depressurizer.Core.Models
{
	/// <summary>
	///     Depressurizer Database Entry
	/// </summary>
	public sealed class DatabaseEntry
	{
		#region Static Fields

		private static readonly Regex RegexAchievements = new Regex(@"<div (?:id=""achievement_block"" ?|class=""block responsive_apppage_details_right"" ?){2}>\s*<div class=""block_title"">[^\d]*(\d+)[^\d<]*</div>\s*<div class=""communitylink_achievement_images"">", RegexOptions.Compiled);

		private static readonly Regex RegexDevelopers = new Regex(@"(<a href=""https?://store\.steampowered\.com/search/\?developer=[^""]*"">([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		private static readonly Regex RegexDLCCheck = new Regex(@"<img class=""category_icon"" src=""https?://store\.akamai\.steamstatic\.com/public/images/v6/ico/ico_dlc\.png"">", RegexOptions.Compiled);

		private static readonly Regex RegexFlags = new Regex(@"<a class=""name"" href=""https?://store\.steampowered\.com/search/\?category2=.*?"">([^<]*)</a>", RegexOptions.Compiled);

		private static readonly Regex RegexGameCheck = new Regex(@"<a href=""http://store\.steampowered\.com/search/\?term=&snr=", RegexOptions.Compiled);

		private static readonly Regex RegexGenre = new Regex(@"<div class=""details_block"">\s*<b>[^:]*:</b>.*?<br>\s*<b>[^:]*:</b>\s*(<a href=""https?://store\.steampowered\.com/genre/[^>]*>([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		private static readonly Regex RegexLanguageSupport = new Regex(@"<td style=""width: 94px; text-align: left"" class=""ellipsis"">\s*([^<]*)\s*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>", RegexOptions.Compiled);

		private static readonly Regex RegexMetacritic = new Regex(@"<div id=""game_area_metalink"">\s*<a href=""https?://www\.metacritic\.com/game/pc/([^""]*)\?ftag=", RegexOptions.Compiled);

		private static readonly Regex RegexPlatformLinux = new Regex(@"<span class=""platform_img linux""></span>", RegexOptions.Compiled);

		private static readonly Regex RegexPlatformMac = new Regex(@"<span class=""platform_img mac""></span>", RegexOptions.Compiled);

		private static readonly Regex RegexPlatformWindows = new Regex(@"<span class=""platform_img win""></span>", RegexOptions.Compiled);

		private static readonly Regex RegexPublishers = new Regex(@"(<a href=""https?://store\.steampowered\.com/search/\?publisher=[^""]*"">([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		private static readonly Regex RegexRelDate = new Regex(@"<div class=""release_date"">\s*<div[^>]*>[^<]*<\/div>\s*<div class=""date"">([^<]+)<\/div>", RegexOptions.Compiled);

		private static readonly Regex RegexReviews = new Regex(@"<span class=""(?:nonresponsive_hidden ?| responsive_reviewdesc ?){2}"">[^\d]*(\d+)%[^\d]*([\d.,]+)[^\d]*\s*</span>", RegexOptions.Compiled);

		private static readonly Regex RegexSoftwareCheck = new Regex(@"<a href=""https?://store\.steampowered\.com/search/\?category1=994&snr=", RegexOptions.Compiled);

		private static readonly Regex RegexSteamStore = new Regex(@"https?://store\.steampowered\.com/?", RegexOptions.Compiled);

		private static readonly Regex RegexTags = new Regex(@"<a[^>]*class=""app_tag""[^>]*>([^<]*)</a>", RegexOptions.Compiled);

		private static readonly Regex RegexVRSupportFlagMatch = new Regex(@"<div class=""game_area_details_specs"">.*?<a class=""name"" href=""https?:\/\/store\.steampowered\.com\/search\/\?vrsupport=\d*"">([^<]*)<\/a><\/div>", RegexOptions.Compiled);

		private static readonly Regex RegexVRSupportHeadsetsSection = new Regex(@"<div class=""details_block vrsupport"">(.*)<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">", RegexOptions.Compiled);

		private static readonly Regex RegexVRSupportInputSection = new Regex(@"<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">(.*)<div class=""details_block vrsupport"">", RegexOptions.Compiled);

		private static readonly Regex RegexVRSupportPlayAreaSection = new Regex(@"<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">(.*)", RegexOptions.Compiled);

		#endregion

		#region Fields

		private LanguageSupport _languageSupport;

		private VRSupport _vrSupport;

		#endregion

		#region Public Properties

		public AppType AppType { get; set; } = AppType.Unknown;

		public string Banner { get; set; } = null;

		public List<string> Developers { get; } = new List<string>();

		public List<string> Flags { get; } = new List<string>();

		public List<string> Genres { get; } = new List<string>();

		public int HltbCompletionist { get; set; } = 0;

		public int HltbExtras { get; set; } = 0;

		public int HltbMain { get; set; } = 0;

		public int Id { get; set; } = 0;

		public LanguageSupport LanguageSupport
		{
			get => _languageSupport ?? (_languageSupport = new LanguageSupport());
			set => _languageSupport = value;
		}

		public int LastAppInfoUpdate { get; set; } = 0;

		public long LastStoreScrape { get; set; } = 0;

		public string MetacriticUrl { get; set; } = null;

		public string Name { get; set; } = null;

		public int ParentId { get; set; } = -1;

		public AppPlatforms Platforms { get; set; } = AppPlatforms.None;

		public List<string> Publishers { get; } = new List<string>();

		public int ReviewPositivePercentage { get; set; } = 0;

		public int ReviewTotal { get; set; } = 0;

		public string SteamReleaseDate { get; set; } = null;

		public List<string> Tags { get; } = new List<string>();

		public int TotalAchievements { get; set; } = 0;

		public VRSupport VRSupport
		{
			get => _vrSupport ?? (_vrSupport = new VRSupport());
			set => _vrSupport = value;
		}

		#endregion

		#region Public Methods and Operators

		public void MergeIn(DatabaseEntry other)
		{
			bool useAppInfoFields = other.LastAppInfoUpdate > LastAppInfoUpdate || LastAppInfoUpdate == 0 && other.LastStoreScrape >= LastStoreScrape;
			bool useScrapeOnlyFields = other.LastStoreScrape >= LastStoreScrape;

			if (other.AppType != AppType.Unknown && (AppType == AppType.Unknown || useAppInfoFields))
			{
				AppType = other.AppType;
			}

			if (other.LastStoreScrape >= LastStoreScrape || LastStoreScrape == 0 && other.LastAppInfoUpdate > LastAppInfoUpdate || Platforms == AppPlatforms.None)
			{
				Platforms = other.Platforms;
			}

			if (useAppInfoFields)
			{
				if (!string.IsNullOrEmpty(other.Name))
				{
					Name = other.Name;
				}

				if (other.ParentId > 0)
				{
					ParentId = other.ParentId;
				}
			}

			if (useScrapeOnlyFields)
			{
				if (other.Genres != null && other.Genres.Count > 0)
				{
					Genres.Clear();
					Genres.AddRange(other.Genres);
				}

				if (other.Flags != null && other.Flags.Count > 0)
				{
					Flags.Clear();
					Flags.AddRange(other.Flags);
				}

				if (other.Tags != null && other.Tags.Count > 0)
				{
					Tags.Clear();
					Tags.AddRange(other.Tags);
				}

				if (other.Developers != null && other.Developers.Count > 0)
				{
					Developers.Clear();
					Developers.AddRange(other.Developers);
				}

				if (other.Publishers != null && other.Publishers.Count > 0)
				{
					Publishers.Clear();
					Publishers.AddRange(other.Publishers);
				}

				if (!string.IsNullOrEmpty(other.SteamReleaseDate))
				{
					SteamReleaseDate = other.SteamReleaseDate;
				}

				if (other.TotalAchievements != 0)
				{
					TotalAchievements = other.TotalAchievements;
				}

				//VR Support
				if (other.VRSupport.Headsets.Count > 0)
				{
					VRSupport.Headsets.Clear();
					VRSupport.Headsets.AddRange(other.VRSupport.Headsets);
				}

				if (other.VRSupport.Input.Count > 0)
				{
					VRSupport.Input.Clear();
					VRSupport.Input.AddRange(other.VRSupport.Input);
				}

				if (other.VRSupport.PlayArea.Count > 0)
				{
					VRSupport.PlayArea.Clear();
					VRSupport.PlayArea.AddRange(other.VRSupport.PlayArea);
				}

				// Language Support
				if (other.LanguageSupport.FullAudio.Count > 0)
				{
					LanguageSupport.FullAudio.Clear();
					LanguageSupport.FullAudio.AddRange(other.LanguageSupport.FullAudio);
				}

				if (other.LanguageSupport.Interface.Count > 0)
				{
					LanguageSupport.Interface.Clear();
					LanguageSupport.Interface.AddRange(other.LanguageSupport.Interface);
				}

				if (other.LanguageSupport.Subtitles.Count > 0)
				{
					LanguageSupport.Subtitles.Clear();
					LanguageSupport.Subtitles.AddRange(other.LanguageSupport.Subtitles);
				}

				if (other.ReviewTotal != 0)
				{
					ReviewTotal = other.ReviewTotal;
					ReviewPositivePercentage = other.ReviewPositivePercentage;
				}

				if (!string.IsNullOrEmpty(other.MetacriticUrl))
				{
					MetacriticUrl = other.MetacriticUrl;
				}
			}

			if (other.LastStoreScrape > LastStoreScrape)
			{
				LastStoreScrape = other.LastStoreScrape;
			}

			if (other.LastAppInfoUpdate > LastAppInfoUpdate)
			{
				LastAppInfoUpdate = other.LastAppInfoUpdate;
			}
		}

		public void ScrapeStore()
		{
			Logger.Instance.Verbose("Scraping {0}: Initializing store scraping for Id: {0}", Id);

			string page;
			int redirectTarget = -1;

			HttpWebResponse resp = null;
			Stream responseStream = null;

			try
			{
				string storePage = string.Format(CultureInfo.InvariantCulture, "https://store.steampowered.com/app/{0}/?l={1}", Id, Settings.Instance.StoreLanguage).ToLowerInvariant();

				HttpWebRequest req = GetSteamRequest(storePage);
				resp = (HttpWebResponse) req.GetResponse();

				int count = 0;
				while (resp.StatusCode == HttpStatusCode.Found && count < 5)
				{
					resp.Close();

					// If we are redirected to the store front page
					if (RegexSteamStore.IsMatch(resp.Headers[HttpResponseHeader.Location]))
					{
						Logger.Instance.Warn("Scraping {0}: Redirected to main store page, aborting scraping", Id);
						return;
					}

					// Check if we were redirected to the same page
					if (resp.ResponseUri.ToString() == resp.Headers[HttpResponseHeader.Location])
					{
						Logger.Instance.Warn("Scraping {0}: Store page redirected to itself, aborting scraping", Id);
						return;
					}

					req = GetSteamRequest(resp.Headers[HttpResponseHeader.Location]);
					resp = (HttpWebResponse) req.GetResponse();
					count++;
				}

				// Check if we were redirected too many times
				if (count == 5 && resp.StatusCode == HttpStatusCode.Found)
				{
					Logger.Instance.Warn("Scraping {0}: Too many redirects, aborting scraping", Id);
					return;
				}

				// Check if we were redirected to the Steam Store front page
				if (resp.ResponseUri.Segments.Length < 2)
				{
					Logger.Instance.Warn("Scraping {0}: Redirected to main store page, aborting scraping", Id);
					return;
				}

				// Check if we were redirected outside of the app route
				if (resp.ResponseUri.Segments[1] != "app/")
				{
					Logger.Instance.Warn("Scraping {0}: Redirected outside the app (app/) route, aborting scraping", Id);
					return;
				}

				// The URI ends with "/app/" ?
				if (resp.ResponseUri.Segments.Length < 3)
				{
					Logger.Instance.Warn("Scraping {0}: Response URI ends with 'app' thus missing ID found, aborting scraping", Id);
					return;
				}

				// Check if we encountered an age gate, cookies should bypass this, but sometimes they don't seem to
				if (resp.ResponseUri.Segments[1] == "agecheck/")
				{
					// Encountered an age check with no redirect
					if (resp.ResponseUri.Segments.Length < 4 || resp.ResponseUri.Segments[3].TrimEnd('/') == Id.ToString(CultureInfo.InvariantCulture))
					{
						Logger.Instance.Warn("Scraping {0}: Encounterd an age check without redirect, aborting scraping", Id);
						return;
					}

					// Age check + redirect
					Logger.Instance.Verbose("Scraping {0}: Hit age check for Id: {1}", Id, resp.ResponseUri.Segments[3].TrimEnd('/'));

					// Check if we encountered an age gate without a numeric id
					if (!int.TryParse(resp.ResponseUri.Segments[3].TrimEnd('/'), out redirectTarget))
					{
						return;
					}
				}

				// Check if we were redirected to a different Id
				else if (resp.ResponseUri.Segments[2].TrimEnd('/') != Id.ToString(CultureInfo.InvariantCulture))
				{
					// if new app id is an actual number
					if (!int.TryParse(resp.ResponseUri.Segments[2].TrimEnd('/'), out redirectTarget))
					{
						Logger.Instance.Warn("Scraping {0}: Redirected to an unknown Id ({1}), aborting scraping", Id, resp.ResponseUri.Segments[2].TrimEnd('/'));
						return;
					}

					Logger.Instance.Verbose("Scraping {0}: Redirected to another app Id ({1})", Id, resp.ResponseUri.Segments[2].TrimEnd('/'));
				}

				responseStream = resp.GetResponseStream();
				if (responseStream == null)
				{
					Logger.Instance.Warn("Scraping {0}: The response stream was null, aborting scraping", Id);
					return;
				}

				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					page = streamReader.ReadToEnd();
					Logger.Instance.Verbose("Scraping {0}: Page read", Id);
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Verbose("Scraping {0}: Page read failed. {1}", Id, e.Message);
				return;
			}
			finally
			{
				if (resp != null)
				{
					resp.Dispose();
				}

				if (responseStream != null)
				{
					responseStream.Dispose();
				}
			}

			// Check for server-sided errors
			if (page.Contains("<title>Site Error</title>"))
			{
				Logger.Instance.Warn("Scraping {0}: Received Site Error, aborting scraping", Id);
				return;
			}

			// Double checking if this is an app (Game or Application)
			if (!RegexGameCheck.IsMatch(page) && !RegexSoftwareCheck.IsMatch(page))
			{
				Logger.Instance.Warn("Scraping {0}: Could not parse info from page, aborting scraping", Id);
				return;
			}

			LastStoreScrape = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			GetAllDataFromPage(page);

			// Set or Update ParentId if we got a redirect target
			if (redirectTarget != -1)
			{
				ParentId = redirectTarget;
			}

			// Check whether it's DLC and return appropriately
			if (RegexDLCCheck.IsMatch(page))
			{
				Logger.Instance.Verbose("Scraping {0}: Parsed. DLC. Genre: {1}", Id, string.Join(",", Genres));
				AppType = AppType.DLC;
				return;
			}

			Logger.Instance.Verbose("Scraping {0}: Parsed. Genre: {1}", Id, string.Join(",", Genres));

			if (RegexSoftwareCheck.IsMatch(page))
			{
				AppType = AppType.Application;
			}

			if (RegexGameCheck.IsMatch(page))
			{
				AppType = AppType.Game;
			}
		}

		#endregion

		#region Methods

		private static HttpWebRequest GetSteamRequest(string url)
		{
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
			// Cookie bypasses the age gate
			req.CookieContainer = new CookieContainer(3);
			req.CookieContainer.Add(new Cookie("birthtime", "-473392799", "/", "store.steampowered.com"));
			req.CookieContainer.Add(new Cookie("mature_content", "1", "/", "store.steampowered.com"));
			req.CookieContainer.Add(new Cookie("lastagecheckage", "1-January-1955", "/", "store.steampowered.com"));
			// Cookies get discarded on automatic redirects so we have to follow them manually
			req.AllowAutoRedirect = false;
			return req;
		}

		private void GetAllDataFromPage(string page)
		{
			// Genres
			Match match = RegexGenre.Match(page);
			if (match.Success)
			{
				Genres.Clear();
				foreach (Capture c in match.Groups[2].Captures)
				{
					string genre = WebUtility.HtmlDecode(c.Value.Trim());
					if (!string.IsNullOrWhiteSpace(genre))
					{
						Genres.Add(genre);
					}
				}
			}

			// Flags
			MatchCollection matches = RegexFlags.Matches(page);
			if (matches.Count > 0)
			{
				Flags.Clear();
				foreach (Match m in matches)
				{
					string flag = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(flag))
					{
						Flags.Add(flag);
					}
				}
			}

			// Tags
			matches = RegexTags.Matches(page);
			if (matches.Count > 0)
			{
				Tags.Clear();
				foreach (Match m in matches)
				{
					string tag = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(tag))
					{
						Tags.Add(tag);
					}
				}
			}

			// Developers
			match = RegexDevelopers.Match(page);
			if (match.Success)
			{
				Developers.Clear();
				foreach (Capture c in match.Groups[2].Captures)
				{
					string developer = WebUtility.HtmlDecode(c.Value.Trim());
					if (!string.IsNullOrWhiteSpace(developer))
					{
						Developers.Add(developer);
					}
				}
			}

			// Publishers
			match = RegexPublishers.Match(page);
			if (match.Success)
			{
				Publishers.Clear();
				foreach (Capture c in match.Groups[2].Captures)
				{
					string publisher = WebUtility.HtmlDecode(c.Value.Trim());
					if (!string.IsNullOrWhiteSpace(publisher))
					{
						Publishers.Add(publisher);
					}
				}
			}

			// Metacritic
			match = RegexMetacritic.Match(page);
			if (match.Success)
			{
				string url = WebUtility.HtmlDecode(match.Groups[1].Captures[0].Value.Trim());
				if (!string.IsNullOrWhiteSpace(url))
				{
					MetacriticUrl = url;
				}
			}

			// Platform Windows
			match = RegexPlatformWindows.Match(page);
			if (match.Success)
			{
				Platforms |= AppPlatforms.Windows;
			}

			// Platform Mac
			match = RegexPlatformMac.Match(page);
			if (match.Success)
			{
				Platforms |= AppPlatforms.Mac;
			}

			// Platform Linux
			match = RegexPlatformLinux.Match(page);
			if (match.Success)
			{
				Platforms |= AppPlatforms.Linux;
			}

			// Get user review data
			match = RegexReviews.Match(page);
			if (match.Success)
			{
				if (int.TryParse(match.Groups[1].Value, out int num))
				{
					ReviewPositivePercentage = num;
				}

				if (int.TryParse(match.Groups[2].Value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out num))
				{
					ReviewTotal = num;
				}
			}

			//Get Achievement number
			match = RegexAchievements.Match(page);
			if (match.Success)
			{
				// Sometimes games have achievements but don't have the "Steam Achievements" flag in the store
				if (!Flags.Contains("Steam Achievements"))
				{
					Flags.Add("Steam Achievements");
				}

				if (int.TryParse(match.Groups[1].Value, out int num))
				{
					TotalAchievements = num;
				}
			}

			// VR Support headsets
			match = RegexVRSupportHeadsetsSection.Match(page);
			if (match.Success)
			{
				matches = RegexVRSupportFlagMatch.Matches(match.Groups[1].Value.Trim());

				VRSupport.Headsets.Clear();
				foreach (Match m in matches)
				{
					string headset = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(headset))
					{
						VRSupport.Headsets.Add(headset);
					}
				}
			}

			// VR Support Input
			match = RegexVRSupportInputSection.Match(page);
			if (match.Success)
			{
				matches = RegexVRSupportFlagMatch.Matches(match.Groups[1].Value.Trim());

				VRSupport.Input.Clear();
				foreach (Match m in matches)
				{
					string input = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(input))
					{
						VRSupport.Input.Add(input);
					}
				}
			}

			// VR Support Play Area
			match = RegexVRSupportPlayAreaSection.Match(page);
			if (match.Success)
			{
				matches = RegexVRSupportFlagMatch.Matches(match.Groups[1].Value.Trim());

				VRSupport.PlayArea.Clear();
				foreach (Match m in matches)
				{
					string playArea = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(playArea))
					{
						VRSupport.PlayArea.Add(playArea);
					}
				}
			}

			// Language Support
			matches = RegexLanguageSupport.Matches(page);
			if (matches.Count > 0)
			{
				LanguageSupport = new LanguageSupport();

				foreach (Match m in matches)
				{
					string language = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
					if (language.StartsWith("#lang") || language.StartsWith("("))
					{
						// Some store pages on steam are bugged.
						continue;
					}

					// Interface
					if (!string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(m.Groups[2].Value.Trim())))
					{
						LanguageSupport.Interface.Add(language);
					}

					// Full Audio
					if (!string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(m.Groups[3].Value.Trim())))
					{
						LanguageSupport.FullAudio.Add(language);
					}

					// Subtitles
					if (!string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(m.Groups[4].Value.Trim())))
					{
						LanguageSupport.Subtitles.Add(language);
					}
				}
			}

			// Get release date
			match = RegexRelDate.Match(page);
			if (match.Success)
			{
				SteamReleaseDate = match.Groups[1].Captures[0].Value;
			}
		}

		#endregion
	}
}
