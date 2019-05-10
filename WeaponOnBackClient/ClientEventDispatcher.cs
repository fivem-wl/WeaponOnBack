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
using CitizenFX.Core;
using CitizenFX.Core.Native;
using PumaFramework.Core.Event;
using WeaponOnBackClient.Event;

namespace WeaponOnBackClient
{

public class ClientEventDispatcher
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";
	
	readonly EventManager _eventManager;

	public ClientEventDispatcher(EventManager eventManager)
	{
		_eventManager = eventManager;
	}

	[EventHandler(GameModeName + "_" + ResourceName + "_" + "PlayerWeaponAttachEvent")]
	void OnPlayerWeaponAttached(int playerServerId , uint weaponHash, uint boneId, Vector3 position,
		Vector3 rotation)
	{
		var player = new Player(API.GetPlayerFromServerId(playerServerId));
		_eventManager.DispatchEvent(new PlayerWeaponAttachEvent(player, (WeaponHash) weaponHash, (Bone) boneId,
			position, rotation));
	}
}

}