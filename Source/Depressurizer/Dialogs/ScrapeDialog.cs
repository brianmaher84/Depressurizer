#region License

//     This file (ScrapeDialog.cs) is part of Depressurizer.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Depressurizer.Core.Models;
using Depressurizer.Properties;

#endregion

namespace Depressurizer.Dialogs
{
	public sealed class ScrapeDialog : CancelableDialog
	{
		#region Fields

		private readonly List<int> _jobs;

		private readonly List<DatabaseEntry> _results = new List<DatabaseEntry>();

		private DateTime _start;

		private string _timeLeft;

		#endregion

		#region Constructors and Destructors

		public ScrapeDialog(IEnumerable<int> jobs) : base(Resources.ScrapeDialog_Title)
		{
			_jobs = jobs.Distinct().ToList();
			TotalJobs = _jobs.Count;
		}

		#endregion

		#region Methods

		protected override void CancelableDialog_Load(object sender, EventArgs e)
		{
			_start = DateTime.Now;
			base.CancelableDialog_Load(sender, e);
		}

		protected override void Finish()
		{
			if (Canceled)
			{
				return;
			}

			SetText(Resources.ScrapeDialog_ApplyingData);

			lock (SyncRoot)
			{
				foreach (DatabaseEntry entry in _results)
				{
					if (Program.Database.Contains(entry.Id))
					{
						Program.Database.Games[entry.Id].MergeIn(entry);
					}
					else
					{
						Program.Database.Games.Add(entry.Id, entry);
					}
				}
			}
		}

		protected override void Start()
		{
			Parallel.ForEach(_jobs, RunJob);
			Close();
		}

		protected override void UpdateText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format(CultureInfo.CurrentUICulture, Resources.ScrapeDialog_Status, CompletedJobs, TotalJobs));

			string timeLeft = string.Format(CultureInfo.CurrentUICulture, "{0}: ", Resources.ScrapeDialog_TimeLeft) + "{0}";
			if (CompletedJobs > 0)
			{
				if (CompletedJobs > TotalJobs / 4 || CompletedJobs % 5 == 0)
				{
					TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.UtcNow.Subtract(_start).Ticks * (TotalJobs - (CompletedJobs + 1)) / (CompletedJobs + 1));
					if (timeRemaining.TotalSeconds >= 60)
					{
						_timeLeft = string.Format(CultureInfo.InvariantCulture, timeLeft, timeRemaining.Minutes + ":" + (timeRemaining.Seconds < 10 ? "0" + timeRemaining.Seconds : timeRemaining.Seconds.ToString()));
					}
					else
					{
						_timeLeft = string.Format(CultureInfo.InvariantCulture, timeLeft, timeRemaining.Seconds + "s");
					}
				}
			}
			else
			{
				_timeLeft = string.Format(CultureInfo.CurrentUICulture, timeLeft, Resources.ScrapeDialog_Unknown);
			}

			stringBuilder.AppendLine(_timeLeft);
			SetText(stringBuilder.ToString());
		}

		private void RunJob(int appId)
		{
			if (Stopped)
			{
				return;
			}

			DatabaseEntry entry = new DatabaseEntry
			{
				Id = appId
			};

			entry.ScrapeStore();

			lock (SyncRoot)
			{
				if (Stopped)
				{
					return;
				}

				_results.Add(entry);
				OnJobCompletion();
			}
		}

		#endregion
	}
}
