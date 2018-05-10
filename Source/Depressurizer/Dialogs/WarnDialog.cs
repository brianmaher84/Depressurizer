#region License

//     This file (WarnDialog.cs) is part of Depressurizer.
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
using MaterialSkin;
using MaterialSkin.Controls;

#endregion

namespace Depressurizer.Dialogs
{
	public partial class WarnDialog : MaterialForm
	{
		#region Constructors and Destructors

		public WarnDialog(string title, string message)
		{
			InitializeComponent();
			MaterialSkinManager.AddFormToManage(this);

			Text = title;
			LabelMessage.Text = message;
		}

		#endregion

		#region Public Properties

		public sealed override string Text
		{
			get => base.Text;
			set => base.Text = value;
		}

		#endregion

		#region Properties

		private static MaterialSkinManager MaterialSkinManager => MaterialSkinManager.Instance;

		#endregion

		#region Methods

		private void ButtonOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
