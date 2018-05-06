#region License

//     This file (AutoCatType.cs) is part of Depressurizer.
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

using System.ComponentModel;

#endregion

namespace Depressurizer.Core.Enums
{
	public enum AutoCatType
	{
		[Description("None")] None,
		[Description("AutoCatGenre")] Genre,
		[Description("AutoCatFlags")] Flags,
		[Description("AutoCatTags")] Tags,
		[Description("AutoCatYear")] Year,
		[Description("AutoCatUserScore")] UserScore,
		[Description("AutoCatHltb")] Hltb,
		[Description("AutoCatManual")] Manual,
		[Description("AutoCatDevPub")] DevPub,
		[Description("AutoCatGroup")] Group,
		[Description("AutoCatName")] Name,
		[Description("AutoCatVrSupport")] VrSupport,
		[Description("AutoCatLanguage")] Language,
		[Description("AutoCatCurator")] Curator,
		[Description("AutoCatPlatform")] Platform
	}
}
