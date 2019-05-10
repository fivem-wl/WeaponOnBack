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
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace WeaponOnBackClient.Event
{

static class WeaponObjectForPed
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";
	
	static readonly Dictionary<Ped, List<Entity>> WeaponObjectsForPed = new Dictionary<Ped, List<Entity>>();

	static void AddWeaponObject(Entity weaponObject, Ped ped)
	{
		var succeed = WeaponObjectsForPed.TryGetValue(ped, out var weaponObjects);
		if (succeed) weaponObjects.Add(weaponObject);
		else
		{
			WeaponObjectsForPed[ped] = new List<Entity> {weaponObject};
		}
	}

	static void RemoveWeaponObject(Entity weaponObject, Ped ped)
	{
		var succeed = WeaponObjectsForPed.TryGetValue(ped, out var weaponObjects);
		if (succeed) weaponObjects?.Remove(weaponObject);
		else WeaponObjectsForPed[ped] = new List<Entity>();
	}
	
	static void TryDelete(Weapon weapon, Ped ped)
	{
		var weaponObject = GetWeaponObject(weapon, ped);
		Debug.WriteLine($"try delete weapon object: {weaponObject?.Handle}, {weaponObject?.Model} from {weapon.Hash}, {weapon.GetHashCode()}, {ped.Handle}");
		weaponObject?.Delete();
		RemoveWeaponObject(weaponObject, ped);
	}

	public static void Delete(Weapon weapon, Ped ped)
	{
		TryDelete(weapon, ped);
	}

	public static void DeleteAll(Ped ped)
	{
		var succeed = WeaponObjectsForPed.TryGetValue(ped, out var weaponObjects);
		if (!succeed) return;
		weaponObjects?.ForEach(wo => wo?.Delete());
		weaponObjects?.Clear();
	}
	
	static Entity GetWeaponObject(Weapon weapon, Ped ped)
	{
		var succeed = WeaponObjectsForPed.TryGetValue(ped, out var weaponObjects);
		return succeed ? weaponObjects?.SingleOrDefault(wo => (!(wo is null)) && wo.Model == weapon.Model) : null;
	}

	public static IList<Entity> GetAllWeaponObjects(Ped ped)
	{
		var succeed = WeaponObjectsForPed.TryGetValue(ped, out var weaponObjects);
		return succeed ? weaponObjects?.Where(wo => !(wo is null)).ToList() : null;
	}
	
	static async Task<Entity> TryCreate(Weapon weapon, Ped ped)
	{
		var weaponModelHash = weapon.Model.Hash;
		var weaponModel = new Model(weaponModelHash);
		if (!weaponModel.IsInCdImage) return null;
		var loaded = await weaponModel.Request(1000 * 10);
		if (!loaded) return null;

		var pos = ped.Position;
		var propHandle = API.CreateWeaponObject((uint)weapon.Hash, 1, pos.X, pos.Y, pos.Z, false, 0f, 0);
		var prop = Entity.FromHandle(propHandle);
		if (prop is null) return null;
        
		// Make persistent
		prop.IsPersistent = true;
        
		// Set tint
		var weaponTintIndex = (int)weapon.Tint;
		API.SetWeaponObjectTintIndex(propHandle, weaponTintIndex);
        
		// Add components.
		foreach (var weaponComponent in weapon.Components)
		{
			var weaponComponentHash = weaponComponent.ComponentHash;
			if (!weaponComponent.Active) continue;
			Debug.WriteLine($"[{ResourceName}]weaponComponentHash - {weaponComponentHash} ,{(uint)weaponComponentHash}");
			API.GiveWeaponComponentToWeaponObject(propHandle, (uint)weaponComponentHash);
            
		}

		return prop;
	}
	
	public static async Task<Entity> Create(Weapon weapon, Ped ped)
	{
		TryDelete(weapon, ped);
		var weaponObject = await TryCreate(weapon, ped);
		if (!(weaponObject is null))
		{
			AddWeaponObject(weaponObject, ped);
		}
		return weaponObject;
	}

	/// <summary>
	/// Clear unmanaged weapon objects.
	/// </summary>
	public static void CleanUp()
	{
		var peds = WeaponObjectsForPed.Keys;
		foreach(var ped in peds)
		{
			if (ped.Exists() && ped.IsPlayer) continue;
			
			var weaponObjects = WeaponObjectsForPed[ped];
			foreach (var weaponObject in weaponObjects)
			{
				weaponObject?.Delete();
			}
			weaponObjects.Clear();
		}
	}
}

}