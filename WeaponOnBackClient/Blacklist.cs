/*
 * This file is part of FuturePlanFreeRoam.
 * 
 * FuturePlanFreeRoam is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * FuturePlanFreeRoam is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with FuturePlanFreeRoam.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using CitizenFX.Core;

namespace WeaponOnBackClient
{

static class Blacklist
{
	public static readonly List<WeaponGroup> WeaponGroup = new List<WeaponGroup>
	{
		CitizenFX.Core.WeaponGroup.Unarmed,
		CitizenFX.Core.WeaponGroup.Parachute,
		CitizenFX.Core.WeaponGroup.PetrolCan,
		CitizenFX.Core.WeaponGroup.NightVision,
		CitizenFX.Core.WeaponGroup.FireExtinguisher,
		CitizenFX.Core.WeaponGroup.DigiScanner,
	};
	
	public static readonly List<WeaponHash> WeaponHash = new List<WeaponHash>
	{
		CitizenFX.Core.WeaponHash.Unarmed,
	};
}



}