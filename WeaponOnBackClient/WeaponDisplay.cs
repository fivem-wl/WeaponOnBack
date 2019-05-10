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
using System.Linq;
using CitizenFX.Core;
using PumaFramework.Core.Container;

namespace WeaponOnBackClient
{

class WeaponDisplay
{
	
	public class WeaponDisplayGroup : List<WeaponGroup> {}
	
	static readonly Dictionary<string, WeaponDisplayGroup> DisplayGroups = new Dictionary<string, WeaponDisplayGroup>
	{
		{"EquipGroupLeft", new WeaponDisplayGroup {WeaponGroup.Melee, WeaponGroup.Thrown, WeaponGroup.Unarmed}},
		{"EquipGroupRight", new WeaponDisplayGroup {WeaponGroup.Pistol, WeaponGroup.SMG, WeaponGroup.Stungun}},
		{"EquipGroupSpine1", new WeaponDisplayGroup {WeaponGroup.Heavy, WeaponGroup.Sniper, WeaponGroup.AssaultRifle, WeaponGroup.MG, WeaponGroup.Shotgun} },
	};

	readonly Dictionary<WeaponDisplayGroup, Weapon> _displayGroupsWeapon = new Dictionary<WeaponDisplayGroup, Weapon>();

	readonly Dictionary<WeaponDisplayGroup, Entity> _displayGroupsWeaponObject = new Dictionary<WeaponDisplayGroup, Entity>();

	public static WeaponDisplayGroup GetWeaponDisplayGroup(Weapon weapon)
	{
		if (weapon is null) return null;
		var equipGroup = DisplayGroups.SingleOrDefault(weg => weg.Value.Contains(weapon.Group)).Value;
		return equipGroup;
	}

	public List<WeaponDisplayGroup> GetAllDisplayGroups()
		=> DisplayGroups.Values.ToList();

	public Dictionary<WeaponDisplayGroup, Entity> GetAllDisplayedWeaponObjects()
		=> _displayGroupsWeaponObject;

	public void SetWeapon(Weapon weapon)
	{
		var equipGroup = GetWeaponDisplayGroup(weapon);
		if (equipGroup is null) return;

		_displayGroupsWeapon[equipGroup] = weapon;
	}

	public void SetWeapon(WeaponDisplayGroup displayGroup, Weapon weapon)
	{
		_displayGroupsWeapon[displayGroup] = weapon;
	}

	public Weapon GetWeapon(WeaponDisplayGroup displayGroup)
	{
		_displayGroupsWeapon.TryGetValue(displayGroup, out var weapon);
		return weapon;
	}

	public void SetWeaponObject(WeaponDisplayGroup weaponDisplayGroup, Entity weaponObject)
	{
		_displayGroupsWeaponObject[weaponDisplayGroup] = weaponObject;
	}

	public void DeleteWeaponObject(WeaponDisplayGroup weaponDisplayGroup)
	{
		if (weaponDisplayGroup is null) return;
		_displayGroupsWeaponObject.TryGetValue(weaponDisplayGroup, out var weaponObject);
		weaponObject?.Delete();
		_displayGroupsWeaponObject[weaponDisplayGroup] = null;
	}

	public void DeleteAllWeaponObjects()
	{
		var allWeaponObjects = GetAllDisplayedWeaponObjects();
		var keys = allWeaponObjects.Keys.ToList();
		allWeaponObjects.Values.ForEach(wo => wo?.Delete());
		keys.ForEach(k => allWeaponObjects[k] = null);
	}
}

}