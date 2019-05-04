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
using CitizenFX.Core.Native;


namespace WeaponOnBackClient
{

internal static class PlayerExtensions
{
	/// <summary>
	/// Get all equipped weapon of <seealso cref="Ped"/>
	/// </summary>
	/// <param name="ped"></param>
	/// <returns></returns>
	internal static List<Weapon> GetAllWeapons(this Ped ped)
	{
		List<Weapon> weaponList = new List<Weapon>();
		var weapons = ped.Weapons;
		foreach (var weaponName in WeaponInfo.WeaponNames.Keys)
		{
			if (!API.HasPedGotWeapon(Game.PlayerPed.Handle, (uint) API.GetHashKey(weaponName), false)) continue;
			weaponList.Add(weapons[(WeaponHash)API.GetHashKey(weaponName)]);
		}
		return weaponList;
	}
}

}