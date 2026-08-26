[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$output = Join-Path $root 'artifacts/Converty_dev_unsigned.msix'

if (-not $IsWindows) {
    throw 'The Converty development package can only be validated on Windows.'
}
if (-not (Test-Path (Join-Path $layout 'AppxManifest.xml'))) {
    throw 'Development package layout is missing. Run ./build/stage-dev-package.ps1 first.'
}

$kitsRoot = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots' -ErrorAction Stop).KitsRoot10
if (-not $kitsRoot) {
    throw 'Windows 10/11 SDK installation root was not found.'
}

$makeAppx = Get-ChildItem -Path (Join-Path $kitsRoot 'bin') -Directory |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName 'x64/makeappx.exe' } |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $makeAppx) {
    throw 'makeappx.exe was not found in the installed Windows SDK.'
}

if (Test-Path $output) {
    Remove-Item -Force $output
}

& $makeAppx pack /d $layout /p $output /o
if ($LASTEXITCODE -ne 0) {
    throw 'MakeAppx rejected the Converty development package layout.'
}
if (-not (Test-Path $output)) {
    throw 'MakeAppx returned success without producing the unsigned development MSIX.'
}

Write-Host "Development package schema/layout validation: PASS"
Write-Host "Unsigned validation package: $output"
Write-Host 'This file is intentionally unsigned and is not a release package.'
