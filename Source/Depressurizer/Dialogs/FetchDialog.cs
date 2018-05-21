#region License

//     This file (FetchDialog.cs) is part of Depressurizer.
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

using System.Xml;
using Depressurizer.Properties;

#endregion

namespace Depressurizer.Dialogs
{
	internal class FetchDialog : CancelableDialog
	{
		#region Fields

		private XmlDocument _document;

		#endregion

		#region Constructors and Destructors

		public FetchDialog() : base(Resources.FetchDialog_Title, false)
		{
			SetText(Resources.FetchDialog_Status);
			Added = 0;
		}

		#endregion

		#region Public Properties

		public int Added { get; private set; }

		#endregion

		#region Methods

		protected override void Finish()
		{
			if (Canceled || (Error != null))
			{
				return;
			}

			if (_document == null)
			{
				return;
			}

			SetText(Resources.FetchDialog_Finished);
			Added = Database.Instance.IntegrateAppList(_document);
		}

		protected override void Start()
		{
			_document = Database.FetchAppListFromWeb();
			OnJobCompletion();
			Close();
		}

		#endregion
	}
}
