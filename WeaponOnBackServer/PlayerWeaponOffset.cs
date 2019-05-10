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
using WeaponOnBackShared;

namespace WeaponOnBackServer
{

class PlayerWeaponOffset
{
	readonly Dictionary<string, Dictionary<uint, (Vector3, Vector3)>> _playerWeaponOffset = new Dictionary<string, Dictionary<uint, (Vector3, Vector3)>>();

	Dictionary<uint, (Vector3, Vector3)> _Get(string playerLicense)
	{
		_playerWeaponOffset.TryGetValue(playerLicense, out var playerWeaponOffset);
		if (!(playerWeaponOffset is null)) return playerWeaponOffset;
		
		playerWeaponOffset = new Dictionary<uint, (Vector3, Vector3)>();
		_playerWeaponOffset[playerLicense] = playerWeaponOffset;
		return playerWeaponOffset;
	}
	public void Set(string playerLicense, uint weaponHash, Vector3 position, Vector3 rotation)
	{
		var playerWeaponOffset = _Get(playerLicense);
		playerWeaponOffset[weaponHash] = (position, rotation);
	}

	public (Vector3, Vector3) Get(string playerLicense, uint weaponHash)
	{
		var playerWeaponOffset = _Get(playerLicense);
		var weaponOffset = playerWeaponOffset.GetValueOrDefault(weaponHash);
		return weaponOffset;
	}

	public Dictionary<uint, (Vector3, Vector3)> GetAll(string playerLicense)
	{
		var playerWeaponOffset = _Get(playerLicense);
		return playerWeaponOffset;
	}
}

}