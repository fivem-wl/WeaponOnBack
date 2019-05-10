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

using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using PumaFramework.Core.Container;

namespace WeaponOnBackClient
{

class PlayerWeaponOnBack
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";
	
	readonly Player _player;

	readonly WeaponDisplay _weaponDisplay;
	Weapon _previousWeaponInHand;
	public Weapon LatestWeaponOnBack;
	public Entity LatestWeaponObjectOnBack;

	public PlayerWeaponOnBack(Player player)
	{
		_player = player;
		_weaponDisplay = new WeaponDisplay();
		_previousWeaponInHand = player.Character.Weapons[WeaponHash.Unarmed];
	}

	public async Task UpdateWeaponOnBack()
	{
		var currentWeaponInHand = _player.Character.Weapons.Current;

		// is weapon in hand changed?
		var isWeaponInHandChanged = _previousWeaponInHand != currentWeaponInHand;
		if (!isWeaponInHandChanged) return;

		var previousWeaponDisplayGroup = WeaponDisplay.GetWeaponDisplayGroup(_previousWeaponInHand);
		var currentWeaponDisplayGroup = WeaponDisplay.GetWeaponDisplayGroup(currentWeaponInHand);

		// update 
		_weaponDisplay.SetWeapon(currentWeaponInHand);
		// prevent duplicate weapon objects
		_weaponDisplay.DeleteWeaponObject(currentWeaponDisplayGroup);
		_weaponDisplay.DeleteWeaponObject(previousWeaponDisplayGroup);

		var weaponNeedToCreate = _previousWeaponInHand;

		// update previous weapon in hand
		_previousWeaponInHand = currentWeaponInHand;
		
//		Debug.WriteLine($"{weaponNeedToCreate.Hash} {weaponNeedToCreate.Model.Hash}");
//		Debug.WriteLine($"{weaponNeedToCreate} {weaponNeedToCreate.Model} {weaponNeedToCreate.Model.IsValid} {weaponNeedToCreate.Model.IsInCdImage}");
//		Debug.WriteLine($"{!Blacklist.WeaponGroup.Contains(weaponNeedToCreate.Group)} {!Blacklist.WeaponHash.Contains(weaponNeedToCreate.Hash)}");

		var isWeaponNeedToDisplay =
			weaponNeedToCreate.Model.IsValid && weaponNeedToCreate.Model.IsInCdImage &&
			!Blacklist.WeaponGroup.Contains(weaponNeedToCreate.Group) &&
			!Blacklist.WeaponHash.Contains(weaponNeedToCreate.Hash);
		if (!isWeaponNeedToDisplay) return;

		var weaponObject = await CreateWeaponObject(weaponNeedToCreate);
		if (weaponObject is null)
		{
			Debug.WriteLine($"[{ResourceName}]Create object {weaponNeedToCreate.Model.Hash} failed for ped: {_player.Handle}");
			return;
		}

		AttachWeapon(weaponNeedToCreate, weaponObject);

		_weaponDisplay.SetWeapon(WeaponDisplay.GetWeaponDisplayGroup(weaponNeedToCreate), weaponNeedToCreate);
		_weaponDisplay.SetWeaponObject(WeaponDisplay.GetWeaponDisplayGroup(weaponNeedToCreate), weaponObject);

		LatestWeaponObjectOnBack = weaponObject;
		LatestWeaponOnBack = weaponNeedToCreate;
	}

	async Task<Entity> CreateWeaponObject(Weapon weapon)
	{
		var weaponModelHash = weapon.Model.Hash;
		var weaponModel = new Model(weaponModelHash);
		if (!weaponModel.IsInCdImage) return null;
		var loaded = await weaponModel.Request(1000 * 5);
		if (!loaded) return null;

		var pos = _player.Character.Position;
		var weaponObjectHandle = API.CreateWeaponObject((uint) weapon.Hash, 1, pos.X, pos.Y, pos.Z, false, 0f, 0);
		var weaponObject = Entity.FromHandle(weaponObjectHandle);
		if (weaponObject is null) return null;
        
		// Set tint
		var weaponTintIndex = (int)weapon.Tint;
		API.SetWeaponObjectTintIndex(weaponObjectHandle, weaponTintIndex);
        
		// Add components.
		foreach (var weaponComponent in weapon.Components)
		{
			var weaponComponentHash = weaponComponent.ComponentHash;
			if (!weaponComponent.Active) continue;
			// Debug.WriteLine($"[{ResourceName}]weaponComponentHash - {weaponComponentHash} ,{(uint)weaponComponentHash}");
			API.GiveWeaponComponentToWeaponObject(weaponObjectHandle, (uint)weaponComponentHash);
		}

		return weaponObject;
	}

	public void AttachWeapon(Weapon weapon, Entity weaponObject)
	{
		var weaponGroup = weapon.Group;
		var info = WeaponAttachDetail.Infos.Single(wad => wad.WeaponGroup == weaponGroup);

		var positionOffset = MainController.PlayersWeaponOffset[_player].Get(weapon.Hash, WeaponOffset.OffsetType.Position);
		var rotationOffset = MainController.PlayersWeaponOffset[_player].Get(weapon.Hash, WeaponOffset.OffsetType.Rotation);
		var position = new Vector3(
			info.Position.X + positionOffset.X,
			info.Position.Y + positionOffset.Y,
			info.Position.Z + positionOffset.Z);
		var rotation = new Vector3(
			info.Rotation.X + rotationOffset.X,
			info.Rotation.Y + rotationOffset.Y,
			info.Rotation.Z + rotationOffset.Z);
		weaponObject.AttachTo(_player.Character.Bones[info.Bone], position, rotation);

		Debug.WriteLine($"[{ResourceName}]Attach {weapon.Hash}({weapon.Group}) to {info.Bone}, {position}, {rotation} to ped: {_player}");
	}

	public void CleanUp()
	{
		_weaponDisplay.DeleteAllWeaponObjects();
	}
}

}