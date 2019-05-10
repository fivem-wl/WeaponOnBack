--[[

This file is part of FuturePlanFreeRoam.

FuturePlanFreeRoam is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FuturePlanFreeRoam is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with FuturePlanFreeRoam.  If not, see <https://www.gnu.org/licenses/>.

--]]

resource_manifest_version "44febabe-d386-4d18-afbe-5e627f4af937"

files {
    "System.ValueTuple.dll",
    "System.Numerics.dll",
    "Newtonsoft.Json.dll",
    'PumaCore.dll',
    'PumaClient.dll',
}

client_scripts {
    "WeaponOnBackClient.net.dll",
}

server_scripts {
    "WeaponOnBackServer.net.dll"
}
