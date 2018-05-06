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

using System.Globalization;
using System.IO;
using System.Threading;
using Depressurizer.Core.Enums;
using Newtonsoft.Json;

#endregion

namespace Depressurizer.Core
{
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

		private bool _autoSaveDatabase = true;

		private bool _checkForUpdates = true;

		private int _configBackupCount = 3;

		private int _height;

		private bool _includeImputedTimes = true;

		private InterfaceLanguage _interfaceLanguage = InterfaceLanguage.English;

		private GameListSource _listSource = GameListSource.XmlPreferred;

		private string _lstGamesState = "";

		private string _profileToLoad;

		private bool _removeExtraEntries = true;

		private int _scrapePromptDays = 30;

		private string _selectedAutoCats;

		private string _selectedCategory;

		private string _selectedFilter;

		private bool _singleCatMode;
		private int _splitBrowser;
		private int _splitBrowserContainerWidth = 722;

		private int _splitContainer;
		private int _splitGame;
		private int _splitGameContainerHeight = 510;

		private StartupAction _startupAction = StartupAction.CreateProfile;

		private string _steamPath = null;
		private StoreLanguage _storeLanguage = StoreLanguage.windows;

		private bool _updateAppInfoOnStart = true;

		private bool _updateHLTBOnStart = true;

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

		/// <summary>
		///     Automatically Save Database
		/// </summary>
		public bool AutoSaveDatabase
		{
			get
			{
				lock (SyncRoot)
				{
					return _autoSaveDatabase;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_autoSaveDatabase = value;
				}
			}
		}

		/// <summary>
		///     Check for Depressurizer updates on startup
		/// </summary>
		public bool CheckForUpdates
		{
			get
			{
				lock (SyncRoot)
				{
					return _checkForUpdates;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_checkForUpdates = value;
				}
			}
		}

		/// <summary>
		///     Num of backups to keep of the Steam config files
		/// </summary>
		public int ConfigBackupCount
		{
			get
			{
				lock (SyncRoot)
				{
					return _configBackupCount;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_configBackupCount = value;
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

		/// <summary>
		///     Include HowLongToBeat imputed times
		/// </summary>
		public bool IncludeImputedTimes
		{
			get
			{
				lock (SyncRoot)
				{
					return _includeImputedTimes;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_includeImputedTimes = value;
				}
			}
		}

		/// <summary>
		///     Depressurizer interface language
		/// </summary>
		public InterfaceLanguage InterfaceLanguage
		{
			get
			{
				lock (SyncRoot)
				{
					return _interfaceLanguage;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_interfaceLanguage = value;
					ChangeInterfaceLanguage(_interfaceLanguage);
				}
			}
		}

		public GameListSource ListSource
		{
			get
			{
				lock (SyncRoot)
				{
					return _listSource;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_listSource = value;
				}
			}
		}

		/// <summary>
		///     Last state of LstGames
		/// </summary>
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

		/// <summary>
		///     Profile to Load
		/// </summary>
		public string ProfileToLoad
		{
			get
			{
				lock (SyncRoot)
				{
					return _profileToLoad;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_profileToLoad = value;
				}
			}
		}

		public bool RemoveExtraEntries
		{
			get
			{
				lock (SyncRoot)
				{
					return _removeExtraEntries;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_removeExtraEntries = value;
				}
			}
		}

		/// <summary>
		///     Re-scrape after X days
		/// </summary>
		public int ScrapePromptDays
		{
			get
			{
				lock (SyncRoot)
				{
					return _scrapePromptDays;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_scrapePromptDays = value;
				}
			}
		}

		/// <summary>
		///     Selected AutoCats
		/// </summary>
		public string SelectedAutoCats
		{
			get
			{
				lock (SyncRoot)
				{
					return _selectedAutoCats;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_selectedAutoCats = value;
				}
			}
		}

		/// <summary>
		///     Selected Category
		/// </summary>
		public string SelectedCategory
		{
			get
			{
				lock (SyncRoot)
				{
					return _selectedCategory;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_selectedCategory = value;
				}
			}
		}

		/// <summary>
		///     Selected Filter
		/// </summary>
		public string SelectedFilter
		{
			get
			{
				lock (SyncRoot)
				{
					return _selectedFilter;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_selectedFilter = value;
				}
			}
		}

		/// <summary>
		///     Single category mode
		/// </summary>
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
				lock (SyncRoot)
				{
					if (_splitBrowser <= 100)
					{
						SplitBrowser = SplitBrowserContainerWidth - 300;
					}

					return _splitBrowser;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_splitBrowser = value;
				}
			}
		}

		public int SplitBrowserContainerWidth
		{
			get
			{
				lock (SyncRoot)
				{
					return _splitBrowserContainerWidth;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_splitBrowserContainerWidth = value;
				}
			}
		}

		public int SplitContainer
		{
			get
			{
				lock (SyncRoot)
				{
					if (_splitContainer <= 100)
					{
						SplitContainer = 250;
					}

					return _splitContainer;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_splitContainer = value;
				}
			}
		}

		public int SplitGame
		{
			get
			{
				lock (SyncRoot)
				{
					if (_splitGame <= 100)
					{
						SplitGame = SplitGameContainerHeight - 150;
					}

					return _splitGame;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_splitGame = value;
				}
			}
		}

		public int SplitGameContainerHeight
		{
			get
			{
				lock (SyncRoot)
				{
					return _splitGameContainerHeight;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_splitGameContainerHeight = value;
				}
			}
		}

		/// <summary>
		///     Action on startup
		/// </summary>
		public StartupAction StartupAction
		{
			get
			{
				lock (SyncRoot)
				{
					return _startupAction;
				}
			}

			set
			{
				lock (SyncRoot)
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

		/// <summary>
		///     Steam Store language
		/// </summary>
		public StoreLanguage StoreLanguage
		{
			get
			{
				lock (SyncRoot)
				{
					return _storeLanguage;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_storeLanguage = value;
				}
			}
		}

		/// <summary>
		///     Update from appinfo on start
		/// </summary>
		public bool UpdateAppInfoOnStart
		{
			get
			{
				lock (SyncRoot)
				{
					return _updateAppInfoOnStart;
				}
			}

			set
			{
				lock (SyncRoot)
				{
					_updateAppInfoOnStart = value;
				}
			}
		}

		/// <summary>
		///     Update HowLongToBeat-times on start
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
				if (!File.Exists(path))
				{
					return;
				}

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

		#region Methods

		private static void ChangeInterfaceLanguage(InterfaceLanguage language)
		{
			CultureInfo newCulture;

			switch (language)
			{
				case InterfaceLanguage.Dutch:
					newCulture = new CultureInfo("nl");
					break;
				case InterfaceLanguage.English:
					newCulture = new CultureInfo("en");
					break;
				case InterfaceLanguage.Russian:
					newCulture = new CultureInfo("ru");
					break;
				case InterfaceLanguage.Spanish:
					newCulture = new CultureInfo("es");
					break;
				case InterfaceLanguage.Ukranian:
					newCulture = new CultureInfo("uk");
					break;
				default:
					newCulture = Thread.CurrentThread.CurrentCulture;
					break;
			}

			Thread.CurrentThread.CurrentUICulture = newCulture;
		}

		#endregion
	}
}
