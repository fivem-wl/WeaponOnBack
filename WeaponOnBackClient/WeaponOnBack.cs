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
using System.Collections;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using PumaFramework.Core.Event;
using PumaFramework.Client;
using PumaFramework.Client.Event;
using PumaFramework.Client.Event.Game;
using PumaFramework.Client.Event.Resource;
using PumaFramework.Core.Container;
using PumaFramework.Shared;


namespace WeaponOnBackClient
{

public class WeaponOnBack : PumaScript
{
	public const string GameModeName = "FuturePlanFreeRoam";
    public const string ResourceName = "WeaponOnBack";
    
    readonly struct WeaponAttachDetail
    {
        public readonly WeaponGroup WeaponGroup;
        public readonly Bone Bone;
        public readonly Vector3 Position;
        public readonly Vector3 Rotation;

        public WeaponAttachDetail(WeaponGroup weaponGroup, Bone bone, Vector3 position, Vector3 rotation)
        {
            WeaponGroup = weaponGroup;
            Bone = bone;
            Position = position;
            Rotation = rotation;
        }
    }
    
    class WeaponDisplayGroup : List<WeaponGroup> {}
    
    static Weapon _previousWeaponInHand;
    static Entity _latestWeaponObjectOnBack;
    static Weapon _latestWeaponOnBack;
    
    static readonly List<WeaponGroup> WeaponGroupBlacklist = new List<WeaponGroup>
    {
        WeaponGroup.Unarmed,
        WeaponGroup.Parachute,
        WeaponGroup.PetrolCan,
        WeaponGroup.NightVision,
        WeaponGroup.FireExtinguisher,
        WeaponGroup.DigiScanner,
    };
    
    static readonly List<WeaponHash> WeaponBlacklist = new List<WeaponHash>
    {
        WeaponHash.Unarmed,
    };
    
