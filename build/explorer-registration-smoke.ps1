[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$manifest = Join-Path $layout 'AppxManifest.xml'
$nativeRoot = Join-Path $root 'artifacts/native-smoke'
$packageName = 'Converty.Dev'

if (-not $IsWindows) {
    throw 'The Explorer registration smoke is Windows-only.'
}
if (-not (Test-Path $manifest)) {
    throw 'Development package layout is missing. Stage it before registration smoke.'
}

$smoke = Get-ChildItem -Path $nativeRoot -Recurse -Filter 'Converty.ExplorerRegistrationSmoke.exe' -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $smoke) {
    throw 'Explorer registration smoke executable is missing from the native build.'
}

$preexisting = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
if ($preexisting.Count -gt 0) {
    throw "A $packageName development package is already registered. Remove it before running this smoke to avoid testing stale registration."
}

try {
    Add-AppxPackage -Register $manifest -ForceApplicationShutdown

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction Stop)
    if ($registered.Count -ne 1) {
        throw "Expected exactly one registered $packageName package; found $($registered.Count)."
    }

    & $smoke.FullName
    if ($LASTEXITCODE -ne 0) {
        throw "Packaged Explorer COM activation smoke failed with exit code $LASTEXITCODE."
    }

    Write-Host 'Explorer package registration and COM activation smoke: PASS'
    Write-Host "Registered layout: $layout"
}
finally {
    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
    foreach ($package in $registered) {
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction SilentlyContinue
    }
}
