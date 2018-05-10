#region License

//     This file (SteamPathDialog.cs) is part of Depressurizer.
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
using System.IO;
using System.Windows.Forms;
using Depressurizer.Properties;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;

#endregion

namespace Depressurizer.Dialogs
{
	public partial class SteamPathDialog : MaterialForm
	{
		#region Constructors and Destructors

		public SteamPathDialog()
		{
			InitializeComponent();
			MaterialSkinManager.AddFormToManage(this);

			SteamPath.Text = GetSteamFolder();
		}

		#endregion

		#region Public Properties

		public string Path => SteamPath.Text;

		#endregion

		#region Properties

		private static MaterialSkinManager MaterialSkinManager => MaterialSkinManager.Instance;

		#endregion

		#region Methods

		private static string GetSteamFolder()
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
				{
					if (key != null)
					{
						object obj = key.GetValue("InstallPath");
						if (obj != null)
						{
							if (obj is string installPath)
							{
								return installPath;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}

			return @"C:\Program Files (x86)\Steam";
		}

		private void ButtonBrowse_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.ShowNewFolderButton = false;
				dialog.SelectedPath = Path;

				DialogResult result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					SteamPath.Text = dialog.SelectedPath;
				}
			}
		}

		private void ButtonOk_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(Path))
			{
				DialogResult result = MessageBox.Show(Resources.SteamPathDialog_PathDoesntExist, Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result == DialogResult.No)
				{
					return;
				}
			}

			if (!File.Exists(System.IO.Path.Combine(Path, "Steam.exe")))
			{
				DialogResult result = MessageBox.Show(Resources.SteamPathDialog_PathDoesntContain, Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result == DialogResult.No)
				{
					return;
				}
			}

			Close();
		}

		#endregion
	}
}
