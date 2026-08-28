[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$manifest = Join-Path $layout 'AppxManifest.xml'
$hostPath = Join-Path $layout 'Converty.Host.exe'
$securityAssembly = Join-Path $layout 'Converty.Security.dll'
$bridgeAssembly = Join-Path $layout 'Converty.Bridge.dll'
$packageName = 'Converty.Dev'

if (-not $IsWindows) {
    throw 'The B2 packaged Host identity smoke is Windows-only.'
}
if (-not (Test-Path -LiteralPath $manifest)) {
    throw 'Development package layout is missing. Stage it before the B2 identity smoke.'
}
if (-not (Test-Path -LiteralPath $hostPath)) {
    throw 'B2 RED: staged development package is missing Converty.Host.exe.'
}
if (-not (Test-Path -LiteralPath $securityAssembly)) {
    throw 'Staged Converty.Security.dll is missing.'
}
if (-not (Test-Path -LiteralPath $bridgeAssembly)) {
    throw 'Staged Converty.Bridge.dll is missing.'
}

$preexisting = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
if ($preexisting.Count -gt 0) {
    throw "A $packageName development package is already registered. Remove it before running this smoke to avoid stale package identity."
}

$hostProcess = $null
$registeredPackage = $null
try {
    Add-AppxPackage -Register $manifest -ForceApplicationShutdown

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction Stop)
    if ($registered.Count -ne 1) {
        throw "Expected exactly one registered $packageName package; found $($registered.Count)."
    }
    $registeredPackage = $registered[0]
    if ([string]::IsNullOrWhiteSpace($registeredPackage.PackageFamilyName)) {
        throw 'Registered development package has no PackageFamilyName.'
    }

    [void][System.Reflection.Assembly]::LoadFrom($securityAssembly)
    [void][System.Reflection.Assembly]::LoadFrom($bridgeAssembly)

    $hostProcess = Start-Process -FilePath $hostPath -WorkingDirectory $layout -PassThru -WindowStyle Hidden
    if ($null -eq $hostProcess) {
        throw 'Unable to launch staged Converty.Host.exe.'
    }

    $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().User
    if ($null -eq $currentUser) {
        throw 'Current Windows identity has no user SID.'
    }
    $pipeName = [Converty.Security.Ipc.PipeEndpointName]::ForUser($currentUser)
    $probe = [Converty.Bridge.Ipc.WindowsConnectedServerIdentityProbe]::new()
    $verifier = [Converty.Bridge.Ipc.WindowsConnectedServerIdentityVerifier]::new(
        $hostPath,
        $registeredPackage.PackageFamilyName,
        $probe)

    $client = [System.IO.Pipes.NamedPipeClientStream]::new(
        '.',
        $pipeName,
        [System.IO.Pipes.PipeDirection]::InOut,
        [System.IO.Pipes.PipeOptions]::Asynchronous,
        [System.Security.Principal.TokenImpersonationLevel]::Impersonation)
    try {
        $client.Connect(5000)
        $verifier.VerifyConnectedServer($client)
        $snapshot = $probe.Capture($client)

        if ($snapshot.ServerProcessId -eq 0 -or $snapshot.ServerProcessId -ne $snapshot.ConfirmedServerProcessId) {
            throw 'Packaged Host identity smoke observed an unstable server PID.'
        }
        if (-not [string]::Equals($snapshot.ImagePath, $hostPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Packaged Host image mismatch: '$($snapshot.ImagePath)' != '$hostPath'."
        }
        if (-not [string]::Equals($snapshot.PackageFamilyName, $registeredPackage.PackageFamilyName, [System.StringComparison]::Ordinal)) {
            throw "Packaged Host family mismatch: '$($snapshot.PackageFamilyName)' != '$($registeredPackage.PackageFamilyName)'."
        }

        Write-Host "B2 packaged Host server identity: PASS (PID $($snapshot.ServerProcessId), PFN $($snapshot.PackageFamilyName))"
    }
    finally {
        $client.Dispose()
    }

    if (-not $hostProcess.HasExited) {
        Stop-Process -Id $hostProcess.Id -Force -ErrorAction Stop
        $hostProcess.WaitForExit()
    }
    $hostProcess.Dispose()
    $hostProcess = $null

    $squatterPipeName = 'converty.b2.squatter.' + [Guid]::NewGuid().ToString('N')
    $squatterServer = [System.IO.Pipes.NamedPipeServerStream]::new(
        $squatterPipeName,
        [System.IO.Pipes.PipeDirection]::InOut,
        1,
        [System.IO.Pipes.PipeTransmissionMode]::Byte,
        [System.IO.Pipes.PipeOptions]::Asynchronous)
    $squatterClient = [System.IO.Pipes.NamedPipeClientStream]::new(
        '.',
        $squatterPipeName,
        [System.IO.Pipes.PipeDirection]::InOut,
        [System.IO.Pipes.PipeOptions]::Asynchronous,
        [System.Security.Principal.TokenImpersonationLevel]::Impersonation)
    try {
        $accept = $squatterServer.WaitForConnectionAsync()
        $squatterClient.Connect(2000)
        $accept.GetAwaiter().GetResult()

        $rejected = $false
        try {
            $verifier.VerifyConnectedServer($squatterClient)
        }
        catch [Converty.Bridge.Ipc.BridgeServerIdentityException] {
            $rejected = $true
        }
        if (-not $rejected) {
            throw 'Same-user unpackaged named-pipe squatter was accepted as Converty Host.'
        }

        $squatterClient.Dispose()
        $closedRead = $squatterServer.ReadByte()
        if ($closedRead -ne -1) {
            throw 'Squatter received application data after server-identity rejection.'
        }
        Write-Host 'B2 same-user unpackaged squatter rejection before application data: PASS'
    }
    finally {
        $squatterClient.Dispose()
        $squatterServer.Dispose()
    }
}
finally {
    if ($null -ne $hostProcess) {
        try {
            if (-not $hostProcess.HasExited) {
                Stop-Process -Id $hostProcess.Id -Force -ErrorAction SilentlyContinue
                $hostProcess.WaitForExit()
            }
        }
        finally {
            $hostProcess.Dispose()
        }
    }

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
    foreach ($package in $registered) {
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction SilentlyContinue
    }
}
