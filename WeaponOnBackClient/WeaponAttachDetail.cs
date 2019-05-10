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

using System.Collections.Generic;
using CitizenFX.Core;

namespace WeaponOnBackClient
{

readonly struct WeaponAttachDetailInfo
{
	public readonly WeaponGroup WeaponGroup;
	public readonly Bone Bone;
	public readonly Vector3 Position;
	public readonly Vector3 Rotation;

	public WeaponAttachDetailInfo(WeaponGroup weaponGroup, Bone bone, Vector3 position, Vector3 rotation)
	{
		WeaponGroup = weaponGroup;
		Bone = bone;
		Position = position;
		Rotation = rotation;
	}
}

static class WeaponAttachDetail
{
	public static readonly List<WeaponAttachDetailInfo> Infos = new List<WeaponAttachDetailInfo>
	{
		// wob-pos 163842 23639 -0.1 0.05 -0.12 90 90 0
		new WeaponAttachDetailInfo(WeaponGroup.Melee, Bone.RB_L_ThighRoll, new Vector3(-0.1f, 0.05f, -0.12f), new Vector3(90f, 90f, 0f)),
		new WeaponAttachDetailInfo(WeaponGroup.Thrown, Bone.RB_L_ThighRoll, new Vector3(0f, 0.05f, -0.12f), new Vector3(0f, 270f, 0f)),
		// wob-pos 95490 11816 0 0 0.21 270 0 0
		new WeaponAttachDetailInfo(WeaponGroup.Pistol, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
		new WeaponAttachDetailInfo(WeaponGroup.SMG, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
		new WeaponAttachDetailInfo(WeaponGroup.Stungun, Bone.SKEL_Pelvis, new Vector3(0f, 0f, 0.21f), new Vector3(270f, 0f, 0f)),
		new WeaponAttachDetailInfo(WeaponGroup.Heavy, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
		new WeaponAttachDetailInfo(WeaponGroup.Sniper, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
		// /wob-pos 250114 24817 0.2 -0.12 -0.08 0 -20 180
		new WeaponAttachDetailInfo(WeaponGroup.AssaultRifle, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
		new WeaponAttachDetailInfo(WeaponGroup.MG, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
		new WeaponAttachDetailInfo(WeaponGroup.Shotgun, Bone.SKEL_Spine2, new Vector3(0.2f, -0.12f, -0.08f), new Vector3(0f, -20f, 180f)),
	};
}

}