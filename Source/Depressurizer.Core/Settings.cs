#region License

//     This file (Settings.cs) is part of Depressurizer.
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

using System.IO;
using Newtonsoft.Json;

#endregion

namespace Depressurizer.Core
{
	public enum StartupAction
	{
		None,
		Load,
		Create
	}

	public enum GameListSource
	{
		XmlPreferred,
		XmlOnly,
		WebsiteOnly
	}

	public enum UILanguage
	{
		windows,
		en, // English
		es, // Spanish
		ru, // Russian
		uk, // Ukranian
		nl // Dutch
	}

	public enum StoreLanguage
	{
		windows,
		bg, // Bulgarian
		cs, // Czech
		da, // Danish
		nl, // Dutch
		en, // English
		fi, // Finnish
		fr, // French
		de, // German
		el, // Greek
		hu, // Hungarian
		it, // Italian
		ja, // Japanese
		ko, // Korean
		no, // Norwegian
		pl, // Polish
		pt, // Portuguese
		pt_BR, // Portuguese (Brasil)
		ro, // Romanian
		ru, // Russian
		zh_Hans, // Simplified Chinese
		es, // Spanish
		sv, // Swedish
		th, // Thai
		zh_Hant, // Traditional Chinese
		tr, // Turkish
		uk // Ukrainian
	}

	/// <summary>
	///     Depressurizer settings controller
	/// </summary>
	public sealed class Settings
	{
		#region Static Fields

		private static readonly object SyncRoot = new object();

		private static volatile Settings _instance;

		#endregion

		#region Fields

		public int SplitBrowserContainerWidth = 722;

		public int SplitGameContainerHeight = 510;

		private string _autocats;

		private bool _autosaveDB = true;

		private string _category;

		private bool _checkForDepressurizerUpdates = true;

		private int _configBackupCount = 3;

		private string _filter;

		private int _height;

		private bool _IncludeImputedTimes = true;

		private GameListSource _listSource = GameListSource.XmlPreferred;

		private string _lstGamesState = "";

		private string _profileToLoad;

		private bool _removeExtraEntries = true;

		private int _scrapePromptDays = 30;

		private bool _singleCatMode;
		private int _splitBrowser;

		private int _splitContainer;
		private int _splitGame;

		private StartupAction _startupAction = StartupAction.Create;

		private string _steamPath = null;
		private StoreLanguage _storeLanguage = StoreLanguage.windows;

		private bool _updateAppInfoOnStart = true;

		private bool _updateHLTBOnStart = true;

		private UILanguage _userLanguage = UILanguage.windows;

		private int _width;

		private int _x;

		private int _y;

		#endregion

		#region Constructors and Destructors

		private Settings() { }

		#endregion

		#region Public Properties

