[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$manifest = Join-Path $layout 'AppxManifest.xml'
$bridgePath = Join-Path $layout 'Converty.Bridge.exe'
$nativeRoot = Join-Path $root 'artifacts/native-smoke'
$diagnosticRoot = Join-Path $root 'artifacts/b2-bridge-package-identity-diagnostic'
$packageName = 'Converty.Dev'

if (-not $IsWindows) {
    throw 'The B2 shell-launched Bridge package-identity diagnostic is Windows-only.'
}
if (-not (Test-Path -LiteralPath $manifest)) {
    throw 'Development package layout is missing. Stage it before the Bridge identity diagnostic.'
}
if (-not (Test-Path -LiteralPath $bridgePath)) {
    throw 'Staged Converty.Bridge.exe is missing.'
}

$smoke = Get-ChildItem -Path $nativeRoot -Recurse -Filter 'Converty.ExplorerRegistrationSmoke.exe' -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $smoke) {
    throw 'Explorer registration smoke executable is missing from the native build.'
}

$preexisting = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
if ($preexisting.Count -gt 0) {
    throw "A $packageName development package is already registered. Remove it before this diagnostic to avoid stale package authority."
}

if (Test-Path -LiteralPath $diagnosticRoot) {
    Remove-Item -LiteralPath $diagnosticRoot -Recurse -Force
}
New-Item -ItemType Directory -Force $diagnosticRoot | Out-Null

$input = Join-Path $diagnosticRoot 'Converty.B2.BridgeIdentityProbe Hör & [x].wav'
$output = [System.IO.Path]::ChangeExtension($input, '.mp3')
$evidence = "$input.bridge-identity.json"

$sampleRate = 44100
$sampleCount = 8820
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

try {
    Add-AppxPackage -Register $manifest -ForceApplicationShutdown

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction Stop)
    if ($registered.Count -ne 1) {
        throw "Expected exactly one registered $packageName package; found $($registered.Count)."
    }
    $package = $registered[0]
    if ([string]::IsNullOrWhiteSpace($package.PackageFamilyName)) {
        throw 'Registered development package has no PackageFamilyName.'
    }

    Write-Host "B2_EXPECTED_PACKAGE_FAMILY=$($package.PackageFamilyName)"
    Write-Host "B2_EXPECTED_BRIDGE_PATH=$bridgePath"

    # Packaged mode deliberately uses CoCreateInstance on the registered COM
    # class. Its child IExplorerCommand::Invoke executes the production
    # LaunchBridge/CreateProcessW path in Converty.ShellExtension.dll.
    & $smoke.FullName $input
    $smokeExit = $LASTEXITCODE
    if ($smokeExit -ne 0) {
        throw "Packaged Explorer COM activation/Invoke diagnostic failed with exit code $smokeExit."
    }

    if (-not (Test-Path -LiteralPath $output) -or (Get-Item -LiteralPath $output).Length -le 0) {
        throw 'Packaged Explorer COM Invoke did not complete the normal Bridge conversion before identity evaluation.'
    }

    if (-not (Test-Path -LiteralPath $evidence)) {
        throw 'B2 BRIDGE IDENTITY RED: packaged COM Invoke reached normal Bridge conversion, but the shell-launched Bridge emitted no identity evidence.'
    }

    $identity = Get-Content -LiteralPath $evidence -Raw | ConvertFrom-Json
    if ([int64]$identity.ProcessId -le 0) {
        throw 'Shell-launched Bridge identity evidence has no valid process ID.'
    }
    if (-not [string]::Equals([string]$identity.ImagePath, $bridgePath, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Shell-launched Bridge image mismatch: '$($identity.ImagePath)' != '$bridgePath'."
    }
    if ([int]$identity.PackageFamilyResult -ne 0) {
        throw "Shell-launched Bridge GetCurrentPackageFamilyName failed with result $($identity.PackageFamilyResult)."
    }
    if (-not [string]::Equals([string]$identity.PackageFamilyName, $package.PackageFamilyName, [System.StringComparison]::Ordinal)) {
        throw "Shell-launched Bridge family mismatch: '$($identity.PackageFamilyName)' != '$($package.PackageFamilyName)'."
    }

    Write-Host "B2_BRIDGE_PROCESS_ID=$($identity.ProcessId)"
    Write-Host "B2_BRIDGE_IMAGE_PATH=$($identity.ImagePath)"
    Write-Host "B2_BRIDGE_PACKAGE_FAMILY_RESULT=$($identity.PackageFamilyResult)"
    Write-Host "B2_BRIDGE_PACKAGE_FAMILY=$($identity.PackageFamilyName)"
    Write-Host 'B2_BRIDGE_PACKAGE_IDENTITY_RESULT=PRESERVED'

    if ($identity.HostSubmissionAccepted -ne $true) {
        throw 'B2 HOST AUTH RED: shell-launched packaged Bridge did not complete an authenticated Host submission.'
    }
    $hostJobId = [Guid]::Empty
    if (-not [Guid]::TryParse([string]$identity.HostJobId, [ref]$hostJobId) -or $hostJobId -eq [Guid]::Empty) {
        throw "Authenticated Host submission returned no valid job ID: '$($identity.HostJobId)'."
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$identity.HostSubmissionReason)) {
        throw "Authenticated Host submission unexpectedly returned a rejection reason: '$($identity.HostSubmissionReason)'."
    }

    Write-Host 'B2_PACKAGED_BRIDGE_HOST_AUTH_RESULT=ACCEPTED'
    Write-Host "B2_PACKAGED_BRIDGE_HOST_JOB_ID=$hostJobId"
}
finally {
    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
    foreach ($package in $registered) {
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction SilentlyContinue
    }
}
