[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$manifest = Join-Path $root 'artifacts/dev-package-layout/AppxManifest.xml'

if (-not $IsWindows) {
    throw 'The Converty development package can only be registered on Windows.'
}
if (-not (Test-Path $manifest)) {
    throw 'Development package layout is missing. Run ./build/stage-dev-package.ps1 first.'
}

Add-AppxPackage -Register $manifest -ForceApplicationShutdown
Write-Host "Registered Converty development package: $manifest"
Write-Host 'Restart File Explorer if its context-menu cache does not refresh automatically.'
