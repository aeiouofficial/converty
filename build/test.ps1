[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:TESTINGPLATFORM_TELEMETRY_OPTOUT = '1'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet test --solution Converty.slnx --configuration $Configuration --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet test failed.' }

    python scripts/generate_sbom.py --mode source
    if ($LASTEXITCODE -ne 0) { throw 'source SBOM generation failed.' }

    python scripts/verify_contract_vectors.py
    if ($LASTEXITCODE -ne 0) { throw 'contract vector verification failed.' }

    python scripts/verify_repository.py
    if ($LASTEXITCODE -ne 0) { throw 'repository static verification failed.' }

    python -m pytest -q tests/static
    if ($LASTEXITCODE -ne 0) { throw 'Python static/schema tests failed.' }
}
finally {
    Pop-Location
}
