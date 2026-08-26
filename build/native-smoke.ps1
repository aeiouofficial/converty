[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$nativeBuild = Join-Path $root 'artifacts/native-smoke'

function Enter-ConvertyMsvcEnvironment {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (-not (Test-Path $vswhere)) {
        throw "Visual Studio locator not found: $vswhere"
    }

    $installPath = (& $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath).Trim()
    if (-not $installPath) {
        throw 'Visual Studio with the MSVC x64 toolchain is required for the Explorer shell extension.'
    }

    $devShell = Join-Path $installPath 'Common7/Tools/Launch-VsDevShell.ps1'
    if (-not (Test-Path $devShell)) {
        throw "Visual Studio developer shell script not found: $devShell"
    }

    & $devShell -Arch amd64 -HostArch amd64 -SkipAutomaticLocation
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        throw 'Visual Studio developer shell initialization failed.'
    }

    $cl = Get-Command cl.exe -ErrorAction SilentlyContinue
    if (-not $cl) {
        throw 'MSVC cl.exe was not available after Visual Studio developer shell initialization.'
    }

    $env:CC = 'cl.exe'
    $env:CXX = 'cl.exe'
    Write-Host "Native compiler: $($cl.Source)"
}

Push-Location $root
try {
    if (-not $IsWindows) {
        throw 'The Converty Explorer shell extension must be built on Windows.'
    }

    Enter-ConvertyMsvcEnvironment

    # A CMake cache permanently records its first compiler. Remove it so a
    # previous PATH-preferred MinGW configure cannot mask the required MSVC SDK.
    if (Test-Path $nativeBuild) {
        Remove-Item -Recurse -Force $nativeBuild
    }

    cmake --preset native-smoke
    if ($LASTEXITCODE -ne 0) { throw 'Native CMake configure failed.' }

    cmake --build --preset native-smoke
    if ($LASTEXITCODE -ne 0) { throw 'Native Explorer smoke build failed.' }

    $dll = Get-ChildItem -Path $nativeBuild -Recurse -Filter 'Converty.ShellExtension.dll' -File | Select-Object -First 1
    if (-not $dll) {
        throw 'Native Explorer build completed without Converty.ShellExtension.dll.'
    }

    Write-Host "Native Explorer DLL: $($dll.FullName)"
}
finally {
    Pop-Location
}
