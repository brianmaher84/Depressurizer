﻿#region License

//     This file (GameDBEntry.cs) is part of Depressurizer.
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Depressurizer.Core;
using Depressurizer.Core.Models;
using Depressurizer.Properties;
using Rallion;

#endregion

namespace Depressurizer
{
	[XmlRoot(ElementName = "game")]
	public class GameDBEntry
	{
		#region Static Fields

		private static readonly Regex regAchievements = new Regex(@"<div (?:id=""achievement_block"" ?|class=""block responsive_apppage_details_right"" ?){2}>\s*<div class=""block_title"">[^\d]*(\d+)[^\d<]*</div>\s*<div class=""communitylink_achievement_images"">", RegexOptions.Compiled);

		private static readonly Regex regDevelopers = new Regex(@"(<a href=""http://store\.steampowered\.com/search/\?developer=[^""]*"">([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		private static readonly Regex regDLCcheck = new Regex(@"<img class=""category_icon"" src=""http://store\.akamai\.steamstatic\.com/public/images/v6/ico/ico_dlc\.png"">", RegexOptions.Compiled);

		private static readonly Regex regFlags = new Regex(@"<a class=""name"" href=""http://store\.steampowered\.com/search/\?category2=.*?"">([^<]*)</a>", RegexOptions.Compiled);

		// If these regexes maches a store page, the app is a game, software or dlc respectively
		private static readonly Regex regGamecheck = new Regex(@"<a href=""http://store\.steampowered\.com/search/\?term=&snr=", RegexOptions.Compiled);

		private static readonly Regex regGenre = new Regex(@"<div class=""details_block"">\s*<b>[^:]*:</b>.*?<br>\s*<b>[^:]*:</b>\s*(<a href=""http://store\.steampowered\.com/genre/[^>]*>([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		//Language Support

		private static readonly Regex regLanguageSupport = new Regex(@"<td style=""width: 94px; text-align: left"" class=""ellipsis"">\s*([^<]*)\s*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>[\s\n\r]*<td class=""checkcol"">[\s\n\r]*(.*)[\s\n\r]*<\/td>", RegexOptions.Compiled);

		private static readonly Regex regMetalink = new Regex(@"<div id=""game_area_metalink"">\s*<a href=""http://www\.metacritic\.com/game/pc/([^""]*)\?ftag=", RegexOptions.Compiled);

		private static readonly Regex regPlatformLinux = new Regex(@"<span class=""platform_img linux""></span>", RegexOptions.Compiled);

		private static readonly Regex regPlatformMac = new Regex(@"<span class=""platform_img mac""></span>", RegexOptions.Compiled);

		//Platform Support

		private static readonly Regex regPlatformWindows = new Regex(@"<span class=""platform_img win""></span>", RegexOptions.Compiled);

		private static readonly Regex regPublishers = new Regex(@"(<a href=""http://store\.steampowered\.com/search/\?publisher=[^""]*"">([^<]+)</a>,?\s*)+\s*<br>", RegexOptions.Compiled);

		private static readonly Regex regRelDate = new Regex(@"<div class=""release_date"">\s*<div[^>]*>[^<]*<\/div>\s*<div class=""date"">([^<]+)<\/div>", RegexOptions.Compiled);

		private static readonly Regex regReviews = new Regex(@"<span class=""(?:nonresponsive_hidden ?| responsive_reviewdesc ?){2}"">[^\d]*(\d+)%[^\d]*([\d.,]+)[^\d]*\s*</span>", RegexOptions.Compiled);

		private static readonly Regex regSoftwarecheck = new Regex(@"<a href=""http://store\.steampowered\.com/search/\?category1=994&snr=", RegexOptions.Compiled);

		private static readonly Regex regTags = new Regex(@"<a[^>]*class=""app_tag""[^>]*>([^<]*)</a>", RegexOptions.Compiled);

		private static readonly Regex regVrSupportFlagMatch = new Regex(@"<div class=""game_area_details_specs"">.*?<a class=""name"" href=""http:\/\/store\.steampowered\.com\/search\/\?vrsupport=\d*"">([^<]*)<\/a><\/div>", RegexOptions.Compiled);

		//VR Support
		//regVrSupportHeadsetsSection, regVrSupportInputSection and regVrSupportPlayAreaSection match the whole Headsets, Input and Play Area sections respectively
		//regVrSupportFlagMatch matches the flags inside those sections
		private static readonly Regex regVrSupportHeadsetsSection = new Regex(@"<div class=""details_block vrsupport"">(.*)<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">", RegexOptions.Compiled);

		private static readonly Regex regVrSupportInputSection = new Regex(@"<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">(.*)<div class=""details_block vrsupport"">", RegexOptions.Compiled);

		private static readonly Regex regVrSupportPlayAreaSection = new Regex(@"<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">.*<div class=""details_block vrsupport"">(.*)", RegexOptions.Compiled);

		#endregion

		#region Fields

		[DefaultValue(AppTypes.Unknown)] public AppTypes AppType = AppTypes.Unknown;

		[XmlIgnore] public string Banner = null;

		[DefaultValue(null), XmlElement("Genre")]
		public List<string> Genres = new List<string>();

		[DefaultValue(null), XmlElement("Developer")]
		public List<string> Developers = new List<string>();

		[DefaultValue(null), XmlArrayItem("Flag")]
		public List<string> Flags = new List<string>();

		// Basics:

		[DefaultValue(0)] public int HltbCompletionist;

		[DefaultValue(0)] public int HltbExtras;

		//howlongtobeat.com times
		[DefaultValue(0)] public int HltbMain;

		public int Id;

		public LanguageSupport LanguageSupport
		{
			get => _languageSupport ?? (_languageSupport = new LanguageSupport());
			set => _languageSupport = value;
		}

		[DefaultValue(0)] public int LastAppInfoUpdate;

		[DefaultValue(0)] public int LastStoreScrape;

		// Metacritic:
		[DefaultValue(null)] public string MetacriticUrl;

		public string Name;

		[DefaultValue(-1)] public int ParentId = -1;

		public AppPlatforms Platforms = AppPlatforms.None;

		[DefaultValue(null), XmlElement("Publisher")]
		public List<string> Publishers = new List<string>();

		[DefaultValue(0)] public int ReviewPositivePercentage;

		[DefaultValue(0)] public int ReviewTotal;

		[DefaultValue(null)] public string SteamReleaseDate;

		[DefaultValue(null), XmlArrayItem("Tag")]
		public List<string> Tags = new List<string>();

		[DefaultValue(0)] public int TotalAchievements;
		private VRSupport _vrSupport;
		private LanguageSupport _languageSupport;

		#endregion

		#region Public Properties

		public VRSupport VRSupport
		{
			get => _vrSupport ?? (_vrSupport = new VRSupport());
			set => _vrSupport = value;
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Merges in data from another entry. Useful for merging scrape results, but could also merge data from a different
		///     database.
		///     Uses newer data when there is a conflict.
		///     Does NOT perform deep copies of list fields.
		/// </summary>
		/// <param name="other">GameDBEntry containing info to be merged into this entry.</param>
		public void MergeIn(GameDBEntry other)
		{
			bool useAppInfoFields = other.LastAppInfoUpdate > LastAppInfoUpdate || LastAppInfoUpdate == 0 && other.LastStoreScrape >= LastStoreScrape;
			bool useScrapeOnlyFields = other.LastStoreScrape >= LastStoreScrape;

			if (other.AppType != AppTypes.Unknown && (AppType == AppTypes.Unknown || useAppInfoFields))
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
					Genres = other.Genres;
				}

				if (other.Flags != null && other.Flags.Count > 0)
				{
					Flags = other.Flags;
				}

				if (other.Tags != null && other.Tags.Count > 0)
				{
					Tags = other.Tags;
				}

				if (other.Developers != null && other.Developers.Count > 0)
				{
					Developers = other.Developers;
				}

				if (other.Publishers != null && other.Publishers.Count > 0)
				{
					Publishers = other.Publishers;
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

		/// <summary>
		///     Scrapes the store page with this game entry's ID and updates this entry with the information found.
		/// </summary>
		/// <returns>The type determined during the scrape</returns>
		public AppTypes ScrapeStore()
		{
			AppTypes result = ScrapeStoreHelper(Id);
			SetTypeFromStoreScrape(result);
			return result;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Applies all data from a steam store page to this entry
		/// </summary>
		/// <param name="page">The full result of the HTTP request.</param>
		private void GetAllDataFromPage(string page)
		{
			// Genres
			Match m = regGenre.Match(page);
			if (m.Success)
			{
				Genres = new List<string>();
				foreach (Capture cap in m.Groups[2].Captures)
				{
					Genres.Add(cap.Value);
				}
			}

			// Flags
			MatchCollection matches = regFlags.Matches(page);
			if (matches.Count > 0)
			{
				Flags = new List<string>();
				foreach (Match ma in matches)
				{
					string flag = ma.Groups[1].Value;
					if (!string.IsNullOrWhiteSpace(flag))
					{
						Flags.Add(flag);
					}
				}
			}

			//Tags
			matches = regTags.Matches(page);
			if (matches.Count > 0)
			{
				Tags = new List<string>();
				foreach (Match ma in matches)
				{
					string tag = WebUtility.HtmlDecode(ma.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(tag))
					{
						Tags.Add(tag);
					}
				}
			}

			//Get VR Support headsets
			m = regVrSupportHeadsetsSection.Match(page);
			if (m.Success)
			{
				matches = regVrSupportFlagMatch.Matches(m.Groups[1].Value.Trim());

				VRSupport.Headsets.Clear();
				foreach (Match match in matches)
				{
					string headset = WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(headset))
					{
						VRSupport.Headsets.Add(headset);
					}
				}
			}

			//Get VR Support Input
			m = regVrSupportInputSection.Match(page);
			if (m.Success)
			{
				matches = regVrSupportFlagMatch.Matches(m.Groups[1].Value.Trim());

				VRSupport.Input.Clear();
				foreach (Match match in matches)
				{
					string input = WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(input))
					{
						VRSupport.Input.Add(input);
					}
				}
			}

			//Get VR Support Play Area
			m = regVrSupportPlayAreaSection.Match(page);
			if (m.Success)
			{
				matches = regVrSupportFlagMatch.Matches(m.Groups[1].Value.Trim());

				VRSupport.PlayArea.Clear();
				foreach (Match match in matches)
				{
					string playArea = WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
					if (!string.IsNullOrWhiteSpace(playArea))
					{
						VRSupport.PlayArea.Add(playArea);
					}
				}
			}

			//Get Language Support
			matches = regLanguageSupport.Matches(page);
			if (matches.Count > 0)
			{
				LanguageSupport = new LanguageSupport();

				foreach (Match match in matches)
				{
					string language = WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
					if (language.StartsWith("#lang") || language.StartsWith("("))
					{
						continue; //Some store pages on steam are bugged.
					}

					// Interface
					if (WebUtility.HtmlDecode(match.Groups[2].Value.Trim()) != "")
					{
						LanguageSupport.Interface.Add(language);
					}

					// Full Audio
					if (WebUtility.HtmlDecode(match.Groups[3].Value.Trim()) != "") 
					{
						LanguageSupport.FullAudio.Add(language);
					}

					// Subtitles
					if (WebUtility.HtmlDecode(match.Groups[4].Value.Trim()) != "")
					{
						LanguageSupport.Subtitles.Add(language);
					}
				}
			}

			//Get Achievement number
			m = regAchievements.Match(page);
			if (m.Success)
			{
				//sometimes games have achievements but don't have the "Steam Achievements" flag in the store
				if (!Flags.Contains("Steam Achievements"))
				{
					Flags.Add("Steam Achievements");
				}

				int num = 0;
				if (int.TryParse(m.Groups[1].Value, out num))
				{
					TotalAchievements = num;
				}
			}

			// Get Developer
			m = regDevelopers.Match(page);
			if (m.Success)
			{
				Developers = new List<string>();
				foreach (Capture cap in m.Groups[2].Captures)
				{
					Developers.Add(WebUtility.HtmlDecode(cap.Value));
				}
			}

			// Get Publishers
			m = regPublishers.Match(page);
			if (m.Success)
			{
				Publishers = new List<string>();
				foreach (Capture cap in m.Groups[2].Captures)
				{
					Publishers.Add(WebUtility.HtmlDecode(cap.Value));
				}
			}

			// Get release date
			m = regRelDate.Match(page);
			if (m.Success)
			{
				SteamReleaseDate = m.Groups[1].Captures[0].Value;
			}

			// Get user review data
			m = regReviews.Match(page);
			if (m.Success)
			{
				int num = 0;
				if (int.TryParse(m.Groups[1].Value, out num))
				{
					ReviewPositivePercentage = num;
				}

				if (int.TryParse(m.Groups[2].Value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out num))
				{
					ReviewTotal = num;
				}
			}

			// Get metacritic url
			m = regMetalink.Match(page);
			if (m.Success)
			{
				MetacriticUrl = m.Groups[1].Captures[0].Value;
			}

			// Get Platforms
			m = regPlatformWindows.Match(page);
			if (m.Success)
			{
				Platforms |= AppPlatforms.Windows;
			}

			m = regPlatformMac.Match(page);
			if (m.Success)
			{
				Platforms |= AppPlatforms.Mac;
			}

			m = regPlatformLinux.Match(page);
			if (m.Success)
			{
				Platforms |= AppPlatforms.Linux;
			}
		}

		private HttpWebRequest GetSteamRequest(string url)
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

		/// <summary>
		///     Private helper function to perform scraping work. Downloads the given store page and updates the entry with all
		///     information found.
		/// </summary>
		/// <param name="id">The id of the store page to scrape</param>
		/// <returns>The type determined during the scrape</returns>
		private AppTypes ScrapeStoreHelper(int id)
		{
			Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_InitiatingStoreScrapeForGame, id);

			string page = "";

			int redirectTarget = -1;

			int oldTime = LastStoreScrape;

			LastStoreScrape = Utility.GetCurrentUTime();

			HttpWebResponse resp = null;
			try
			{
				string storeLanguage = "en";
				if (Program.GameDB != null)
				{
					if (Program.GameDB.Language == StoreLanguage.windows)
					{
						storeLanguage = "english";
					}
					else if (Program.GameDB.Language == StoreLanguage.zh_Hans)
					{
						storeLanguage = "schinese";
					}
					else if (Program.GameDB.Language == StoreLanguage.zh_Hant)
					{
						storeLanguage = "tchinese";
					}
					else if (Program.GameDB.Language == StoreLanguage.pt_BR)
					{
						storeLanguage = "brazilian";
					}
					else
					{
						storeLanguage = CultureInfo.GetCultureInfo(Enum.GetName(typeof(StoreLanguage), Program.GameDB.Language)).EnglishName.ToLowerInvariant();
					}
				}

				HttpWebRequest req = GetSteamRequest(string.Format(Resources.UrlSteamStoreApp + "?l=" + storeLanguage, id));
				resp = (HttpWebResponse) req.GetResponse();

				int count = 0;
				while (resp.StatusCode == HttpStatusCode.Found && count < 5)
				{
					resp.Close();
					if (resp.Headers[HttpResponseHeader.Location] == Resources.UrlSteamStore)
					{
						// If we are redirected to the store front page
						Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingRedirectedToMainStorePage, id);
						SetTypeFromStoreScrape(AppTypes.Unknown);
						return AppTypes.Unknown;
					}

					if (resp.ResponseUri.ToString() == resp.Headers[HttpResponseHeader.Location])
					{
						//If page redirects to itself
						Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_RedirectsToItself, id);
						return AppTypes.Unknown;
					}

					req = GetSteamRequest(resp.Headers[HttpResponseHeader.Location]);
					resp = (HttpWebResponse) req.GetResponse();
					count++;
				}

				if (count == 5 && resp.StatusCode == HttpStatusCode.Found)
				{
					//If we got too many redirects
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_TooManyRedirects, id);
					return AppTypes.Unknown;
				}
				else if (resp.ResponseUri.Segments.Length < 2)
				{
					// If we were redirected to the store front page
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingRedirectedToMainStorePage, id);
					SetTypeFromStoreScrape(AppTypes.Unknown);
					return AppTypes.Unknown;
				}
				else if (resp.ResponseUri.Segments[1] == "agecheck/")
				{
					// If we encountered an age gate (cookies should bypass this, but sometimes they don't seem to)
					if (resp.ResponseUri.Segments.Length >= 4 && resp.ResponseUri.Segments[3].TrimEnd('/') != id.ToString())
					{
						// Age check + redirect
						Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingHitAgeCheck, id, resp.ResponseUri.Segments[3].TrimEnd('/'));
						if (int.TryParse(resp.ResponseUri.Segments[3].TrimEnd('/'), out redirectTarget)) { }
						else
						{
							// If we got an age check without numeric id (shouldn't happen)
							return AppTypes.Unknown;
						}
					}
					else
					{
						// If we got an age check with no redirect
						Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingAgeCheckNoRedirect, id);
						return AppTypes.Unknown;
					}
				}
				else if (resp.ResponseUri.Segments[1] != "app/")
				{
					// Redirected outside of the app path
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingRedirectedToNonApp, id);
					return AppTypes.Other;
				}
				else if (resp.ResponseUri.Segments.Length < 3)
				{
					// The URI ends with "/app/" ?
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_Log_ScrapingNoAppId, id);
					return AppTypes.Unknown;
				}
				else if (resp.ResponseUri.Segments[2].TrimEnd('/') != id.ToString())
				{
					// Redirected to a different app id
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingRedirectedToOtherApp, id, resp.ResponseUri.Segments[2].TrimEnd('/'));
					if (!int.TryParse(resp.ResponseUri.Segments[2].TrimEnd('/'), out redirectTarget))
					{
						// if new app id is an actual number
						return AppTypes.Unknown;
					}
				}

				StreamReader sr = new StreamReader(resp.GetResponseStream());
				page = sr.ReadToEnd();
				Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingPageRead, id);
			}
			catch (Exception e)
			{
				// Something went wrong with the download.
				Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingPageReadFailed, id, e.Message);
				LastStoreScrape = oldTime;
				return AppTypes.Unknown;
			}
			finally
			{
				if (resp != null)
				{
					resp.Close();
				}
			}

			AppTypes result = AppTypes.Unknown;

			if (page.Contains("<title>Site Error</title>"))
			{
				Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingReceivedSiteError, id);
				result = AppTypes.Unknown;
			}
			else if (regGamecheck.IsMatch(page) || regSoftwarecheck.IsMatch(page))
			{
				// Here we should have an app, but make sure.

				GetAllDataFromPage(page);

				// Check whether it's DLC and return appropriately
				if (regDLCcheck.IsMatch(page))
				{
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingParsedDLC, id, string.Join(",", Genres));
					result = AppTypes.DLC;
				}
				else
				{
					Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingParsed, id, string.Join(",", Genres));
					result = regSoftwarecheck.IsMatch(page) ? AppTypes.Application : AppTypes.Game;
				}
			}
			else
			{
				// The URI is right, but it didn't pass the regex check
				Program.Logger.Write(LoggerLevel.Verbose, GlobalStrings.GameDB_ScrapingCouldNotParse, id);
				result = AppTypes.Unknown;
			}

			if (redirectTarget != -1)
			{
				ParentId = redirectTarget;
				result = AppTypes.Unknown;
			}

			return result;
		}

		/// <summary>
		///     Updates the game's type with a type determined during a store scrape. Makes sure that better data (AppInfo type)
		///     isn't overwritten with worse data.
		/// </summary>
		/// <param name="typeFromStore">Type found from the store scrape</param>
		private void SetTypeFromStoreScrape(AppTypes typeFromStore)
		{
			if (AppType == AppTypes.Unknown || typeFromStore != AppTypes.Unknown && LastAppInfoUpdate == 0)
			{
				AppType = typeFromStore;
			}
		}

		#endregion
	}
}