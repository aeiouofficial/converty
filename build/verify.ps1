[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/bootstrap.ps1"
& "$PSScriptRoot/dependency-audit.ps1"
python "$PSScriptRoot/../scripts/verify_release_inputs.py"
if ($LASTEXITCODE -ne 0) { throw 'release input preflight failed.' }
& "$PSScriptRoot/build.ps1" -Configuration $Configuration
& "$PSScriptRoot/test.ps1" -Configuration $Configuration
