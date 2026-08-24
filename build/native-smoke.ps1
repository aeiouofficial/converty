[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    cmake --preset native-smoke
    if ($LASTEXITCODE -ne 0) { throw 'Native CMake configure failed.' }
    cmake --build --preset native-smoke
    if ($LASTEXITCODE -ne 0) { throw 'Native CMake topology smoke build failed.' }
}
finally {
    Pop-Location
}
