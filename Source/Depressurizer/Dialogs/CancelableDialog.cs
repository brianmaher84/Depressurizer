#region License

//     This file (CancelableDialog.cs) is part of Depressurizer.
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
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

#endregion

namespace Depressurizer.Dialogs
{
	public partial class CancelableDialog : MaterialForm
	{
		#region Static Fields

		protected static object SyncRoot = new object();

		#endregion

		#region Fields

		private bool _canceled = false;

		private bool _stopped = false;

		#endregion

		#region Constructors and Destructors

		public CancelableDialog(string title, bool showStopButton = true)
		{
			InitializeComponent();

			MaterialSkinManager.AddFormToManage(this);
			//MaterialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
			//MaterialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

			Text = title;
			ButtonStop.Visible = ButtonStop.Enabled = showStopButton;
		}

		#endregion

		#region Delegates

		private delegate void SetTextDelegate(string text);

		private delegate void SimpleDelegate();

		#endregion

		#region Public Properties

		public int CompletedJobs { get; protected set; }

		public Exception Error { get; protected set; }

		public sealed override string Text
		{
			get => base.Text;
			set => base.Text = value;
		}

		public int TotalJobs { get; protected set; }

		#endregion

		#region Properties

		protected bool Canceled
		{
			get
			{
				lock (SyncRoot)
				{
					return _canceled;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_canceled = value;
				}
			}
		}

		protected bool Stopped
		{
			get
			{
				lock (SyncRoot)
				{
					return _stopped;
				}
			}
			set
			{
				lock (SyncRoot)
				{
					_stopped = value;
				}
			}
		}

		private static MaterialSkinManager MaterialSkinManager => MaterialSkinManager.Instance;

		#endregion

		#region Methods

		protected virtual void CancelableDialog_Load(object sender, EventArgs e)
		{
			try
			{
				Task.Run(() => Start());
			}
			catch (Exception exception)
			{
				Error = exception;
			}
		}

		protected new void Close()
		{
			if (InvokeRequired)
			{
				Invoke(new SimpleDelegate(Close));
			}
			else
			{
				base.Close();
			}
		}

		protected virtual void Finish() { }

		protected void OnJobCompletion()
		{
			lock (SyncRoot)
			{
				CompletedJobs++;
			}

			UpdateText();
		}

		protected void SetText(string text)
		{
			if (InvokeRequired)
			{
				Invoke(new SetTextDelegate(SetText), text);
			}
			else
			{
				LabelStatus.Text = text;
			}
		}

		protected virtual void Start() { }

		protected virtual void UpdateText() { }

		private void ButtonCancel_Click(object sender, EventArgs e)
		{
			Stopped = true;
			Canceled = true;

			DisableButtons();

			Close();
		}

		private void ButtonStop_Click(object sender, EventArgs e)
		{
			Stopped = true;

			DisableButtons();

			Close();
		}

		private void CancelableDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			Stopped = true;

			DisableButtons();
		}

		private void DisableButtons()
		{
			if (InvokeRequired)
			{
				Invoke(new SimpleDelegate(DisableButtons));
				return;
			}

			ButtonStop.Enabled = ButtonCancel.Enabled = false;
		}

		#endregion
	}
}
