[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$pinPath = Join-Path $root 'eng/ffmpeg-development.json'
$workRoot = Join-Path $root 'artifacts/dev-ffmpeg'
$archivePath = Join-Path $workRoot 'ffmpeg.zip'
$extractRoot = Join-Path $workRoot 'expanded'
$outputPath = Join-Path $workRoot 'ffmpeg.exe'

if (-not $IsWindows) {
    throw 'The pinned development FFmpeg payload is Windows-only.'
}
if (-not (Test-Path $pinPath)) {
    throw "Development FFmpeg pin is missing: $pinPath"
}

$pin = Get-Content -Raw $pinPath | ConvertFrom-Json
if ($pin.purpose -ne 'development-qualification-only') {
    throw 'Development FFmpeg pin has an unexpected purpose.'
}
if (-not $pin.archiveUrl -or -not $pin.archiveSha256 -or $pin.expectedExecutableName -ne 'ffmpeg.exe') {
    throw 'Development FFmpeg pin is incomplete.'
}

if (Test-Path $workRoot) {
    Remove-Item -Recurse -Force $workRoot
}
New-Item -ItemType Directory -Force $workRoot | Out-Null

Invoke-WebRequest -Uri $pin.archiveUrl -OutFile $archivePath -UseBasicParsing
$actualHash = (Get-FileHash -Algorithm SHA256 $archivePath).Hash.ToLowerInvariant()
$expectedHash = ([string]$pin.archiveSha256).ToLowerInvariant()
if ($actualHash -ne $expectedHash) {
    throw "Pinned FFmpeg archive hash mismatch. Expected $expectedHash, got $actualHash."
}

Expand-Archive -LiteralPath $archivePath -DestinationPath $extractRoot -Force
$matches = @(Get-ChildItem -Path $extractRoot -Recurse -Filter 'ffmpeg.exe' -File)
if ($matches.Count -ne 1) {
    throw "Expected exactly one ffmpeg.exe in the pinned archive; found $($matches.Count)."
}

Copy-Item -LiteralPath $matches[0].FullName -Destination $outputPath -Force
& $outputPath -hide_banner -version | Select-Object -First 1 | Write-Host
if ($LASTEXITCODE -ne 0) {
    throw 'Pinned development ffmpeg.exe did not execute successfully.'
}

Write-Host "Development FFmpeg archive SHA-256: $actualHash"
Write-Host "Trusted development FFmpeg: $outputPath"
Write-Host 'This payload is development qualification input only; it is not production redistribution approval.'
