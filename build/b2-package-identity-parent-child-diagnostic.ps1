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
$canaryOutput = Join-Path $root "tests/Converty.PackageIdentityCanary/bin/$Configuration/net10.0"
$canaryExe = Join-Path $layout 'Converty.PackageIdentityCanary.exe'
$packageName = 'Converty.Dev'
$appId = 'B2IdentityCanary'
$activatedPid = 0
$evidencePath = Join-Path $env:TEMP ("converty-b2-parent-child-identity-{0}.json" -f [Guid]::NewGuid().ToString('N'))

if (-not $IsWindows) {
    throw 'The B2 package-identity parent/child diagnostic is Windows-only.'
}
if (-not (Test-Path -LiteralPath $manifest)) {
    throw 'Development package layout is missing. Stage it before running the diagnostic.'
}
if (-not (Test-Path -LiteralPath $hostPath)) {
    throw 'Staged Converty.Host.exe is missing.'
}
if (-not (Test-Path -LiteralPath $canaryOutput)) {
    throw "Package identity canary build output is missing: $canaryOutput"
}
if (-not (Test-Path -LiteralPath (Join-Path $canaryOutput 'Converty.PackageIdentityCanary.exe'))) {
    throw 'Converty.PackageIdentityCanary.exe is missing from its build output.'
}

Get-ChildItem -LiteralPath $canaryOutput -File | Where-Object {
    $_.Name -like 'Converty.PackageIdentityCanary.*'
} | Copy-Item -Destination $layout -Force

if (-not (Test-Path -LiteralPath $canaryExe)) {
    throw 'The package identity canary was not copied into the development package layout.'
}

$preexisting = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
if ($preexisting.Count -gt 0) {
    throw "A $packageName development package is already registered on this runner. Refusing stale identity state."
}

$activationSource = @'
using System;
using System.Runtime.InteropServices;

[Flags]
public enum ActivateOptions : uint
{
    None = 0x00000000,
}

[ComImport]
[Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IApplicationActivationManager
{
    [PreserveSig]
    int ActivateApplication(
        [MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)] string arguments,
        ActivateOptions options,
        out uint processId);
}

[ComImport]
[Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
public class ApplicationActivationManager
{
}

public static class PackageActivation
{
    public static uint Activate(string appUserModelId, string arguments)
    {
        var manager = (IApplicationActivationManager)new ApplicationActivationManager();
        int hr = manager.ActivateApplication(appUserModelId, arguments, ActivateOptions.None, out uint processId);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
        return processId;
    }
}
'@

Add-Type -TypeDefinition $activationSource -Language CSharp

try {
    Add-AppxPackage -Register $manifest -ForceApplicationShutdown

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction Stop)
    if ($registered.Count -ne 1) {
        throw "Expected exactly one registered $packageName package; found $($registered.Count)."
    }

    $registeredPackage = $registered[0]
    $pfn = [string]$registeredPackage.PackageFamilyName
    if ([string]::IsNullOrWhiteSpace($pfn)) {
        throw 'Registered development package has no PackageFamilyName.'
    }

    $aumid = "$pfn!$appId"
    $activationArguments = '"' + $evidencePath + '"'
    $activatedPid = [int][PackageActivation]::Activate($aumid, $activationArguments)
    if ($activatedPid -le 0) {
        throw 'AUMID activation returned no process id.'
    }

    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    while (-not (Test-Path -LiteralPath $evidencePath) -and [DateTime]::UtcNow -lt $deadline) {
        Start-Sleep -Milliseconds 100
    }
    if (-not (Test-Path -LiteralPath $evidencePath)) {
        throw "Package-activated parent did not write diagnostic evidence: $evidencePath"
    }

    $evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
    Write-Host (Get-Content -LiteralPath $evidencePath -Raw)

    if ([int]$evidence.ParentProcessId -ne $activatedPid) {
        throw "Activation PID mismatch: activation manager returned $activatedPid, evidence reported $($evidence.ParentProcessId)."
    }
    if ([int]$evidence.ParentPackageFamilyError -ne 0) {
        throw "Package-activated parent failed GetCurrentPackageFamilyName with error $($evidence.ParentPackageFamilyError)."
    }
    if (-not [string]::Equals([string]$evidence.ParentPackageFamilyName, $pfn, [System.StringComparison]::Ordinal)) {
        throw "Package-activated parent PFN mismatch: '$($evidence.ParentPackageFamilyName)' != '$pfn'."
    }
    if (-not [string]::Equals([System.IO.Path]::GetFullPath([string]$evidence.HostPath), [System.IO.Path]::GetFullPath($hostPath), [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Diagnostic launched the wrong Host path: '$($evidence.HostPath)' != '$hostPath'."
    }
    if (-not [string]::IsNullOrWhiteSpace([string]$evidence.Failure)) {
        throw "Diagnostic parent reported a launch failure: $($evidence.Failure)"
    }
    if ([int]$evidence.ChildProcessId -le 0) {
        throw 'Diagnostic parent did not start Converty.Host.exe.'
    }

    if ([int]$evidence.ChildPackageFamilyError -eq 0) {
        if (-not [string]::Equals([string]$evidence.ChildPackageFamilyName, $pfn, [System.StringComparison]::Ordinal)) {
            throw "Child received an unexpected package family '$($evidence.ChildPackageFamilyName)' instead of '$pfn'."
        }
        Write-Host 'B2_PARENT_CHILD_IDENTITY_RESULT=PRESERVED'
        Write-Host "Package-identified parent PFN: $pfn"
        Write-Host "Direct child Host PFN: $($evidence.ChildPackageFamilyName)"
    }
    elseif ([int]$evidence.ChildPackageFamilyError -eq 15700 -and [string]::IsNullOrWhiteSpace([string]$evidence.ChildPackageFamilyName)) {
        Write-Host 'B2_PARENT_CHILD_IDENTITY_RESULT=NOT_PRESERVED'
        Write-Host "Package-identified parent PFN: $pfn"
        Write-Host 'Direct child Host PFN: <none> (APPMODEL_ERROR_NO_PACKAGE)'
    }
    else {
        throw "Unexpected GetPackageFamilyName result for child: error=$($evidence.ChildPackageFamilyError), family='$($evidence.ChildPackageFamilyName)'."
    }
}
finally {
    if ($activatedPid -gt 0) {
        Stop-Process -Id $activatedPid -Force -ErrorAction SilentlyContinue
    }

    if (Test-Path -LiteralPath $evidencePath) {
        Remove-Item -LiteralPath $evidencePath -Force -ErrorAction SilentlyContinue
    }

    $registered = @(Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue)
    foreach ($package in $registered) {
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction SilentlyContinue
    }
}
