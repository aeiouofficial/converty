[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$FfmpegPath,
    [string]$FfprobePath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridgeOutput = Join-Path $root "src/Converty.Bridge/bin/$Configuration/net10.0"
$hostOutput = Join-Path $root "src/Converty.Host/bin/$Configuration/net10.0"
$workerOutput = Join-Path $root "src/Converty.EngineWorker/bin/$Configuration/net10.0"
$probeWorkerOutput = Join-Path $root "src/Converty.ProbeWorker/bin/$Configuration/net10.0"
$manifest = Join-Path $root 'packaging/Converty.Package/AppxManifest.xml'
$assets = Join-Path $root 'packaging/Converty.Package/Assets'
$nativeRoot = Join-Path $root 'artifacts/native-smoke'

if (-not $IsWindows) {
    throw 'The Converty development package can only be staged on Windows.'
}
if (-not (Test-Path $bridgeOutput)) {
    throw "Bridge output is missing. Build Converty first: $bridgeOutput"
}
if (-not (Test-Path (Join-Path $bridgeOutput 'Converty.Bridge.exe'))) {
    throw 'Converty.Bridge.exe is missing from the managed build output.'
}
if (-not (Test-Path $hostOutput)) {
    throw "Host output is missing. Build Converty first: $hostOutput"
}
if (-not (Test-Path (Join-Path $hostOutput 'Converty.Host.exe'))) {
    throw 'Converty.Host.exe is missing from the managed build output.'
}
if (-not (Test-Path $workerOutput)) {
    throw "Engine worker output is missing. Build Converty first: $workerOutput"
}
if (-not (Test-Path (Join-Path $workerOutput 'Converty.EngineWorker.exe'))) {
    throw 'Converty.EngineWorker.exe is missing from the managed build output.'
}
if (-not (Test-Path (Join-Path $workerOutput 'Converty.Provider.FFmpeg.dll'))) {
    throw 'Converty.Provider.FFmpeg.dll is missing from the engine worker output.'
}
if (-not (Test-Path $probeWorkerOutput)) {
    throw "Probe worker output is missing. Build Converty first: $probeWorkerOutput"
}
if (-not (Test-Path (Join-Path $probeWorkerOutput 'Converty.ProbeWorker.exe'))) {
    throw 'Converty.ProbeWorker.exe is missing from the managed build output.'
}
if (-not (Test-Path (Join-Path $probeWorkerOutput 'Converty.Provider.FFmpeg.dll'))) {
    throw 'Converty.Provider.FFmpeg.dll is missing from the probe worker output.'
}
if (-not (Test-Path $manifest)) {
    throw "Development package manifest is missing: $manifest"
}
if (-not (Test-Path $assets)) {
    throw "Development package assets are missing: $assets"
}

$shellDll = Get-ChildItem -Path $nativeRoot -Recurse -Filter 'Converty.ShellExtension.dll' -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $shellDll) {
    throw 'Converty.ShellExtension.dll is missing. Run ./build/native-smoke.ps1 first.'
}

if (Test-Path $layout) {
    Remove-Item -Recurse -Force $layout
}
New-Item -ItemType Directory -Force $layout | Out-Null

Copy-Item -Path (Join-Path $bridgeOutput '*') -Destination $layout -Recurse -Force
Copy-Item -Path (Join-Path $hostOutput '*') -Destination $layout -Recurse -Force
Copy-Item -Path (Join-Path $workerOutput '*') -Destination $layout -Recurse -Force
Copy-Item -Path (Join-Path $probeWorkerOutput '*') -Destination $layout -Recurse -Force
Copy-Item -Path $manifest -Destination (Join-Path $layout 'AppxManifest.xml') -Force
Copy-Item -Path $assets -Destination (Join-Path $layout 'Assets') -Recurse -Force
Copy-Item -Path $shellDll.FullName -Destination (Join-Path $layout 'Converty.ShellExtension.dll') -Force

$ffmpegDestination = Join-Path $layout 'tools/ffmpeg'
if ($FfmpegPath) {
    $resolvedFfmpeg = (Resolve-Path -LiteralPath $FfmpegPath -ErrorAction Stop).Path
    if ([System.IO.Path]::GetFileName($resolvedFfmpeg) -ine 'ffmpeg.exe') {
        throw 'The trusted converter input must be named ffmpeg.exe.'
    }

    New-Item -ItemType Directory -Force $ffmpegDestination | Out-Null
    Copy-Item -LiteralPath $resolvedFfmpeg -Destination (Join-Path $ffmpegDestination 'ffmpeg.exe') -Force
}

if ($FfprobePath) {
    $resolvedFfprobe = (Resolve-Path -LiteralPath $FfprobePath -ErrorAction Stop).Path
    if ([System.IO.Path]::GetFileName($resolvedFfprobe) -ine 'ffprobe.exe') {
        throw 'The trusted probe input must be named ffprobe.exe.'
    }

    New-Item -ItemType Directory -Force $ffmpegDestination | Out-Null
    Copy-Item -LiteralPath $resolvedFfprobe -Destination (Join-Path $ffmpegDestination 'ffprobe.exe') -Force
}

Write-Host "Development package layout: $layout"
Write-Host "Bridge: $(Join-Path $layout 'Converty.Bridge.exe')"
Write-Host "Host: $(Join-Path $layout 'Converty.Host.exe')"
Write-Host "Engine worker: $(Join-Path $layout 'Converty.EngineWorker.exe')"
Write-Host "Probe worker: $(Join-Path $layout 'Converty.ProbeWorker.exe')"
Write-Host "Explorer DLL: $(Join-Path $layout 'Converty.ShellExtension.dll')"
if (-not $FfmpegPath) {
    Write-Host 'FFmpeg was not staged; Explorer registration can be validated, but conversions will report the missing trusted converter.'
}
if (-not $FfprobePath) {
    Write-Host 'FFprobe was not staged; ProbeWorker execution will report the missing trusted probe engine.'
}
