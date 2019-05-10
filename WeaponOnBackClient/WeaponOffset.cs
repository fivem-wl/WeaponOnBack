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

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using PumaFramework.Shared;

using WeaponOnBackShared;


namespace WeaponOnBackClient
{

abstract class WeaponOffset
{

	Player _player;
	
	protected const string GameModeName = "FuturePlanFreeRoam";
	protected const string ResourceName = "WeaponOnBack";

	public enum OffsetType
	{
		Position = 1,
		Rotation = 2
	}
	const float MaxPositionOffset = 0.20f;
	const float MaxRotationOffset = 360f;

	protected WeaponOffset(Player player)
	{
		_player = player;
	}

	public abstract bool TryGet(WeaponHash weaponHash, OffsetType offsetType, out Vector3 vector3);
	public abstract Vector3 Get(WeaponHash weaponHash, OffsetType offsetType);
	public abstract void Set(WeaponHash weaponHash, OffsetType offsetType, Vector3 vector3);

	protected static void TriggerServerEvent(string eventName, params object[] args) => BaseScript.TriggerServerEvent(eventName, args);
	
	/// <summary>
	/// Edit vector3 value to a specific range, base on offset type.
	/// </summary>
	/// <param name="vector3"></param>
	/// <param name="offsetType"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	protected static Vector3 Polish(Vector3 vector3, OffsetType offsetType)
	{

//		if (vector3.IsZero) return Vector3.Zero;
		
		float GetPosDimOffset(float p) =>
			p >= 0 ? Math.Min(p, MaxPositionOffset) : Math.Max(p, -MaxPositionOffset);
		float GetRotDimOffset(float r) =>
			r >= 0 ? Math.Min(r, MaxRotationOffset) : Math.Max(r, -MaxRotationOffset);
		
		switch (offsetType)
		{
			case OffsetType.Position:
				return new Vector3(GetPosDimOffset(vector3.X), GetPosDimOffset(vector3.Y), GetPosDimOffset(vector3.Z));
			case OffsetType.Rotation:
				return new Vector3(GetRotDimOffset(vector3.X), GetRotDimOffset(vector3.Y), GetRotDimOffset(vector3.Z));
			default:
				throw new ArgumentException();
		}
	}
}

sealed class ThisPlayerWeaponOffset : WeaponOffset
{
	public ThisPlayerWeaponOffset() : base(Game.Player)
	{
		
	}
	public override bool TryGet(WeaponHash weaponHash, OffsetType offsetType, out Vector3 vector3)
	{
		var key = GetKey(weaponHash, offsetType);
		var value = ResourceKvp.Get<string>(key);
		Debug.WriteLine($"[{ResourceName}][WeaponOffset]TryGet - key: {key} - value: {value}");
		if (string.IsNullOrEmpty(value))
		{
			vector3 = default;
			return false;
		}
		vector3 = JsonConvert.DeserializeObject<Vector3>(value);
		return true;
	}

	public override Vector3 Get(WeaponHash weaponHash, OffsetType offsetType)
	{
		var key = GetKey(weaponHash, offsetType);
		var value = ResourceKvp.Get<string>(key);
		return string.IsNullOrEmpty(value) ? Vector3.Zero : JsonConvert.DeserializeObject<Vector3>(value);
	}

	public override void Set(WeaponHash weaponHash, OffsetType offsetType, Vector3 vector3)
	{
		var key = GetKey(weaponHash, offsetType);
		var rawValue = Polish(vector3, offsetType);
		var value = JsonConvert.SerializeObject(Polish(vector3, offsetType));
		ResourceKvp.Set(key, value);
		
		TriggerServerEvent(
			$"{GameModeName}_{ResourceName}_PlayerWeaponOffsetSetEvent", 
			(uint) weaponHash, Get(weaponHash, OffsetType.Position), Get(weaponHash, OffsetType.Rotation));
		Debug.WriteLine($"[{ResourceName}][WeaponOffset]Set - key: {key} - value: {value} - rawValue: {rawValue}");
	}

	string GetKey(WeaponHash weaponHash, OffsetType offsetType)
		=> $"{GameModeName}:{ResourceName}:{weaponHash}:{offsetType.ToString()}";

}

sealed class GeneralPlayerWeaponOffset : WeaponOffset
{
	readonly Dictionary<WeaponHash, Vector3> _weaponsPosition;
	readonly Dictionary<WeaponHash, Vector3> _weaponsRotation;
	
	public GeneralPlayerWeaponOffset(Player player) : base(player)
	{
		_weaponsPosition = new Dictionary<WeaponHash, Vector3>();
		_weaponsRotation = new Dictionary<WeaponHash, Vector3>();
	}

	/// <summary>
	/// Not implemented
	/// </summary>
	/// <param name="weaponHash"></param>
	/// <param name="offsetType"></param>
	/// <param name="vector3"></param>
	/// <returns></returns>
	public override bool TryGet(WeaponHash weaponHash, OffsetType offsetType, out Vector3 vector3)
	{
		vector3 = default;
		return false;
	}

	public override Vector3 Get(WeaponHash weaponHash, OffsetType offsetType)
	{
		switch (offsetType)
		{
			case OffsetType.Position: 
				return _weaponsPosition.GetValueOrDefault(weaponHash);
            case OffsetType.Rotation: 
	            return _weaponsRotation.GetValueOrDefault(weaponHash);
            default: throw new ArgumentException();
		}
	}

	public override void Set(WeaponHash weaponHash, OffsetType offsetType, Vector3 vector3)
	{
		switch (offsetType)
		{
			case OffsetType.Position: 
				_weaponsPosition[weaponHash] = Polish(vector3, OffsetType.Position);
				break;
			case OffsetType.Rotation:
				_weaponsRotation[weaponHash] = Polish(vector3, OffsetType.Rotation);
				break;
			default: throw new ArgumentException();
		}
	}
}

}