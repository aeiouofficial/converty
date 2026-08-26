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
if (-not $pin.archiveUrl -or -not $pin.archiveSha256 -or
    $pin.expectedExecutableName -ne 'ffmpeg.exe' -or
    $pin.expectedProbeExecutableName -ne 'ffprobe.exe') {
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
$ffmpegVersionOutput | Select-Object -First 1 | Write-Host

$ffprobeVersionOutput = @(& $ffprobeOutputPath -hide_banner -version 2>&1)
$ffprobeExitCode = $LASTEXITCODE
if ($ffprobeExitCode -ne 0) {
    throw "Pinned development ffprobe.exe did not execute successfully (exit code $ffprobeExitCode)."
}
$ffprobeVersionOutput | Select-Object -First 1 | Write-Host

Write-Host "Development FFmpeg archive SHA-256: $actualHash"
Write-Host "Trusted development FFmpeg: $ffmpegOutputPath"
Write-Host "Development probe verifier: $ffprobeOutputPath"
Write-Host 'These payloads are development qualification input only; they are not production redistribution approval.'
