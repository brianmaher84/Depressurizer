#region License

//     This file (DlgOptions.cs) is part of Depressurizer.
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
using System.Windows.Forms;
using Depressurizer.Core;
using Rallion;
using Enum = System.Enum;

#endregion

namespace Depressurizer
{
	public partial class DlgOptions : Form
	{
		#region Constructors and Destructors

		public DlgOptions()
		{
			InitializeComponent();

			// Set up help tooltips
			ttHelp.Ext_SetToolTip(helpIncludeImputedTimes, GlobalStrings.DlgOptions_Help_IncludeImputedTimes);
		}

		#endregion

		#region Properties

		private static Settings Settings => Settings.Instance;

		#endregion

		#region Methods

		private void cmdAccept_Click(object sender, EventArgs e)
		{
			SaveFieldsToSettings();
			Close();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cmdDefaultProfileBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			DialogResult res = dlg.ShowDialog();
			if (res == DialogResult.OK)
			{
				txtDefaultProfile.Text = dlg.FileName;
			}
		}

		private void cmdSteamPathBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			DialogResult res = dlg.ShowDialog();
			if (res == DialogResult.OK)
			{
				txtSteamPath.Text = dlg.SelectedPath;
			}
		}

		private void FillFieldsFromSettings()
		{
			txtSteamPath.Text = Settings.SteamPath;
			txtDefaultProfile.Text = Settings.ProfileToLoad;
			switch (Settings.StartupAction)
			{
				case StartupAction.LoadProfile:
					radLoad.Checked = true;
					break;
				case StartupAction.CreateProfile:
					radCreate.Checked = true;
					break;
				default:
					radNone.Checked = true;
					break;
			}

			switch (Settings.ListSource)
			{
				case GameListSource.XmlPreferred:
					cmbDatSrc.SelectedIndex = 0;
					break;
				case GameListSource.XmlOnly:
					cmbDatSrc.SelectedIndex = 1;
					break;
				case GameListSource.WebsiteOnly:
					cmbDatSrc.SelectedIndex = 2;
					break;
			}

			chkUpdateAppInfoOnStartup.Checked = Settings.UpdateAppInfoOnStart;
			chkUpdateHltbOnStartup.Checked = Settings.UpdateHLTBOnStart;
			chkIncludeImputedTimes.Checked = Settings.IncludeImputedTimes;
			chkAutosaveDB.Checked = Settings.AutoSaveDatabase;
			numScrapePromptDays.Value = Settings.ScrapePromptDays;

			chkCheckForDepressurizerUpdates.Checked = Settings.CheckForUpdates;

			chkRemoveExtraEntries.Checked = Settings.RemoveExtraEntries;

			//supported languages have an enum value of 1-5 (en, es, ru, uk, nl). 0 is windows language.
			cmbUILanguage.SelectedIndex = (int) Settings.InterfaceLanguage;
			cmbStoreLanguage.SelectedIndex = (int) Settings.StoreLanguage;
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
			string[] levels = Enum.GetNames(typeof(LoggerLevel));
			cmbLogLevel.Items.AddRange(levels);

			foreach (string language in Enum.GetNames(typeof(InterfaceLanguage)))
			{
				cmbUILanguage.Items.Add(language);
			}

			//Store Languages
			List<string> storeLanguages = new List<string>();
			foreach (string l in Enum.GetNames(typeof(StoreLanguage)))
			{
				string name;
				switch (l)
				{
					case "windows":
						name = "Default";
						break;
					case "zh_Hans":
						name = CultureInfo.GetCultureInfo("zh-Hans").NativeName;
						break;
					case "zh_Hant":
						name = CultureInfo.GetCultureInfo("zh-Hant").NativeName;
						break;
					case "pt_BR":
						name = CultureInfo.GetCultureInfo("pt-BR").NativeName;
						break;
					default:
						name = CultureInfo.GetCultureInfo(l).NativeName;
						break;
				}

				storeLanguages.Add(name);
			}

			cmbStoreLanguage.Items.AddRange(storeLanguages.ToArray());

			FillFieldsFromSettings();
		}

		private void SaveFieldsToSettings()
		{
			Settings.SteamPath = txtSteamPath.Text;
			if (radLoad.Checked)
			{
				Settings.StartupAction = StartupAction.LoadProfile;
			}
			else if (radCreate.Checked)
			{
				Settings.StartupAction = StartupAction.CreateProfile;
			}

			switch (cmbDatSrc.SelectedIndex)
			{
				case 0:
					Settings.ListSource = GameListSource.XmlPreferred;
					break;
				case 1:
					Settings.ListSource = GameListSource.XmlOnly;
					break;
				case 2:
					Settings.ListSource = GameListSource.WebsiteOnly;
					break;
			}

			Settings.ProfileToLoad = txtDefaultProfile.Text;

			Settings.UpdateAppInfoOnStart = chkUpdateAppInfoOnStartup.Checked;
			Settings.UpdateHLTBOnStart = chkUpdateHltbOnStartup.Checked;
			Settings.IncludeImputedTimes = chkIncludeImputedTimes.Checked;
			Settings.AutoSaveDatabase = chkAutosaveDB.Checked;
			Settings.ScrapePromptDays = (int) numScrapePromptDays.Value;

			Settings.CheckForUpdates = chkCheckForDepressurizerUpdates.Checked;

			Settings.RemoveExtraEntries = chkRemoveExtraEntries.Checked;

			Settings.InterfaceLanguage = (InterfaceLanguage) cmbUILanguage.SelectedIndex;
			Settings.StoreLanguage = (StoreLanguage) cmbStoreLanguage.SelectedIndex;

			try
			{
				Settings.Save();
			}
			catch (Exception e)
			{
				MessageBox.Show(GlobalStrings.DlgOptions_ErrorSavingSettingsFile + e.Message, GlobalStrings.DBEditDlg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion
	}
}