		/// <summary>
		///     Settings controller instance
		/// </summary>
		public static Settings Instance
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
						_instance = new Settings();
					}
				}

				return _instance;
			}
		}

		public string AutoCats
		{
			get => _autocats;
			set
			{
				if (_autocats != value)
				{
					_autocats = value;
				}
			}
		}

		public bool AutosaveDB
		{
			get => _autosaveDB;
			set
			{
				if (_autosaveDB != value)
				{
					_autosaveDB = value;
				}
			}
		}

		public string Category
		{
			get => _category;
			set
			{
				lock (SyncRoot)
				{
					_category = value;
				}
			}
		}

		public bool CheckForDepressurizerUpdates
		{
			get => _checkForDepressurizerUpdates;
			set
			{
				lock (SyncRoot)
				{
					_checkForDepressurizerUpdates = value;
				}
			}
		}

		public int ConfigBackupCount
		{
			get => _configBackupCount;
			set
			{
				if (_configBackupCount != value)
				{
					_configBackupCount = value;
				}
			}
		}

		public string Filter
		{
			get => _filter;
			set
			{
				if (_filter != value)
				{
					_filter = value;
				}
			}
		}

		/// <summary>
		///     Height of MainForm
		/// </summary>
		public int Height
		{
			get
			{
				lock (SyncRoot)
				{
					if (_height <= 350)
					{
						_height = 600;
					}

					return _height;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_height = value;
				}
			}
		}

		public bool IncludeImputedTimes
		{
			get => _IncludeImputedTimes;
			set
			{
				if (_IncludeImputedTimes != value)
				{
					_IncludeImputedTimes = value;
				}
			}
		}

		public GameListSource ListSource
		{
			get => _listSource;
			set
			{
				if (_listSource != value)
				{
					_listSource = value;
				}
			}
		}

		public string LstGamesState
		{
			get
			{
				lock (SyncRoot)
				{
					return _lstGamesState;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_lstGamesState = value;
				}
			}
		}

		public string ProfileToLoad
		{
			get => _profileToLoad;
			set
			{
				if (_profileToLoad != value)
				{
					_profileToLoad = value;
				}
			}
		}

		public bool RemoveExtraEntries
		{
			get => _removeExtraEntries;
			set
			{
				if (_removeExtraEntries != value)
				{
					_removeExtraEntries = value;
				}
			}
		}

		public int ScrapePromptDays
		{
			get => _scrapePromptDays;
			set
			{
				if (_scrapePromptDays != value)
				{
					_scrapePromptDays = value;
				}
			}
		}

		public bool SingleCatMode
		{
			get
			{
				lock (SyncRoot)
				{
					return _singleCatMode;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_singleCatMode = value;
				}
			}
		}

		public int SplitBrowser
		{
			get
			{
				if (_splitBrowser <= 100)
				{
					SplitBrowser = SplitBrowserContainerWidth - 300;
				}

				return _splitBrowser;
			}
			set
			{
				if (_splitBrowser != value)
				{
					_splitBrowser = value;
				}
			}
		}

		public int SplitContainer
		{
			get
			{
				if (_splitContainer <= 100)
				{
					SplitContainer = 250;
				}

				return _splitContainer;
			}
			set
			{
				if (_splitContainer != value)
				{
					_splitContainer = value;
				}
			}
		}

		public int SplitGame
		{
			get
			{
				if (_splitGame <= 100)
				{
					SplitGame = SplitGameContainerHeight - 150;
				}

				return _splitGame;
			}
			set
			{
				if (_splitGame != value)
				{
					_splitGame = value;
				}
			}
		}

		public StartupAction StartupAction
		{
			get => _startupAction;
			set
			{
				if (_startupAction != value)
				{
					_startupAction = value;
				}
			}
		}

		/// <summary>
		///     Path to the Steam installation folder.
		/// </summary>
		public string SteamPath
		{
			get
			{
				lock (SyncRoot)
				{
					return _steamPath;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_steamPath = value;
				}
			}
		}

		public StoreLanguage StoreLang
		{
			get => _storeLanguage;
			set
			{
				if (_storeLanguage != value)
				{
					_storeLanguage = value;
					// Database change
				}
			}
		}

		public bool UpdateAppInfoOnStart
		{
			get => _updateAppInfoOnStart;
			set
			{
				if (_updateAppInfoOnStart != value)
				{
					_updateAppInfoOnStart = value;
				}
			}
		}

		/// <summary>
		///     Update HowLongToBeat On Start
		/// </summary>
		public bool UpdateHLTBOnStart
		{
			get
			{
				lock (SyncRoot)
				{
					return _updateHLTBOnStart;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_updateHLTBOnStart = value;
				}
			}
		}

		public UILanguage UserLang
		{
			get => _userLanguage;
			set
			{
				lock (SyncRoot)
				{
					_userLanguage = value;
					// Interface change
				}
			}
		}

		/// <summary>
		///     Width of MainForm
		/// </summary>
		public int Width
		{
			get
			{
				lock (SyncRoot)
				{
					if (_width <= 600)
					{
						_width = 1000;
					}

					return _width;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_width = value;
				}
			}
		}

		/// <summary>
		///     X-Position of MainForm
		/// </summary>
		public int X
		{
			get
			{
				lock (SyncRoot)
				{
					return _x;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_x = value;
				}
			}
		}

		/// <summary>
		///     Y-Position of MainForm
		/// </summary>
		public int Y
		{
			get
			{
				lock (SyncRoot)
				{
					return _y;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_y = value;
				}
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Loads the saved instance from the default location.
		/// </summary>
		public void Load()
		{
			Load("settings.json");
		}

		/// <summary>
		///     Loads the saved instance from the specified location.
		/// </summary>
		/// <param name="path">Path to load from</param>
		public void Load(string path)
		{
			lock (SyncRoot)
			{
				string settings = File.ReadAllText(path);
				_instance = JsonConvert.DeserializeObject<Settings>(settings);
			}
		}

		/// <summary>
		///     Saves the current instance to the default location.
		/// </summary>
		public void Save()
		{
			Save("settings.json");
		}

		/// <summary>
		///     Saves the current instance to the specified location.
		/// </summary>
		/// <param name="path">Path to save to</param>
		public void Save(string path)
		{
			lock (SyncRoot)
			{
				string settings = JsonConvert.SerializeObject(_instance);
				File.WriteAllText(path, settings);
			}
		}

		#endregion
	}
}
