### WeaponOnBack
#### Builds
|Latest Build|
|:-:|
|[![Build status](https://ci.appveyor.com/api/projects/status/iafga7jyodadnmcg?svg=true)](https://ci.appveyor.com/project/imckl/weapononback)|
#### Description
1. Put/Sheath weapons on back, with custom components.
2. Weapon is diplayed via switching weapons.
3. [Demo here](https://youtu.be/ArKqJMv8ZIE).
4. Client-specified attachment offset (can adjust offset per weapon).
#### Commands/Features
1. Client-specified attachment offset (can adjust offset per weapon).
    1. /wob pos [posX] [posY] [posZ] - set attachment position offset, which is lastest weapon switching "on back".
    2. /wob rot [rotX] [rotY] [rotZ] - set attachment rotation offest, which is lastest weapon switching "on back".
2. Since UI might be server preference, there's no UI or command hint for player, and you can add them by yourself:
    1. Event "FuturePlanFreeRoam:WeaponOnBack:CommandSucceed" with input will be triggered when command succeed. eg.: FuturePlanFreeRoam:WeaponOnBack:CommandSucceed: wob pos 0 0 0
    2. Event "FuturePlanFreeRoam:WeaponOnBack:CommandFailed" with input will be triggered when command failed, eg.: FuturePlanFreeRoam:WeaponOnBack:CommandFailed: wob pos i m wrong command
#### Known issues
1. It seems too much components/tints on weapon objects would cause unknown crash(eg.: crash when switch to first-person), I’ve limited up to 3 weapon objects to display, and tested only 1-2 players online and works fine. If players encounter more frequent crash related to this script, please provide detail info.
2. ~Currently “attached” position is not fit well on every models (working on it)~ Use /wob pos|rot to adjust
3. Weapon object not sync well through client (working on it)
4. Weapon object only synced in OneSync server, I'm working on it, but might not be fixed :(
5. If you find more issues, feel free to post it.