    static readonly List<WeaponAttachDetail> WeaponsAttachDetails = new List<WeaponAttachDetail>
    {
        // wob-pos 163842 23639 -0.1 0.05 -0.12 90 90 0
        new WeaponAttachDetail(WeaponGroup.Melee, Bone.RB_L_ThighRoll, new Vector3(-0.1f, 0.05f, -0.12f), new Vector3(90f, 90f, 0f)),
        new WeaponAttachDetail(WeaponGroup.Thrown, Bone.RB_L_ThighRoll, new Vector3(0f, 0.05f, -0.12f), new Vector3(0f, 270f, 0f)),
        // wob-pos 95490 11816 0 0 0.21 270 0 0
        new WeaponAttachDetail(WeaponGroup.Pistol, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
        new WeaponAttachDetail(WeaponGroup.SMG, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
        new WeaponAttachDetail(WeaponGroup.Stungun, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
        new WeaponAttachDetail(WeaponGroup.Heavy, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
        new WeaponAttachDetail(WeaponGroup.Sniper, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
        // /wob-pos 250114 24817 0.2 -0.12 -0.08 0 -20 180
        new WeaponAttachDetail(WeaponGroup.AssaultRifle, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
        new WeaponAttachDetail(WeaponGroup.MG, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
        new WeaponAttachDetail(WeaponGroup.Shotgun, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
    };

    public WeaponOnBack()
    {
	    _previousWeaponInHand = Game.PlayerPed.Weapons[WeaponHash.Unarmed];
	    
        Tick += UpdateWeaponsOnBackAsync;
        Tick += LoadWeaponAsset;
        Tick += ReAttachWeapons;
        
        API.RegisterCommand("wob", new Action<int, List<object>, string>((source, args, raw) =>
        {
	        try
	        {
		        var commandArg1 = args[0].ToString().ToLower();
		        var weapon = _latestWeaponOnBack;
		        var weaponObject = _latestWeaponObjectOnBack;

		        switch (commandArg1)
		        {
			        case "pos":
			        case "position":
				        var position = new Vector3(
					        float.Parse(args[1].ToString()) / 100,
					        float.Parse(args[2].ToString()) / 100,
					        float.Parse(args[3].ToString()) / 100);
				        WeaponOffset.Set(weapon.Hash, WeaponOffset.OffsetType.Position, position);
				        AttachWeapon(weapon, weaponObject);
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}:{ResourceName}:CommandSucceed: {raw}");
				        TriggerEvent($"{GameModeName}:{ResourceName}:CommandSucceed", raw);
				        break;
			        case "rot":
			        case "rotation":
				        var rotation = new Vector3(
					        float.Parse(args[1].ToString()),
					        float.Parse(args[2].ToString()),
					        float.Parse(args[3].ToString()));
				        WeaponOffset.Set(weapon.Hash, WeaponOffset.OffsetType.Rotation, rotation);
				        AttachWeapon(weapon, weaponObject);
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}:{ResourceName}:CommandSucceed: {raw}");
				        TriggerEvent($"{GameModeName}:{ResourceName}:CommandSucceed", raw);
				        break;
//			        case "reg":
//				        // Sync
//				        var handle = weaponObject.Handle;
//				        
//				        if (API.NetworkGetEntityIsNetworked(weaponObject.Handle))
//				        {
//					        Debug.WriteLine($"not networked {weaponObject.Handle}");
//				        }
//				        weaponObject.IsPersistent = true;
//				        var netId = API.NetworkGetNetworkIdFromEntity(handle);
//				        API.NetworkRegisterEntityAsNetworked(API.ObjToNet(handle));
//				        API.SetNetworkIdCanMigrate(netId, true);
//				        API.SetNetworkIdExistsOnAllMachines(netId, true);
//				        API.NetworkRequestControlOfEntity(handle);
//				        Debug.WriteLine($"try set prop as networked {weaponObject.Handle}");
//				        break;
			        default:
				        Debug.WriteLine($"[{ResourceName}]{GameModeName}:{ResourceName}:CommandFailed: {raw}");
				        TriggerEvent($"{GameModeName}:{ResourceName}:CommandFailed", raw);
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
        
        #if DEBUG
        
        API.RegisterCommand("wob-current", new Action(() =>
        {
            var currentWeapon = Game.PlayerPed.Weapons.Current;
            var currentWeaponSlot = API.GetWeapontypeSlot((uint)currentWeapon.Hash);
            var currentWeapontypeInSlot = API.GetPedWeapontypeInSlot(Game.PlayerPed.Handle, (uint)currentWeaponSlot);
            Debug.WriteLine($"currentWeapon: {(uint)currentWeapon.Hash}");
            Debug.WriteLine($"Game.PlayerPed.Weapons.Current.Group: {(uint)Game.PlayerPed.Weapons.Current.Group}");
            Debug.WriteLine($"currentWeaponSlot: {(uint)currentWeaponSlot}");
            Debug.WriteLine($"currentWeapontypeInSlot: {(uint)currentWeapontypeInSlot}");
        }), false);
        
        API.RegisterCommand("wob-pos", new Action<int, List<object>, string>((source, args, raw) =>
        {
            var weaponObject = Entity.FromHandle(int.Parse(args[0].ToString()));
            
            if (weaponObject is null || !weaponObject.Exists()) return;

            var pedBone = Game.PlayerPed.Bones[(Bone) int.Parse(args[1].ToString())];
            var attachPos = new Vector3(float.Parse(args[2].ToString()), float.Parse(args[3].ToString()), float.Parse(args[4].ToString()));
            var attachRot = new Vector3(float.Parse(args[5].ToString()), float.Parse(args[6].ToString()), float.Parse(args[7].ToString()));
            weaponObject.AttachTo(pedBone, attachPos, attachRot);
            // CurrentWeaponsOnBack.Values.ForEach(weaponObject => { });
        }), false);
        
        API.RegisterCommand("wob-view", new Action(() =>
        {
            Debug.WriteLine("_weaponAttachedObject");
            WeaponDisplay.GetAllDisplayedWeaponObjects().ForEach(kvp =>
            {
                var weaponObject = kvp.Value;
                Debug.WriteLine($"weapon: {weaponObject?.Model}({weaponObject?.Model.Hash}), modelHandle: {weaponObject?.Handle}");
            });
        }), false);
        
        #endif
    }

    static class WeaponDisplay
    {
        
        static readonly Dictionary<string, WeaponDisplayGroup> DisplayGroups = new Dictionary<string, WeaponDisplayGroup>
        {
            {"EquipGroupLeft", new WeaponDisplayGroup{WeaponGroup.Melee, WeaponGroup.Thrown}},
            {"EquipGroupRight", new WeaponDisplayGroup{WeaponGroup.Pistol, WeaponGroup.SMG, WeaponGroup.Stungun}},
            {"EquipGroupSpine1", new WeaponDisplayGroup{WeaponGroup.Heavy, WeaponGroup.Sniper, WeaponGroup.AssaultRifle, WeaponGroup.MG, WeaponGroup.Shotgun}},
        };
        static readonly Dictionary<WeaponDisplayGroup, Weapon> DisplayGroupsWeapon = new Dictionary<WeaponDisplayGroup, Weapon>();
        static readonly Dictionary<WeaponDisplayGroup, Entity> DisplayGroupsWeaponObject = new Dictionary<WeaponDisplayGroup, Entity>();
        
        public static WeaponDisplayGroup GetWeaponDisplayGroup(Weapon weapon)
        {
            if (weapon is null) return null;
            var equipGroup = DisplayGroups.SingleOrDefault(weg => weg.Value.Contains(weapon.Group)).Value;
            return equipGroup;
        }

        public static List<WeaponDisplayGroup> GetAllDisplayGroups()
            => DisplayGroups.Values.ToList();

        public static Dictionary<WeaponDisplayGroup, Entity> GetAllDisplayedWeaponObjects()
            => DisplayGroupsWeaponObject;

        public static void SetWeapon(Weapon weapon)
        {
            var equipGroup = GetWeaponDisplayGroup(weapon);
            if (equipGroup is null) return;
            
            DisplayGroupsWeapon[equipGroup] = weapon;
        }

        public static void SetWeapon(WeaponDisplayGroup displayGroup, Weapon weapon)
        {
	        DisplayGroupsWeapon[displayGroup] = weapon;
        }

        public static Weapon GetWeapon(WeaponDisplayGroup displayGroup)
        {
            DisplayGroupsWeapon.TryGetValue(displayGroup, out var weapon);
            return weapon;
        }

        public static void SetWeaponObject(WeaponDisplayGroup weaponDisplayGroup, Entity weaponObject)
        {
            DisplayGroupsWeaponObject[weaponDisplayGroup] = weaponObject;
        }

        public static void DeleteWeaponObject(WeaponDisplayGroup weaponDisplayGroup)
        {
            if (weaponDisplayGroup is null) return;
            DisplayGroupsWeaponObject.TryGetValue(weaponDisplayGroup, out var weaponObject);
            weaponObject?.Delete();
            DisplayGroupsWeaponObject[weaponDisplayGroup] = null;
        }

        public static void DeleteAllWeaponObjects()
        {
	        var allWeaponObjects = GetAllDisplayedWeaponObjects();
	        var keys = allWeaponObjects.Keys.ToList();
	        allWeaponObjects.Values.ForEach(wo => wo?.Delete());
	        keys.ForEach(k => allWeaponObjects[k] = null);
        }
    }

    static async Task ReAttachWeapons()
    {
	    WeaponDisplay.GetAllDisplayedWeaponObjects()
		    .Where(d => !(d.Value is null) && d.Value.Exists())
		    .ToList()
		    .ForEach(d =>
		    {
			    var weaponDisplayGroup = d.Key;
			    var weaponObject = d.Value;
			    var weapon = WeaponDisplay.GetWeapon(weaponDisplayGroup);
			    	    
			    // Try sync
			    API.NetworkRegisterEntityAsNetworked(API.ObjToNet(weaponObject.Handle));
			    weaponObject.IsPersistent = true;
			    var netId = API.NetworkGetNetworkIdFromEntity(weaponObject.Handle);
			    API.SetNetworkIdCanMigrate(netId, true);
			    API.SetNetworkIdExistsOnAllMachines(netId, true);
			    API.NetworkRequestControlOfEntity(weaponObject.Handle);

			    AttachWeapon(weapon, weaponObject);
			    Debug.WriteLine($"[{ResourceName}]ReAttachWeapon: weapon: {weapon.Hash}, model: {weaponObject.Model.Hash}, handle: {weaponObject.Handle}");
		    });

	    await Delay(1000 * 10);
    }
    
    static async Task UpdateWeaponsOnBackAsync()
    {
        await Delay(100);
        
        var currentWeaponInHand = Game.PlayerPed.Weapons.Current;
        
        // is weapon in hand changed?
        var isWeaponInHandChanged = _previousWeaponInHand != currentWeaponInHand;
        if (!isWeaponInHandChanged) return;

        var previousWeaponDisplayGroup = WeaponDisplay.GetWeaponDisplayGroup(_previousWeaponInHand);
        var currentWeaponDisplayGroup = WeaponDisplay.GetWeaponDisplayGroup(currentWeaponInHand);
        
        // update 
        WeaponDisplay.SetWeapon(currentWeaponInHand);
        // prevent duplicate weapon objects
        WeaponDisplay.DeleteWeaponObject(currentWeaponDisplayGroup);
        WeaponDisplay.DeleteWeaponObject(previousWeaponDisplayGroup);
        
        var weaponNeedToCreate = _previousWeaponInHand;
        
        // update previous weapon in hand
        _previousWeaponInHand = currentWeaponInHand;
        
        var isWeaponNeedToDisplay =
            weaponNeedToCreate.Model.IsValid && weaponNeedToCreate.Model.IsInCdImage &&
            !WeaponGroupBlacklist.Contains(weaponNeedToCreate.Group) &&
            !WeaponBlacklist.Contains(weaponNeedToCreate.Hash);
        if (!isWeaponNeedToDisplay) return;
        
        var weaponObject = await CreateWeaponObject(weaponNeedToCreate);
        if (weaponObject is null)
        {
            Debug.WriteLine($"[{ResourceName}]Create object {weaponNeedToCreate.Model.Hash} failed");
            return;
        }

        AttachWeapon(weaponNeedToCreate, weaponObject);
		
        WeaponDisplay.SetWeapon(WeaponDisplay.GetWeaponDisplayGroup(weaponNeedToCreate), weaponNeedToCreate);
        WeaponDisplay.SetWeaponObject(WeaponDisplay.GetWeaponDisplayGroup(weaponNeedToCreate), weaponObject);

        _latestWeaponObjectOnBack = weaponObject;
        _latestWeaponOnBack = weaponNeedToCreate;
    }

    static async Task LoadWeaponAsset()
    {
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
		    Debug.WriteLine($"[{ResourceName}]Load weapon model {(WeaponHash)weaponHash} {weaponHash}");
	    });
		
	    await Delay(1000 * 60 * 5);
    }
    
    static async Task<Entity> CreateWeaponObject(Weapon weapon)
    {
        var weaponModelHash = weapon.Model.Hash;
        var weaponModel = new Model(weaponModelHash);
        if (!weaponModel.IsInCdImage) return null;
        var loaded = await weaponModel.Request(1000 * 10);
        if (!loaded) return null;

        var pos = Game.PlayerPed.Position;
        var propHandle = API.CreateWeaponObject((uint)weapon.Hash, 1, pos.X, pos.Y, pos.Z, true, 0f, 0);
        var prop = Entity.FromHandle(propHandle);
        if (prop is null) return null;
        
        // Try sync
        API.NetworkRegisterEntityAsNetworked(API.ObjToNet(propHandle));
        prop.IsPersistent = true;
        var netId = API.NetworkGetNetworkIdFromEntity(propHandle);
        API.SetNetworkIdCanMigrate(netId, true);
        API.SetNetworkIdExistsOnAllMachines(netId, true);
        API.NetworkRequestControlOfEntity(propHandle);
        
        // Set tint
        var weaponTintIndex = (int)weapon.Tint;
        API.SetWeaponObjectTintIndex(propHandle, weaponTintIndex);
        
        // Add components.
        foreach (WeaponComponent weaponComponent in weapon.Components)
        {
            var weaponComponentHash = weaponComponent.ComponentHash;
            if (!weaponComponent.Active) continue;
            Debug.WriteLine($"[{ResourceName}]weaponComponentHash - {weaponComponentHash} ,{(uint)weaponComponentHash}");
            API.GiveWeaponComponentToWeaponObject(propHandle, (uint)weaponComponentHash);
            
        }

        return prop;
    }

    static void AttachWeapon(Weapon weapon, Entity weaponObject)
    {
	    var weaponGroup = weapon.Group;
	    var info = WeaponsAttachDetails.Single(wad => wad.WeaponGroup == weaponGroup);
	    var positionOffset = WeaponOffset.Get(weapon.Hash, WeaponOffset.OffsetType.Position);
	    var rotationOffset = WeaponOffset.Get(weapon.Hash, WeaponOffset.OffsetType.Rotation);
	    var position = new Vector3(
		    info.Position.X + positionOffset.X,
		    info.Position.Y + positionOffset.Y,
		    info.Position.Z + positionOffset.Z);
	    var rotation = new Vector3(
		    info.Rotation.X + rotationOffset.X,
		    info.Rotation.Y + rotationOffset.Y,
		    info.Rotation.Z + rotationOffset.Z);
	    weaponObject.AttachTo(Game.PlayerPed.Bones[info.Bone], position, rotation);
	    Debug.WriteLine($"[{ResourceName}]Attach {weapon.Hash}({weapon.Group}) to {info.Bone}, {position}, {rotation}");
    }
    
    [PumaEventHandler]
    void OnThisPlayerDead(ThisPlayerDeadEvent @event)
    {
        WeaponDisplay.DeleteAllWeaponObjects();
    }
    
    protected override void OnStart()
    {
        Debug.WriteLine($"[{ResourceName}]Start.");
    }

    protected override void OnStop()
    {
        Debug.WriteLine($"[{ResourceName}]Stop.");
        WeaponDisplay.DeleteAllWeaponObjects();
    }
}


}