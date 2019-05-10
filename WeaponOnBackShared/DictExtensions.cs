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

namespace WeaponOnBackShared
{

public static class DictExtensions
{
	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
	{
		if (dictionary == null)
			throw new ArgumentNullException(nameof(dictionary));

		if (key == null)
			throw new ArgumentNullException(nameof(key));

		return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
	}
	
	public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
	{
		if (dict.TryGetValue(key, out var val)) return val;
		val = new TValue();
		dict.Add(key, val);
		return val;
	}
}

}