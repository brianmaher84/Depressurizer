#region License

//     This file (Database.cs) is part of Depressurizer.
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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Depressurizer.Core;
using Depressurizer.Core.Enums;
using Depressurizer.Core.Helpers;
using Depressurizer.Core.Models;
using Depressurizer.Dialogs;
using Depressurizer.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Depressurizer
{
	public sealed class Database
	{
		#region Constants

		// Utility
		private const int Version = 2;

		private const string XmlNameRootNode = "gamelist", XmlNameVersion = "version", XmlNameLastHltbUpdate = "lastHltbUpdate", XmlNameDbLanguage = "Language", XmlNameGames = "games";

		#endregion

		#region Static Fields

		private static readonly object SyncRoot = new object();
		private static volatile Database _instance;

		#endregion

		#region Fields

		// Main Data
		public Dictionary<int, DatabaseEntry> Games { get; }= new Dictionary<int, DatabaseEntry>();
		public int LastHltbUpdate;
		private LanguageSupport _allLanguages;
		private SortedSet<string> _allStoreDevelopers;

		private SortedSet<string> _allStoreFlags;

		// Extra data
		private SortedSet<string> _allStoreGenres;
		private SortedSet<string> _allStorePublishers;
		private VRSupport _allVrSupportFlags;

		#endregion

		#region Constructors and Destructors

		private Database() { }

		#endregion

		#region Public Properties

		public static Database Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}

				lock (SyncRoot)
				{
					if (_instance == null)
					{
						_instance = new Database();
					}
				}

				return _instance;
			}
		}

		public StoreLanguage Language => Settings.Instance.StoreLanguage;

		#endregion

		#region Public Methods and Operators

		public static XmlDocument FetchAppListFromWeb()
		{
			XmlDocument doc = new XmlDocument();
			Program.Logger.Info(GlobalStrings.GameDB_DownloadingSteamAppList);
			WebRequest req = WebRequest.Create(@"http://api.steampowered.com/ISteamApps/GetAppList/v0002/?format=xml");
			using (WebResponse resp = req.GetResponse())
			{
				doc.Load(resp.GetResponseStream());
			}

			Program.Logger.Info(GlobalStrings.GameDB_XMLAppListDownloaded);
			return doc;
		}

		/// <summary>
		///     Gets a list of all Steam store developers found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A set of developers, as strings</returns>
		public SortedSet<string> CalculateAllDevelopers()
		{
			if (_allStoreDevelopers == null)
			{
				_allStoreDevelopers = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_allStoreDevelopers.Clear();
			}

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.Developers != null)
				{
					_allStoreDevelopers.UnionWith(entry.Developers);
				}
			}

			return _allStoreDevelopers;
		}

		/// <summary>
		///     Gets a list of all Steam store genres found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A set of genres, as strings</returns>
		public SortedSet<string> CalculateAllGenres()
		{
			if (_allStoreGenres == null)
			{
				_allStoreGenres = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_allStoreGenres.Clear();
			}

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.Genres != null)
				{
					_allStoreGenres.UnionWith(entry.Genres);
				}
			}

			return _allStoreGenres;
		}

		/// <summary>
		///     Gets a list of all Game Languages found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A LanguageSupport struct containing the languages</returns>
		public LanguageSupport CalculateAllLanguages()
		{
			SortedSet<string> Interface = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			SortedSet<string> subtitles = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			SortedSet<string> fullAudio = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.LanguageSupport.Interface != null)
				{
					Interface.UnionWith(entry.LanguageSupport.Interface);
				}

				if (entry.LanguageSupport.Subtitles != null)
				{
					subtitles.UnionWith(entry.LanguageSupport.Subtitles);
				}

				if (entry.LanguageSupport.FullAudio != null)
				{
					fullAudio.UnionWith(entry.LanguageSupport.FullAudio);
				}
			}

			_allLanguages.Interface.Clear();
			_allLanguages.Interface.AddRange(Interface.ToList());

			_allLanguages.FullAudio.Clear();
			_allLanguages.FullAudio.AddRange(fullAudio.ToList());

			_allLanguages.Subtitles.Clear();
			_allLanguages.Subtitles.AddRange(subtitles.ToList());

			return _allLanguages;
		}

		/// <summary>
		///     Gets a list of all Steam store publishers found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A set of publishers, as strings</returns>
		public SortedSet<string> CalculateAllPublishers()
		{
			if (_allStorePublishers == null)
			{
				_allStorePublishers = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_allStorePublishers.Clear();
			}

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.Publishers != null)
				{
					_allStorePublishers.UnionWith(entry.Publishers);
				}
			}

			return _allStorePublishers;
		}

		/// <summary>
		///     Gets a list of all Steam store flags found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A set of genres, as strings</returns>
		public SortedSet<string> CalculateAllStoreFlags()
		{
			if (_allStoreFlags == null)
			{
				_allStoreFlags = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				_allStoreFlags.Clear();
			}

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.Flags != null)
				{
					_allStoreFlags.UnionWith(entry.Flags);
				}
			}

			return _allStoreFlags;
		}

		/// <summary>
		///     Gets a list of all Steam store VR Support flags found in the entire database.
		///     Always recalculates.
		/// </summary>
		/// <returns>A VRSupport struct containing the flags</returns>
		public VRSupport CalculateAllVrSupportFlags()
		{
			SortedSet<string> headsets = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			SortedSet<string> input = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			SortedSet<string> playArea = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.VRSupport.Headsets != null)
				{
					headsets.UnionWith(entry.VRSupport.Headsets);
				}

				if (entry.VRSupport.Input != null)
				{
					input.UnionWith(entry.VRSupport.Input);
				}

				if (entry.VRSupport.PlayArea != null)
				{
					playArea.UnionWith(entry.VRSupport.PlayArea);
				}
			}

			_allVrSupportFlags.Headsets.Clear();
			_allVrSupportFlags.Headsets.AddRange(headsets.ToList());

			_allVrSupportFlags.Input.Clear();
			_allVrSupportFlags.Input.AddRange(input.ToList());

			_allVrSupportFlags.PlayArea.Clear();
			_allVrSupportFlags.PlayArea.AddRange(playArea.ToList());

			return _allVrSupportFlags;
		}

		/// <summary>
		///     Gets a list of developers found on games with their game count.
		/// </summary>
		/// <param name="filter">
		///     GameList including games to include in the search. If null, finds developers for all games in the
		///     database.
		/// </param>
		/// <param name="minScore">
		///     Minimum count of developers games to include in the result list. Developers with lower game
		///     counts will be discarded.
		/// </param>
		/// <returns>List of developers, as strings with game counts</returns>
		public IEnumerable<Tuple<string, int>> CalculateSortedDevList(GameList filter, int minCount)
		{
			SortedSet<string> developers = GetAllDevelopers();
			Dictionary<string, int> devCounts = new Dictionary<string, int>();
			if (filter == null)
			{
				foreach (DatabaseEntry dbEntry in Games.Values)
				{
					CalculateSortedDevListHelper(devCounts, dbEntry);
				}
			}
			else
			{
				foreach (int gameId in filter.Games.Keys)
				{
					if (Games.ContainsKey(gameId) && !filter.Games[gameId].Hidden)
					{
						CalculateSortedDevListHelper(devCounts, Games[gameId]);
					}
				}
			}

			IEnumerable<Tuple<string, int>> unsortedList = from entry in devCounts where entry.Value >= minCount select new Tuple<string, int>(entry.Key, entry.Value);
			return unsortedList.ToList();
		}

		/// <summary>
		///     Gets a list of publishers found on games with their game count.
		/// </summary>
		/// <param name="filter">
		///     GameList including games to include in the search. If null, finds publishers for all games in the
		///     database.
		/// </param>
		/// <param name="minScore">
		///     Minimum count of publishers games to include in the result list. publishers with lower game
		///     counts will be discarded.
		/// </param>
		/// <returns>List of publishers, as strings with game counts</returns>
		public IEnumerable<Tuple<string, int>> CalculateSortedPubList(GameList filter, int minCount)
		{
			SortedSet<string> publishers = GetAllPublishers();
			Dictionary<string, int> pubCounts = new Dictionary<string, int>();
			if (filter == null)
			{
				foreach (DatabaseEntry dbEntry in Games.Values)
				{
					CalculateSortedPubListHelper(pubCounts, dbEntry);
				}
			}
			else
			{
				foreach (int gameId in filter.Games.Keys)
				{
					if (Games.ContainsKey(gameId) && !filter.Games[gameId].Hidden)
					{
						CalculateSortedPubListHelper(pubCounts, Games[gameId]);
					}
				}
			}

			IEnumerable<Tuple<string, int>> unsortedList = from entry in pubCounts where entry.Value >= minCount select new Tuple<string, int>(entry.Key, entry.Value);
			return unsortedList.ToList();
		}

		/// <summary>
		///     Gets a list of tags found on games, sorted by a popularity score.
		/// </summary>
		/// <param name="filter">
		///     GameList including games to include in the search. If null, finds tags for all games in the
		///     database.
		/// </param>
		/// <param name="weightFactor">
		///     Value of the popularity score contributed by the first processed tag for each game. Each subsequent tag contributes
		///     less to its own score.
		///     The last tag always contributes 1. Value less than or equal to 1 indicates no weighting.
		/// </param>
		/// <param name="minScore">Minimum score of tags to include in the result list. Tags with lower scores will be discarded.</param>
		/// <param name="tagsPerGame">
		///     Maximum tags to find per game. If a game has more tags than this, they will be discarded. 0
		///     indicates no limit.
		/// </param>
		/// <returns>List of tags, as strings</returns>
		public IEnumerable<Tuple<string, float>> CalculateSortedTagList(GameList filter, float weightFactor, int minScore, int tagsPerGame, bool excludeGenres, bool scoreSort)
		{
			SortedSet<string> genreNames = GetAllGenres();
			Dictionary<string, float> tagCounts = new Dictionary<string, float>();
			if (filter == null)
			{
				foreach (DatabaseEntry dbEntry in Games.Values)
				{
					CalculateSortedTagListHelper(tagCounts, dbEntry, weightFactor, tagsPerGame);
				}
			}
			else
			{
				foreach (int gameId in filter.Games.Keys)
				{
					if (Games.ContainsKey(gameId) && !filter.Games[gameId].Hidden)
					{
						CalculateSortedTagListHelper(tagCounts, Games[gameId], weightFactor, tagsPerGame);
					}
				}
			}

			if (excludeGenres)
			{
				foreach (string genre in genreNames)
				{
					tagCounts.Remove(genre);
				}
			}

			IEnumerable<Tuple<string, float>> unsortedList = from entry in tagCounts where entry.Value >= minScore select new Tuple<string, float>(entry.Key, entry.Value);
			IOrderedEnumerable<Tuple<string, float>> sortedList = scoreSort ? from entry in unsortedList orderby entry.Item2 descending select entry : from entry in unsortedList orderby entry.Item1 select entry;
			return sortedList.ToList();
		}

		public void ChangeLanguage(StoreLanguage lang)
		{
			if (Program.Database == null)
			{
				return;
			}

			StoreLanguage dbLang = StoreLanguage.en;
			if (lang == StoreLanguage.windows)
			{
				CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
				if (Enum.GetNames(typeof(StoreLanguage)).ToList().Contains(currentCulture.TwoLetterISOLanguageName))
				{
					dbLang = (StoreLanguage) Enum.Parse(typeof(StoreLanguage), currentCulture.TwoLetterISOLanguageName);
				}
				else
				{
					if (currentCulture.Name == "zh-Hans" || currentCulture.Parent.Name == "zh-Hans")
					{
						dbLang = StoreLanguage.zh_Hans;
					}
					else if (currentCulture.Name == "zh-Hant" || currentCulture.Parent.Name == "zh-Hant")
					{
						dbLang = StoreLanguage.zh_Hant;
					}
					else if (currentCulture.Name == "pt-BR" || currentCulture.Parent.Name == "pt-BR")
					{
						dbLang = StoreLanguage.pt_BR;
					}
				}
			}
			else
			{
				dbLang = lang;
			}

			if (Language == dbLang)
			{
				return;
			}

			foreach (DatabaseEntry entry in Games.Values)
			{
				if (entry.Id <= 0)
				{
					continue;
				}

				entry.Tags.Clear();
				entry.Flags.Clear();
				entry.Genres.Clear();
				entry.SteamReleaseDate = null;
				entry.LastStoreScrape = 1; //pretend it is really old data
				entry.VRSupport = new VRSupport();
				entry.LanguageSupport = new LanguageSupport();
			}

			//Update DB with data in correct language

			if (FormMain.CurrentProfile == null)
			{
				Save("GameDB.xml.gz");
				return;
			}

			SortedSet<int> appIds = new SortedSet<int>();
			foreach (GameInfo game in FormMain.CurrentProfile.GameData.Games.Values)
			{
				if (game.Id <= 0)
				{
					continue;
				}

				appIds.Add(game.Id);
			}

			using (ScrapeDialog dialog = new ScrapeDialog(appIds))
			{
				dialog.ShowDialog();
			}

			Save("GameDB.xml.gz");
		}

		public bool Contains(int id)
		{
			return Games.ContainsKey(id);
		}

		/// <summary>
		///     Gets a list of all Steam store developers found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A set of developers, as strings</returns>
		public SortedSet<string> GetAllDevelopers()
		{
			if (_allStoreDevelopers == null)
			{
				return CalculateAllDevelopers();
			}

			return _allStoreDevelopers;
		}

		/// <summary>
		///     Gets a list of all Steam store genres found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A set of genres, as strings</returns>
		public SortedSet<string> GetAllGenres()
		{
			if (_allStoreGenres == null)
			{
				return CalculateAllGenres();
			}

			return _allStoreGenres;
		}

		/// <summary>
		///     Gets a list of all Game Languages found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A LanguageSupport struct containing the languages</returns>
		public LanguageSupport GetAllLanguages()
		{
			if (_allLanguages.FullAudio == null || _allLanguages.Interface == null || _allLanguages.Subtitles == null)
			{
				return CalculateAllLanguages();
			}

			return _allLanguages;
		}

		/// <summary>
		///     Gets a list of all Steam store publishers found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A set of publishers, as strings</returns>
		public SortedSet<string> GetAllPublishers()
		{
			if (_allStorePublishers == null)
			{
				return CalculateAllPublishers();
			}

			return _allStorePublishers;
		}

		/// <summary>
		///     Gets a list of all Steam store flags found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A set of genres, as strings</returns>
		public SortedSet<string> GetAllStoreFlags()
		{
			if (_allStoreFlags == null)
			{
				return CalculateAllStoreFlags();
			}

			return _allStoreFlags;
		}

		/// <summary>
		///     Gets a list of all Steam store VR Support flags found in the entire database.
		///     Only recalculates if necessary.
		/// </summary>
		/// <returns>A VRSupport struct containing the flags</returns>
		public VRSupport GetAllVrSupportFlags()
		{
			if (_allVrSupportFlags.Headsets == null || _allVrSupportFlags.Input == null || _allVrSupportFlags.PlayArea == null)
			{
				return CalculateAllVrSupportFlags();
			}

			return _allVrSupportFlags;
		}

		public List<string> GetDevelopers(int gameId, int depth = 3)
		{
			if (Games.ContainsKey(gameId))
			{
				List<string> res = Games[gameId].Developers;
				if ((res == null || res.Count == 0) && depth > 0 && Games[gameId].ParentId > 0)
				{
					res = GetDevelopers(Games[gameId].ParentId, depth - 1);
				}

				return res;
			}

			return null;
		}

		public List<string> GetFlagList(int gameId, int depth = 3)
		{
			if (Games.ContainsKey(gameId))
			{
				List<string> res = Games[gameId].Flags;
				if ((res == null || res.Count == 0) && depth > 0 && Games[gameId].ParentId > 0)
				{
					res = GetFlagList(Games[gameId].ParentId, depth - 1);
				}

				return res;
			}

			return null;
		}

		public List<string> GetGenreList(int gameId, int depth = 3, bool tagFallback = true)
		{
			if (Games.ContainsKey(gameId))
			{
				List<string> res = Games[gameId].Genres;
				if (tagFallback && (res == null || res.Count == 0))
				{
					List<string> tags = GetTagList(gameId, 0);
					if (tags != null && tags.Count > 0)
					{
						res = new List<string>(tags.Intersect(GetAllGenres()));
					}
				}

				if ((res == null || res.Count == 0) && depth > 0 && Games[gameId].ParentId > 0)
				{
					res = GetGenreList(Games[gameId].ParentId, depth - 1, tagFallback);
				}

				return res;
			}

			return null;
		}

		public string GetName(int id)
		{
			if (Games.ContainsKey(id))
			{
				return Games[id].Name;
			}

			return null;
		}

		public List<string> GetPublishers(int gameId, int depth = 3)
		{
			if (Games.ContainsKey(gameId))
			{
				List<string> res = Games[gameId].Publishers;
				if ((res == null || res.Count == 0) && depth > 0 && Games[gameId].ParentId > 0)
				{
					res = GetPublishers(Games[gameId].ParentId, depth - 1);
				}

				return res;
			}

			return null;
		}

		public int GetReleaseYear(int gameId)
		{
			if (Games.ContainsKey(gameId))
			{
				DatabaseEntry dbEntry = Games[gameId];
				DateTime releaseDate;
				if (DateTime.TryParse(dbEntry.SteamReleaseDate, out releaseDate))
				{
					return releaseDate.Year;
				}
			}

			return 0;
		}

		public List<string> GetTagList(int gameId, int depth = 3)
		{
			if (Games.ContainsKey(gameId))
			{
				List<string> res = Games[gameId].Tags;
				if ((res == null || res.Count == 0) && depth > 0 && Games[gameId].ParentId > 0)
				{
					res = GetTagList(Games[gameId].ParentId, depth - 1);
				}

				return res;
			}

			return null;
		}

		public VRSupport GetVRSupport(int gameId, int depth = 3)
		{
			if (!Contains(gameId))
			{
				return new VRSupport();
			}

			VRSupport vrSupport = Games[gameId].VRSupport;
			if (vrSupport.Headsets.Count == 0 && vrSupport.Input.Count == 0 && vrSupport.PlayArea.Count == 0 && depth > 0 && Games[gameId].ParentId > 0)
			{
				vrSupport = GetVRSupport(Games[gameId].ParentId, depth - 1);
			}

			return vrSupport;
		}

		public bool IncludeItemInGameList(int appId)
		{
			if (!Contains(appId))
			{
				return false;
			}

			DatabaseEntry entry = Games[appId];
			return entry.AppType == AppType.Application || entry.AppType == AppType.Game;
		}

		public int IntegrateAppList(XmlDocument doc)
		{
			int added = 0;
			foreach (XmlNode node in doc.SelectNodes("/applist/apps/app"))
			{
				int appId;
				if (XmlUtil.TryGetIntFromNode(node["appid"], out appId))
				{
					string gameName = XmlUtil.GetStringFromNode(node["name"], null);
					if (Games.ContainsKey(appId))
					{
						DatabaseEntry g = Games[appId];
						if (string.IsNullOrEmpty(g.Name) || g.Name != gameName)
						{
							g.Name = gameName;
							g.AppType = AppType.Unknown;
						}
					}
					else
					{
						DatabaseEntry g = new DatabaseEntry();
						g.Id = appId;
						g.Name = gameName;
						Games.Add(appId, g);
						added++;
					}
				}
			}

			Program.Logger.Info(GlobalStrings.GameDB_LoadedNewItemsFromAppList, added);
			return added;
		}

		public void Save(string path)
		{
			Save(path, path.EndsWith(".gz"));
		}

		public void Save(string path, bool compress)
		{
			Program.Logger.Info(GlobalStrings.GameDB_SavingGameDBTo, path);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.CloseOutput = true;

			Stream stream = null;
			try
			{
				stream = new FileStream(path, FileMode.Create);

				if (compress)
				{
					stream = new GZipStream(stream, CompressionMode.Compress);
				}

				XmlWriter writer = XmlWriter.Create(stream, settings);
				writer.WriteStartDocument();
				writer.WriteStartElement(XmlNameRootNode);

				writer.WriteElementString(XmlNameVersion, Version.ToString());

				writer.WriteElementString(XmlNameLastHltbUpdate, LastHltbUpdate.ToString());

				writer.WriteElementString(XmlNameDbLanguage, Enum.GetName(typeof(StoreLanguage), Language));

				writer.WriteStartElement(XmlNameGames);
				XmlSerializer x = new XmlSerializer(typeof(DatabaseEntry));
				XmlSerializerNamespaces nameSpace = new XmlSerializerNamespaces();
				nameSpace.Add("", "");
				foreach (DatabaseEntry g in Games.Values)
				{
					x.Serialize(writer, g, nameSpace);
				}

				writer.WriteEndElement(); //XmlName_Games

				writer.WriteEndElement(); //XmlName_RootNode
				writer.WriteEndDocument();
				writer.Close();
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}

			Program.Logger.Info(GlobalStrings.GameDB_GameDBSaved);
		}

		/// <summary>
		///     Returns whether the game supports VR
		/// </summary>
		public bool SupportsVr(int gameId, int depth = 3)
		{
			if (Games.ContainsKey(gameId))
			{
				VRSupport res = Games[gameId].VRSupport;
				if (res.Headsets != null && res.Headsets.Count > 0 || res.Input != null && res.Input.Count > 0 || res.PlayArea != null && res.PlayArea.Count > 0 && depth > 0 && Games[gameId].ParentId > 0)
				{
					return true;
				}

				if (depth > 0 && Games[gameId].ParentId > 0)
				{
					return SupportsVr(Games[gameId].ParentId, depth - 1);
				}
			}

			return false;
		}

		public void UpdateAppListFromWeb()
		{
			XmlDocument doc = FetchAppListFromWeb();
			IntegrateAppList(doc);
		}

		/// <summary>
		///     Updated the database with information from the AppInfo cache file.
		/// </summary>
		/// <param name="path">Path to the cache file</param>
		/// <returns>The number of entries integrated into the database.</returns>
		public int UpdateFromAppInfo(string path)
		{
			int updated = 0;

			Dictionary<int, AppInfo> appInfos = AppInfo.LoadApps(path);
			int timestamp = Utility.GetCurrentUTime();

			foreach (AppInfo aInf in appInfos.Values)
			{
				DatabaseEntry entry;
				if (!Games.ContainsKey(aInf.AppId))
				{
					entry = new DatabaseEntry();
					entry.Id = aInf.AppId;
					Games.Add(entry.Id, entry);
				}
				else
				{
					entry = Games[aInf.AppId];
				}

				entry.LastAppInfoUpdate = timestamp;
				if (aInf.AppType != AppType.Unknown)
				{
					entry.AppType = aInf.AppType;
				}

				if (!string.IsNullOrEmpty(aInf.Name))
				{
					entry.Name = aInf.Name;
				}

				if (entry.Platforms == AppPlatforms.None || entry.LastStoreScrape == 0 && aInf.Platforms > AppPlatforms.None)
				{
					entry.Platforms = aInf.Platforms;
				}

				if (aInf.ParentId > 0)
				{
					entry.ParentId = aInf.ParentId;
				}

				updated++;
			}

			return updated;
		}

		/// <summary>
		///     Update the database with information from howlongtobeatsteam.com.
		/// </summary>
		/// <param name="includeImputedTimes">Whether to include imputed hltb times</param>
		/// <returns>The number of entries integrated into the database.</returns>
		public int UpdateFromHltb(bool includeImputedTimes)
		{
			int updated = 0;

			using (WebClient wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;
				string json = wc.DownloadString(Constants.UrlHLTBAll);
				JObject parsedJson = JObject.Parse(json);
				dynamic games = parsedJson.SelectToken("Games");
				foreach (dynamic g in games)
				{
					dynamic steamAppData = g.SteamAppData;
					int id = steamAppData.SteamAppId;
					if (Games.ContainsKey(id))
					{
						dynamic htlbInfo = steamAppData.HltbInfo;

						if (!includeImputedTimes && htlbInfo.MainTtbImputed == "True")
						{
							Games[id].HltbMain = 0;
						}
						else
						{
							Games[id].HltbMain = htlbInfo.MainTtb;
						}

						if (!includeImputedTimes && htlbInfo.ExtrasTtbImputed == "True")
						{
							Games[id].HltbExtras = 0;
						}
						else
						{
							Games[id].HltbExtras = htlbInfo.ExtrasTtb;
						}

						if (!includeImputedTimes && htlbInfo.CompletionistTtbImputed == "True")
						{
							Games[id].HltbCompletionist = 0;
						}
						else
						{
							Games[id].HltbCompletionist = htlbInfo.CompletionistTtb;
						}

						updated++;
					}
				}
			}

			LastHltbUpdate = Utility.GetCurrentUTime();
			return updated;
		}

		#endregion

		#region Methods

		/// <summary>
		///     Counts games for each developer.
		/// </summary>
		/// <param name="counts">
		///     Existing dictionary of developers and game count. Key is the developer as a string, value is the
		///     count
		/// </param>
		/// <param name="dbEntry">Entry to add developers from</param>
		private void CalculateSortedDevListHelper(Dictionary<string, int> counts, DatabaseEntry dbEntry)
		{
			if (dbEntry.Developers != null)
			{
				for (int i = 0; i < dbEntry.Developers.Count; i++)
				{
					string dev = dbEntry.Developers[i];
					if (counts.ContainsKey(dev))
					{
						counts[dev] += 1;
					}
					else
					{
						counts[dev] = 1;
					}
				}
			}
		}

		/// <summary>
		///     Counts games for each publisher.
		/// </summary>
		/// <param name="counts">
		///     Existing dictionary of publishers and game count. Key is the publisher as a string, value is the
		///     count
		/// </param>
		/// <param name="dbEntry">Entry to add publishers from</param>
		private void CalculateSortedPubListHelper(Dictionary<string, int> counts, DatabaseEntry dbEntry)
		{
			if (dbEntry.Publishers != null)
			{
				for (int i = 0; i < dbEntry.Publishers.Count; i++)
				{
					string pub = dbEntry.Publishers[i];
					if (counts.ContainsKey(pub))
					{
						counts[pub] += 1;
					}
					else
					{
						counts[pub] = 1;
					}
				}
			}
		}

		/// <summary>
		///     Adds tags from the given DBEntry to the dictionary. Adds new elements if necessary, and increases values on
		///     existing elements.
		/// </summary>
		/// <param name="counts">Existing dictionary of tags and scores. Key is the tag as a string, value is the score</param>
		/// <param name="dbEntry">Entry to add tags from</param>
		/// <param name="weightFactor">
		///     The score value of the first tag in the list.
		///     The first tag on the game will have this score, and the last tag processed will always have score 1.
		///     The tags between will have linearly interpolated values between them.
		/// </param>
		/// <param name="tagsPerGame"></param>
		private void CalculateSortedTagListHelper(Dictionary<string, float> counts, DatabaseEntry dbEntry, float weightFactor, int tagsPerGame)
		{
			if (dbEntry.Tags != null)
			{
				int tagsToLoad = tagsPerGame == 0 ? dbEntry.Tags.Count : Math.Min(tagsPerGame, dbEntry.Tags.Count);
				for (int i = 0; i < tagsToLoad; i++)
				{
					// Get the score based on the weighting factor
					float score = 1;
					if (weightFactor > 1)
					{
						if (tagsToLoad <= 1)
						{
							score = weightFactor;
						}
						else
						{
							float interp = i / (float) (tagsToLoad - 1);
							score = (1 - interp) * weightFactor + interp;
						}
					}

					string tag = dbEntry.Tags[i];
					if (counts.ContainsKey(tag))
					{
						counts[tag] += score;
					}
					else
					{
						counts[tag] = score;
					}
				}
			}
		}

		private void ClearAggregates()
		{
			_allStoreGenres = null;
			_allStoreFlags = null;
			_allStoreDevelopers = null;
			_allStorePublishers = null;
		}

		/// <summary>
		///     Reads GameDbEntry objects from selected node and adds them to GameDb
		///     Legacy method used to read data from version 1 of the database.
		/// </summary>
		/// <param name="gameListNode">Node containing GameDbEntry objects with game as element name</param>
		private void LoadGamelistVersion1(XmlNode gameListNode)
		{
			const string xmlNameGame = "game", xmlNameGameId = "id", xmlNameGameName = "name", xmlNameGameLastStoreUpdate = "lastStoreUpdate", xmlNameGameLastAppInfoUpdate = "lastAppInfoUpdate", xmlNameGameType = "type", xmlNameGamePlatforms = "platforms", xmlNameGameParent = "parent", xmlNameGameGenre = "genre", xmlNameGameTag = "tag", xmlNameGameAchievements = "achievements", xmlNameGameDeveloper = "developer", xmlNameGamePublisher = "publisher", xmlNameGameFlag = "flag", xmlNameGameReviewTotal = "reviewTotal", xmlNameGameReviewPositivePercent = "reviewPositiveP", xmlNameGameMcUrl = "mcUrl", xmlNameGameDate = "steamDate", xmlNameGameHltbMain = "hltbMain", xmlNameGameHltbExtras = "hltbExtras", xmlNameGameHltbCompletionist = "hltbCompletionist", xmlNameGameVRSupport = "vrSupport", xmlNameGameVRSupportHeadsets = "Headset", xmlNameGameVRSupportInput = "Input", xmlNameGameVRSupportPlayArea = "PlayArea", xmlNameGameLanguageSupport = "languageSupport", xmlNameGameLanguageSupportInterface = "Headset", xmlNameGameLanguageSupportFullAudio = "Input", xmlNameGameLanguageSupportSubtitles = "PlayArea";

			foreach (XmlNode gameNode in gameListNode.SelectNodes(xmlNameGame))
			{
				int id;
				if (!XmlUtil.TryGetIntFromNode(gameNode[xmlNameGameId], out id) || Games.ContainsKey(id))
				{
					continue;
				}

				DatabaseEntry g = new DatabaseEntry();
				g.Id = id;

				g.Name = XmlUtil.GetStringFromNode(gameNode[xmlNameGameName], null);

				g.AppType = XmlUtil.GetEnumFromNode(gameNode[xmlNameGameType], AppType.Unknown);

				g.Platforms = XmlUtil.GetEnumFromNode(gameNode[xmlNameGamePlatforms], AppPlatforms.All);

				g.ParentId = XmlUtil.GetIntFromNode(gameNode[xmlNameGameParent], -1);

				g.Genres.Clear();
				g.Genres.AddRange(XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(xmlNameGameGenre)));

				g.Tags.Clear();
				g.Tags.AddRange(XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(xmlNameGameTag)));

				foreach (XmlNode vrNode in gameNode.SelectNodes(xmlNameGameVRSupport))
				{
					g.VRSupport.Headsets.Clear();
					g.VRSupport.Headsets.AddRange(XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(xmlNameGameVRSupportHeadsets)));

					g.VRSupport.Input.Clear();
					g.VRSupport.Input.AddRange(XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(xmlNameGameVRSupportInput)));

					g.VRSupport.PlayArea.Clear();
					g.VRSupport.PlayArea.AddRange(XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(xmlNameGameVRSupportPlayArea)));
				}

				foreach (XmlNode langNode in gameNode.SelectNodes(xmlNameGameLanguageSupport))
				{
					g.LanguageSupport.Interface.Clear();
					g.LanguageSupport.Interface.AddRange(XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(xmlNameGameLanguageSupportInterface)));

					g.LanguageSupport.FullAudio.Clear();
					g.LanguageSupport.FullAudio.AddRange(XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(xmlNameGameLanguageSupportFullAudio)));

					g.LanguageSupport.Subtitles.Clear();
					g.LanguageSupport.Subtitles.AddRange(XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(xmlNameGameLanguageSupportSubtitles)));
				}

				g.Developers.Clear();
				g.Developers.AddRange(XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(xmlNameGameDeveloper)));

				g.Publishers.Clear();
				g.Publishers.AddRange(XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(xmlNameGamePublisher)));

				g.SteamReleaseDate = XmlUtil.GetStringFromNode(gameNode[xmlNameGameDate], null);

				g.Flags.Clear();
				g.Flags.AddRange(XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(xmlNameGameFlag)));

				g.TotalAchievements = XmlUtil.GetIntFromNode(gameNode[xmlNameGameAchievements], 0);

				g.ReviewTotal = XmlUtil.GetIntFromNode(gameNode[xmlNameGameReviewTotal], 0);
				g.ReviewPositivePercentage = XmlUtil.GetIntFromNode(gameNode[xmlNameGameReviewPositivePercent], 0);

				g.MetacriticUrl = XmlUtil.GetStringFromNode(gameNode[xmlNameGameMcUrl], null);

				g.LastAppInfoUpdate = XmlUtil.GetIntFromNode(gameNode[xmlNameGameLastAppInfoUpdate], 0);
				g.LastStoreScrape = XmlUtil.GetIntFromNode(gameNode[xmlNameGameLastStoreUpdate], 0);

				g.HltbMain = XmlUtil.GetIntFromNode(gameNode[xmlNameGameHltbMain], 0);
				g.HltbExtras = XmlUtil.GetIntFromNode(gameNode[xmlNameGameHltbExtras], 0);
				g.HltbCompletionist = XmlUtil.GetIntFromNode(gameNode[xmlNameGameHltbCompletionist], 0);

				Games.Add(id, g);
			}
		}

		public bool Load()
		{
			return Load("database.json");
		}

		private static Logger Logger => Logger.Instance;

		public bool Load(string path)
		{
			lock (SyncRoot)
			{
				Logger.Info("Loading database from \"{0}\"", path);

				if (!File.Exists(path))
				{
					Logger.Warn("Database not found at \"{0}\"", path);
					return false;
				}

				string database = File.ReadAllText(path);
				_instance = JsonConvert.DeserializeObject<Database>(database);

				Logger.Info("Processed database, loading completed. Language {0}", Language);

				return true;
			}
		}

		#endregion
	}
}
