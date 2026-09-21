[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$pinPath = Join-Path $root 'eng/ffmpeg-development.json'
$workRoot = Join-Path $root 'artifacts/dev-ffmpeg'
$archivePath = Join-Path $workRoot 'ffmpeg.zip'
$extractRoot = Join-Path $workRoot 'expanded'
$ffmpegOutputPath = Join-Path $workRoot 'ffmpeg.exe'
$ffprobeOutputPath = Join-Path $workRoot 'ffprobe.exe'

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

$requiredStrings = @(
    'version',
    'vendor',
    'releaseTag',
    'archiveAssetName',
    'assetApiUrl',
    'archiveUrl',
    'archiveSha256',
    'expectedVersionToken',
    'sourceCommit'
)
foreach ($field in $requiredStrings) {
    if ([string]::IsNullOrWhiteSpace([string]$pin.$field)) {
        throw "Development FFmpeg pin is missing '$field'."
    }
}

if ($pin.expectedExecutableName -ne 'ffmpeg.exe' -or
    $pin.expectedProbeExecutableName -ne 'ffprobe.exe') {
    throw 'Development FFmpeg executable names are invalid.'
}
if ([long]$pin.releaseAssetId -le 0) {
    throw 'Development FFmpeg releaseAssetId must be positive.'
}
if ([long]$pin.archiveBytes -le 0) {
    throw 'Development FFmpeg archiveBytes must be positive.'
}
if ([string]$pin.releaseTag -notmatch '^autobuild-\d{4}-\d{2}-\d{2}-\d{2}-\d{2}$') {
    throw 'Development FFmpeg releaseTag must identify one exact autobuild.'
}
if ([string]$pin.archiveSha256 -notmatch '^[0-9a-fA-F]{64}$') {
    throw 'Development FFmpeg archive SHA-256 is invalid.'
}
if ([string]$pin.sourceCommit -notmatch '^[0-9a-fA-F]{10}$') {
    throw 'Development FFmpeg sourceCommit is invalid.'
}
if ([string]$pin.archiveUrl -like '*/latest/*') {
    throw 'Development FFmpeg archive URL must not use a mutable latest alias.'
}

$expectedArchiveUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/$($pin.releaseTag)/$($pin.archiveAssetName)"
if ([string]$pin.archiveUrl -cne $expectedArchiveUrl) {
    throw 'Development FFmpeg archive URL is not bound to the declared release tag and asset name.'
}
$expectedAssetApiUrl = "https://api.github.com/repos/BtbN/FFmpeg-Builds/releases/assets/$($pin.releaseAssetId)"
if ([string]$pin.assetApiUrl -cne $expectedAssetApiUrl) {
    throw 'Development FFmpeg assetApiUrl is not bound to releaseAssetId.'
}

if (Test-Path $workRoot) {
    Remove-Item -Recurse -Force $workRoot
}
New-Item -ItemType Directory -Force $workRoot | Out-Null

Invoke-WebRequest -Uri $pin.archiveUrl -OutFile $archivePath -UseBasicParsing
$archive = Get-Item -LiteralPath $archivePath
if ($archive.Length -ne [long]$pin.archiveBytes) {
    throw "Pinned FFmpeg archive size mismatch. Expected $($pin.archiveBytes), got $($archive.Length)."
}

$actualHash = (Get-FileHash -Algorithm SHA256 $archivePath).Hash.ToLowerInvariant()
$expectedHash = ([string]$pin.archiveSha256).ToLowerInvariant()
if ($actualHash -ne $expectedHash) {
    throw "Pinned FFmpeg archive hash mismatch. Expected $expectedHash, got $actualHash."
}

Expand-Archive -LiteralPath $archivePath -DestinationPath $extractRoot -Force
$ffmpegMatches = @(Get-ChildItem -Path $extractRoot -Recurse -Filter 'ffmpeg.exe' -File)
$ffprobeMatches = @(Get-ChildItem -Path $extractRoot -Recurse -Filter 'ffprobe.exe' -File)
if ($ffmpegMatches.Count -ne 1) {
    throw "Expected exactly one ffmpeg.exe in the pinned archive; found $($ffmpegMatches.Count)."
}
if ($ffprobeMatches.Count -ne 1) {
    throw "Expected exactly one ffprobe.exe in the pinned archive; found $($ffprobeMatches.Count)."
}

Copy-Item -LiteralPath $ffmpegMatches[0].FullName -Destination $ffmpegOutputPath -Force
Copy-Item -LiteralPath $ffprobeMatches[0].FullName -Destination $ffprobeOutputPath -Force

$ffmpegVersionOutput = @(& $ffmpegOutputPath -hide_banner -version 2>&1)
$ffmpegExitCode = $LASTEXITCODE
if ($ffmpegExitCode -ne 0) {
    throw "Pinned development ffmpeg.exe did not execute successfully (exit code $ffmpegExitCode)."
}
$ffmpegVersionLine = [string]($ffmpegVersionOutput | Select-Object -First 1)
if ($ffmpegVersionLine -notlike "*$($pin.expectedVersionToken)*") {
    throw "Pinned development ffmpeg.exe version mismatch: $ffmpegVersionLine"
}
$ffmpegVersionLine | Write-Host

$ffprobeVersionOutput = @(& $ffprobeOutputPath -hide_banner -version 2>&1)
$ffprobeExitCode = $LASTEXITCODE
if ($ffprobeExitCode -ne 0) {
    throw "Pinned development ffprobe.exe did not execute successfully (exit code $ffprobeExitCode)."
}
$ffprobeVersionLine = [string]($ffprobeVersionOutput | Select-Object -First 1)
if ($ffprobeVersionLine -notlike "*$($pin.expectedVersionToken)*") {
    throw "Pinned development ffprobe.exe version mismatch: $ffprobeVersionLine"
}
$ffprobeVersionLine | Write-Host

Write-Host "Development FFmpeg release tag: $($pin.releaseTag)"
Write-Host "Development FFmpeg asset ID: $($pin.releaseAssetId)"
Write-Host "Development FFmpeg source commit: $($pin.sourceCommit)"
Write-Host "Development FFmpeg archive SHA-256: $actualHash"
Write-Host "Trusted development FFmpeg: $ffmpegOutputPath"
Write-Host "Development probe verifier: $ffprobeOutputPath"
Write-Host 'These payloads are development qualification input only; they are not production redistribution approval.'
