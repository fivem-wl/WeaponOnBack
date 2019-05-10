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
using System.Net.Mime;
using System.Runtime.InteropServices;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using PumaFramework.Core.Container;
using PumaFramework.Core.Event;
using PumaFramework.Server.Event;
using PumaFramework.Shared;

using WeaponOnBackShared;
using WeaponOnBackServer.Event;


namespace WeaponOnBackServer
{

public class MainController : PumaScript
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";

	readonly PlayerWeaponOffset _playerWeaponOffset = new PlayerWeaponOffset();

	string PlayerWeaponOffsetSetEventName => $"{GameModeName}_{ResourceName}_PlayerWeaponOffsetSetEvent";
	Action<Player, uint, Vector3, Vector3> PlayerWeaponOffsetSetEventAction => OnPlayerWeaponOffsetSet;

	string PlayerJoinedEventName => $"{GameModeName}_{ResourceName}_PlayerJoinedEvent";
	Action<Player> PlayerJoinedEventAction => OnPlayerJoined;
	
	public MainController()
	{
		EventHandlers.Add(PlayerJoinedEventName, PlayerJoinedEventAction);
		EventHandlers.Add(PlayerWeaponOffsetSetEventName, PlayerWeaponOffsetSetEventAction);
	}

	void OnPlayerWeaponOffsetSet([FromSource] Player source, uint weaponHash, Vector3 position, Vector3 rotation)
	{
		// Debug.WriteLine($"[{ResourceName}]OnPlayerWeaponOffsetSet: {source.Handle}, {weaponHash}, {position}, {rotation}");

		var playerServerId = source.GetServerId();
		var playerLicense = source.GetLicense();
		
		_playerWeaponOffset.Set(playerLicense, weaponHash, position, rotation);
		TriggerClientEvent(PlayerWeaponOffsetSetEventName, playerServerId, weaponHash, position, rotation);
	}

	void OnPlayerJoined([FromSource] Player source)
	{
		Debug.WriteLine($"[{ResourceName}]OnPlayerJoined: {source.Handle}");
		
		Players.ForEach(player =>
		{
			var playerLicense = player.GetLicense();
			var playerServerId = player.GetServerId();
			var playerWeaponOffset = _playerWeaponOffset.GetAll(playerLicense);
			playerWeaponOffset.ForEach(kvp =>
			{
				var weaponHash = kvp.Key;
				var (position, rotation) = kvp.Value;
				TriggerClientEvent(source, PlayerWeaponOffsetSetEventName, playerServerId, weaponHash, position, rotation);
			});
		});
	}

	
	protected override void OnStart()
	{
		EventHandlers.ForEach(kvp => Debug.WriteLine($"{kvp.Key}, {kvp.Value}"));
		Debug.WriteLine($"[{ResourceName}]Start.");
	}

	protected override void OnStop()
	{
		Debug.WriteLine($"[{ResourceName}]Stop.");
	}
}

}