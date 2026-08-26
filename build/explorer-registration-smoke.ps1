[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$manifest = Join-Path $layout 'AppxManifest.xml'
$shellDll = Join-Path $layout 'Converty.ShellExtension.dll'
$nativeRoot = Join-Path $root 'artifacts/native-smoke'
$smokeRoot = Join-Path $root 'artifacts/explorer-registration-smoke'
$packageName = 'Converty.Dev'

if (-not $IsWindows) {
    throw 'The Explorer registration smoke is Windows-only.'
}
if (-not (Test-Path $manifest)) {
    throw 'Development package layout is missing. Stage it before registration smoke.'
}
if (-not (Test-Path $shellDll)) {
    throw 'Staged Converty.ShellExtension.dll is missing.'
}

$smoke = Get-ChildItem -Path $nativeRoot -Recurse -Filter 'Converty.ExplorerRegistrationSmoke.exe' -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $smoke) {
    throw 'Explorer registration smoke executable is missing from the native build.'
}

$preexisting = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
if ($preexisting.Count -gt 0) {
    throw "A $packageName development package is already registered. Remove it before running this smoke to avoid testing stale registration."
}

if (Test-Path $smokeRoot) {
    Remove-Item -Recurse -Force $smokeRoot
}
New-Item -ItemType Directory -Force $smokeRoot | Out-Null

$input = Join-Path $smokeRoot 'Explorer invoke Hör & [x].wav'
$output = [System.IO.Path]::ChangeExtension($input, '.mp3')

# Generate a valid, tiny PCM source so the shell command's Invoke path can launch
# the staged Bridge and complete one real FFmpeg conversion.
$sampleRate = 8000
$sampleCount = 1600
$channels = 1
$bitsPerSample = 16
$bytesPerSample = $bitsPerSample / 8
$dataBytes = $sampleCount * $channels * $bytesPerSample
$stream = [System.IO.File]::Open($input, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
$writer = [System.IO.BinaryWriter]::new($stream, [System.Text.Encoding]::ASCII, $false)
try {
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
    $writer.Write([int](36 + $dataBytes))
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVE'))
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('fmt '))
    $writer.Write([int]16)
    $writer.Write([int16]1)
    $writer.Write([int16]$channels)
    $writer.Write([int]$sampleRate)
    $writer.Write([int]($sampleRate * $channels * $bytesPerSample))
    $writer.Write([int16]($channels * $bytesPerSample))
    $writer.Write([int16]$bitsPerSample)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
    $writer.Write([int]$dataBytes)
    for ($index = 0; $index -lt $sampleCount; $index++) {
        $sample = [int16](12000 * [Math]::Sin(2 * [Math]::PI * 440 * $index / $sampleRate))
        $writer.Write($sample)
    }
}
finally {
    $writer.Dispose()
}

# First prove the exact staged DLL exports a usable COM class factory and that
# IExplorerCommand::Invoke reaches the staged Bridge + FFmpeg, independent of
# whether this hosted Windows image permits loose MSIX registration.
& $smoke.FullName '--module' $shellDll $input
if ($LASTEXITCODE -ne 0) {
    throw "Direct staged shell DLL activation/invoke smoke failed with exit code $LASTEXITCODE."
}
if (-not (Test-Path $output) -or (Get-Item $output).Length -le 0) {
    throw 'Direct staged shell DLL Invoke did not produce a non-empty conversion output.'
}
Write-Host 'Direct staged shell DLL class-factory + Invoke conversion smoke: PASS'
Remove-Item -LiteralPath $output -Force

try {
    Add-AppxPackage -Register $manifest -ForceApplicationShutdown

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction Stop)
    if ($registered.Count -ne 1) {
        throw "Expected exactly one registered $packageName package; found $($registered.Count)."
    }

    & $smoke.FullName $input
    if ($LASTEXITCODE -ne 0) {
        throw "Packaged Explorer COM activation/invoke smoke failed with exit code $LASTEXITCODE."
    }

    if (-not (Test-Path $output)) {
        throw "Explorer COM Invoke did not create the expected conversion output: $output"
    }
    if ((Get-Item $output).Length -le 0) {
        throw 'Explorer COM Invoke produced an empty conversion output.'
    }

    Write-Host 'Explorer package registration, COM activation, and Invoke conversion smoke: PASS'
    Write-Host "Registered layout: $layout"
    Write-Host "Source: $input"
    Write-Host "Converted output: $output"
}
finally {
    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
    foreach ($package in $registered) {
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction SilentlyContinue
    }
}
