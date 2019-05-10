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

using PumaFramework.Core.Event;
using WeaponOnBackServer.Event;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WeaponOnBackServer
{

class ServerEventDispatcher
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";
	
	readonly EventManager _eventManager;

	public ServerEventDispatcher(EventManager eventManager)
	{
		_eventManager = eventManager;
	}

	[EventHandler(GameModeName + "_" + ResourceName + "_" + "PlayerWeaponAttachedEvent")]
	void OnPlayerWeaponAttached([FromSource] Player source, uint weaponHash, uint boneId, Vector3 position,
		Vector3 rotation)
	{
		Debug.WriteLine(GameModeName + "_" + ResourceName + "_" + "PlayerWeaponAttachedEvent");
		_eventManager.DispatchEvent(new PlayerWeaponAttachedEvent(source, weaponHash, boneId, position, rotation));
	}
}

}