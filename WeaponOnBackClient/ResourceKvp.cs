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

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WeaponOnBackClient
{

internal static class ResourceKvp
{
	
	const int NotFoundHandle = -1;

	public static T Get<T>(string key) where T : class
	{
		var type = typeof(T);
		if (type == typeof(string)) 	return (T) (object) API.GetResourceKvpString(key);
		if (type == typeof(int)) 		return (T) (object) API.GetResourceKvpInt(key);
		if (type == typeof(float)) 		return (T) (object) API.GetResourceKvpFloat(key);
		throw new ArgumentException("Supported types: int, float, string");
	}

	public static void Set<T>(string key, T value) where T : class
	{
		var type = typeof(T);
		if (type == typeof(string)) 		API.SetResourceKvp(key, (string) (object) value);
		else if (type == typeof(int))		API.SetResourceKvpInt(key, (int) (object) value);
		else if (type == typeof(float)) 	API.SetResourceKvpFloat(key, (float) (object) value);
		else throw new ArgumentException("Supported types: int, float, string");
	}

	public static void Delete(string key)
	{
		API.DeleteResourceKvp(key);
	}

	public static Dictionary<string, T> GetAll<T>(string prefix) where T : class
	{
		var dict = new Dictionary<string, T>();
		var handle = API.StartFindKvp(prefix);

		if (handle == NotFoundHandle) return dict;

		while (true)
		{
			var key = API.FindKvp(handle);
			if (string.IsNullOrEmpty(key)) break;

			dict[key] = Get<T>(key);
		}
		
		API.EndFindKvp(handle);
		return dict;
	}
	
}

}