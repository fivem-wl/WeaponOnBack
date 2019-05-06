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


namespace WeaponOnBackClient
{

static class WeaponOffset
{
	
	public enum OffsetType
	{
		Position = 1,
		Rotation = 2
	}
	const float MaxPositionOffset = 0.20f;
	const float MaxRotationOffset = 360f;
	static string GameModeName => WeaponOnBack.GameModeName;
	static string ResourceName => WeaponOnBack.ResourceName;

	public static Vector3 Get(WeaponHash weaponHash, OffsetType offsetType)
	{
		var key = GetKey(weaponHash, offsetType);
		var value = ResourceKvp.Get<string>(key);
		Debug.WriteLine($"[{ResourceName}][WeaponOffset]Get - key: {key} - value: {value}");
		return string.IsNullOrEmpty(value) ? Vector3.Zero : JsonConvert.DeserializeObject<Vector3>(value);
	}
	
	public static void Set(WeaponHash weaponHash, OffsetType offsetType, Vector3 vector3)
	{
		var key = GetKey(weaponHash, offsetType);
		var rawValue = Polish(vector3, offsetType);
		var value = JsonConvert.SerializeObject(Polish(vector3, offsetType));
		Debug.WriteLine($"[{ResourceName}][WeaponOffset]Set - key: {key} - value: {value} - rawValue: {rawValue}");
		ResourceKvp.Set(key, value);
	}

	/// <summary>
	/// Edit vector3 value to a specific range, base on offset type.
	/// </summary>
	/// <param name="vector3"></param>
	/// <param name="offsetType"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	static Vector3 Polish(Vector3 vector3, OffsetType offsetType)
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

	static string GetKey(WeaponHash weaponHash, OffsetType offsetType)
		=> $"{GameModeName}:{ResourceName}:{weaponHash}:{offsetType.ToString()}";

}

}