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
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using PumaFramework.Client.Event.Game;
using PumaFramework.Core.Container;
using PumaFramework.Core.Event;
using PumaFramework.Shared;

using WeaponOnBackShared;

namespace WeaponOnBackClient
{

class MainController : PumaScript
{
	const string GameModeName = "FuturePlanFreeRoam";
	const string ResourceName = "WeaponOnBack";

	const float MaxUpdateRadius = 500f;

	readonly Dictionary<Player, PlayerWeaponOnBack> _playerWeaponOnBack = new Dictionary<Player, PlayerWeaponOnBack>();

	public static readonly Dictionary<Player, GeneralPlayerWeaponOffset> PlayersWeaponOffset = new Dictionary<Player, GeneralPlayerWeaponOffset>();

	readonly ThisPlayerWeaponOffset _thisPlayerWeaponOffset = new ThisPlayerWeaponOffset();

	static string PlayerWeaponOffsetSetEventName => $"{GameModeName}_{ResourceName}_PlayerWeaponOffsetSetEvent";
	Action<int, uint, Vector3, Vector3> PlayerWeaponOffsetSetEventAction => OnPlayerWeaponOffsetSet;

	public MainController()
	{
		FxEventHandlerUtils.RegisterEventHandlers(EventHandlers, new ClientEventDispatcher(EventManager));

		EventHandlers.Add( PlayerWeaponOffsetSetEventName, PlayerWeaponOffsetSetEventAction);

		Tick += PlayerSessionStartedCheckAsync;
		Tick += UpdateWeaponOnBackAsync;
		// Load WeaponAsset
		Tick += async () =>
		{
			await Delay(1000 * 60 * 2);
			//var weaponHashes = Enum.GetValues(typeof(WeaponHash)).Cast<uint>();
			var weaponHashes = WeaponInfo.WeaponNames.Keys.Select(k => (uint)API.GetHashKey(k)).ToList();
			weaponHashes.ForEach(async weaponHash =>
			{
				if (API.HasWeaponAssetLoaded(weaponHash)) return;
				API.RequestWeaponAsset(weaponHash, 31, 0);
				while (API.HasWeaponAssetLoaded(weaponHash))
				{
					await Delay(1000);
				}
				// Debug.WriteLine($"[{ResourceName}]Load weapon model {(WeaponHash)weaponHash} {weaponHash}");
			});
		
			await Delay(1000 * 60 * 3);
		};
		
		API.RegisterCommand("wob", new Action<int, List<object>, string>((source, args, raw) =>
        {
	        try
	        {
		        var commandArg1 = args[0].ToString().ToLower();

		        var playerWeaponOffset = PlayersWeaponOffset[Game.Player];
		        var playerWeaponOnBack = _playerWeaponOnBack[Game.Player];
		        var weapon = playerWeaponOnBack.LatestWeaponOnBack;
		        var weaponObject = playerWeaponOnBack.LatestWeaponObjectOnBack;
		        var weaponHash = weapon.Hash;

		        switch (commandArg1)
		        {
			        case "pos":
			        case "position":
				        var position = new Vector3(
					        float.Parse(args[1].ToString()) / 100,
					        float.Parse(args[2].ToString()) / 100,
					        float.Parse(args[3].ToString()) / 100);
				        _thisPlayerWeaponOffset.Set(weaponHash, WeaponOffset.OffsetType.Position, position);
				        playerWeaponOffset.Set(weaponHash, WeaponOffset.OffsetType.Position, position);
				        playerWeaponOnBack.AttachWeapon(weapon, weaponObject);
				        TriggerServerEvent(
					        PlayerWeaponOffsetSetEventName, 
					        (uint) weaponHash, 
					        _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Position),
					        _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Rotation));
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}_{ResourceName}_CommandSucceed: {raw}");
				        TriggerEvent($"{GameModeName}_{ResourceName}_CommandSucceed", raw);
				        break;
			        case "rot":
			        case "rotation":
				        var rotation = new Vector3(
					        float.Parse(args[1].ToString()),
					        float.Parse(args[2].ToString()),
					        float.Parse(args[3].ToString()));
				        _thisPlayerWeaponOffset.Set(weaponHash, WeaponOffset.OffsetType.Rotation, rotation);
				        playerWeaponOffset.Set(weaponHash, WeaponOffset.OffsetType.Rotation, rotation);
				        playerWeaponOnBack.AttachWeapon(weapon, weaponObject);
				        TriggerServerEvent(
					        PlayerWeaponOffsetSetEventName, 
					        (uint) weaponHash, 
					        _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Position),
					        _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Rotation));
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}_{ResourceName}_CommandSucceed: {raw}");
				        TriggerEvent($"{GameModeName}_{ResourceName}_CommandSucceed", raw);
				        break;
			        default:
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}_{ResourceName}_CommandFailed: {raw}");
				        TriggerEvent($"{GameModeName}_{ResourceName}_CommandFailed", raw);
				        break;
		        }
	        }
	        catch (Exception e)
	        {
		        Debug.WriteLine($"[{ResourceName}][ERROR]{e.Message}");
		        // Debug.WriteLine($"[{ResourceName}]Usage: /wob pos [posX] [posY] [posZ]");
		        // Debug.WriteLine($"[{ResourceName}]Usage: /wob rot [rotX] [rotY] [rotZ]");
		        Debug.WriteLine($"[{ResourceName}]{GameModeName}:{ResourceName}:CommandFailed: {raw}");
		        TriggerEvent($"{GameModeName}:{ResourceName}:CommandFailed", raw);
	        }
	        
        }), false);
		
	}

	async Task UpdateWeaponOnBackAsync()
	{
		await Delay(100);

		var thisPlayer = Game.PlayerPed;
		var thisPlayerPosition = thisPlayer.Position;

		Players
			.Where(p => p.IsPlaying)
			.Where(p => p.Character.Position.DistanceToSquared(thisPlayerPosition) <= Math.Pow(MaxUpdateRadius, 2))
			.ForEach(async player =>
			{
				_playerWeaponOnBack.TryGetValue(player, out var weaponOnBack);
				if (weaponOnBack is null)
				{
					Debug.WriteLine($"recreate, {player.Handle}");
					weaponOnBack = new PlayerWeaponOnBack(player);
					_playerWeaponOnBack[player] = weaponOnBack;
				}
				await weaponOnBack.UpdateWeaponOnBack();
			});
	}

	async Task PlayerSessionStartedCheckAsync()
	{
		await Delay(100);
		if (API.NetworkIsSessionStarted() && !API.GetIsLoadingScreenActive())
		{
			OnThisPlayerJoined();
			TriggerServerEvent($"{GameModeName}_{ResourceName}_PlayerJoinedEvent");
			Tick -= PlayerSessionStartedCheckAsync;
		}
	}

	void OnThisPlayerJoined()
	{
		WeaponInfo.GetWeaponHashes().ForEach(weaponHash =>
		{
			var position = _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Position);
			var rotation = _thisPlayerWeaponOffset.Get(weaponHash, WeaponOffset.OffsetType.Rotation);
			
			TriggerServerEvent(PlayerWeaponOffsetSetEventName, (uint) weaponHash, position, rotation);
		});
	}
	
	void OnPlayerWeaponOffsetSet(int playerServerId, uint weaponHash, Vector3 position, Vector3 rotation)
	{
		Debug.WriteLine($"get called {playerServerId}, {(WeaponHash) weaponHash}, {position}, {rotation}");
		var player = new Player(API.GetPlayerFromServerId(playerServerId));

		PlayersWeaponOffset.TryGetValue(player, out var playerWeaponOffset);
		if (playerWeaponOffset is null)
		{
			playerWeaponOffset = new GeneralPlayerWeaponOffset(player);
			PlayersWeaponOffset[player] = playerWeaponOffset;
		}
		
		playerWeaponOffset.Set((WeaponHash) weaponHash, WeaponOffset.OffsetType.Position, position);
		playerWeaponOffset.Set((WeaponHash) weaponHash, WeaponOffset.OffsetType.Rotation, rotation);
	}

	protected override void OnStart()
	{
		Debug.WriteLine($"[{ResourceName}]Start.");
	}

	protected override void OnStop()
	{
		Debug.WriteLine($"[{ResourceName}]Stop.");
		_playerWeaponOnBack.Values.ForEach(x => x.CleanUp());
	}
}

}