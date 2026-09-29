[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$MsixPath,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ExpectedSha256,
    [Parameter(Mandatory = $true)][string]$ExpectedPublisherSubject,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ExpectedSignerCertificateSha256
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows) { throw 'Production MSIX verification requires Windows.' }
$resolved = (Resolve-Path -LiteralPath $MsixPath -ErrorAction Stop).Path

$actual = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
if ($actual -ine $ExpectedSha256) { throw "MSIX SHA-256 mismatch: $actual" }

$signature = Get-AuthenticodeSignature -LiteralPath $resolved
if ($signature.Status -ne [System.Management.Automation.SignatureStatus]::Valid) {
    throw "MSIX Authenticode signature is not valid: $($signature.Status)"
}
if ($null -eq $signature.SignerCertificate) { throw 'MSIX signer certificate is missing.' }
if ($signature.SignerCertificate.Subject -ne $ExpectedPublisherSubject) {
    throw "MSIX publisher mismatch: $($signature.SignerCertificate.Subject)"
}
$certHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($signature.SignerCertificate.RawData))
if ($certHash -ine $ExpectedSignerCertificateSha256) { throw "Signer certificate SHA-256 mismatch: $certHash" }
if ($null -eq $signature.TimeStamperCertificate) { throw 'MSIX timestamp evidence is missing.' }

$kitsRoot = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots' -ErrorAction Stop).KitsRoot10
$signtool = Get-ChildItem -Path (Join-Path $kitsRoot 'bin') -Directory |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName 'x64\signtool.exe' } |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1
if (-not $signtool) { throw 'signtool.exe was not found in the Windows SDK.' }

& $signtool verify /pa /v $resolved
if ($LASTEXITCODE -ne 0) { throw 'signtool rejected the production MSIX signature.' }

Write-Host "Production MSIX cryptographic verification: PASS"
Write-Host "MSIX SHA-256: $($actual.ToLowerInvariant())"
Write-Host "Publisher: $ExpectedPublisherSubject"
Write-Host "Signer certificate SHA-256: $($certHash.ToLowerInvariant())"
