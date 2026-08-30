[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/product-conversion-smoke'

if (-not $IsWindows) {
    throw 'The Converty product conversion smoke is Windows-only.'
}
if (-not (Test-Path -LiteralPath $bridge)) {
    throw 'Staged Converty.Bridge.exe is missing.'
}
if (-not (Test-Path -LiteralPath $ffmpeg)) {
    throw 'Staged trusted ffmpeg.exe is missing.'
}
if (-not (Test-Path -LiteralPath $ffprobe)) {
    throw 'Pinned development ffprobe.exe is missing.'
}

if (Test-Path -LiteralPath $smokeRoot) {
    Remove-Item -LiteralPath $smokeRoot -Recurse -Force
}
New-Item -ItemType Directory -Force $smokeRoot | Out-Null

$input = Join-Path $smokeRoot 'Hör test & semi; -dash [x].wav'

# Write a small 44.1 kHz mono PCM WAV without using another media tool. 44.1 kHz
# keeps the 320 kbps MP3 baseline valid while exercising every dev.15 audio target.
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

$sourceHash = (Get-FileHash -LiteralPath $input -Algorithm SHA256).Hash
$cases = @(
    [pscustomobject]@{ PresetId = 'audio.mp3'; Extension = '.mp3'; Codec = 'mp3'; ExpectedBitRate = 320000 },
    [pscustomobject]@{ PresetId = 'audio.m4a.aac'; Extension = '.m4a'; Codec = 'aac'; ExpectedBitRate = $null },
    [pscustomobject]@{ PresetId = 'audio.opus'; Extension = '.opus'; Codec = 'opus'; ExpectedBitRate = $null },
    [pscustomobject]@{ PresetId = 'audio.ogg.vorbis'; Extension = '.ogg'; Codec = 'vorbis'; ExpectedBitRate = $null }
)

foreach ($case in $cases) {
    $existingOutput = [System.IO.Path]::ChangeExtension($input, $case.Extension)
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($input)
    $expectedOutput = Join-Path $smokeRoot ($stem + ' (1)' + $case.Extension)

    # Reserve the base destination to prove Converty never silently overwrites it.
    [System.IO.File]::WriteAllBytes($existingOutput, [byte[]](1, 2, 3, 4))
    $existingHash = (Get-FileHash -LiteralPath $existingOutput -Algorithm SHA256).Hash

    # Bridge is a Windows GUI-subsystem executable (WinExe), so a shell invocation
    # does not reliably block or populate $LASTEXITCODE. Start it explicitly, keep
    # every argument structured, and wait for the exact process with a finite bound.
    $bridgeStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $bridgeStartInfo.FileName = $bridge
    $bridgeStartInfo.UseShellExecute = $false
    $bridgeStartInfo.CreateNoWindow = $true
    $bridgeStartInfo.WorkingDirectory = $layout
    $bridgeStartInfo.ArgumentList.Add('--preset')
    $bridgeStartInfo.ArgumentList.Add($case.PresetId)
    $bridgeStartInfo.ArgumentList.Add('--')
    $bridgeStartInfo.ArgumentList.Add($input)

    $bridgeProcess = [System.Diagnostics.Process]::Start($bridgeStartInfo)
    if ($null -eq $bridgeProcess) {
        throw "Converty.Bridge.exe product smoke could not start preset $($case.PresetId)."
    }
    try {
        if (-not $bridgeProcess.WaitForExit(30000)) {
            try {
                $bridgeProcess.Kill($true)
            }
            catch [System.InvalidOperationException] {
            }
            throw "Converty.Bridge.exe product smoke exceeded the 30-second process deadline for $($case.PresetId)."
        }
        $bridgeExitCode = $bridgeProcess.ExitCode
    }
    finally {
        $bridgeProcess.Dispose()
    }
    if ($bridgeExitCode -ne 0) {
        throw "Converty.Bridge.exe product smoke failed for $($case.PresetId) with exit code $bridgeExitCode."
    }

    if (-not (Test-Path -LiteralPath $input)) {
        throw "Product smoke removed or replaced the source file for $($case.PresetId)."
    }
    if ((Get-FileHash -LiteralPath $input -Algorithm SHA256).Hash -ne $sourceHash) {
        throw "Product smoke modified the source file for $($case.PresetId)."
    }
    if (-not (Test-Path -LiteralPath $expectedOutput)) {
        throw "Expected numbered conversion output was not created for $($case.PresetId): $expectedOutput"
    }
    if ((Get-Item -LiteralPath $expectedOutput).Length -le 0) {
        throw "Product smoke produced an empty conversion output for $($case.PresetId)."
    }
    if ((Get-FileHash -LiteralPath $existingOutput -Algorithm SHA256).Hash -ne $existingHash) {
        throw "Product smoke overwrote the pre-existing destination file for $($case.PresetId)."
    }

    $partialOutputs = @(Get-ChildItem -LiteralPath $smokeRoot -File | Where-Object Name -Like '.converty-*.partial.*')
    if ($partialOutputs.Count -ne 0) {
        throw "Transactional publish left $($partialOutputs.Count) temporary output file(s) behind after $($case.PresetId)."
    }

    $probeJson = (& $ffprobe -v error -select_streams 'a:0' -show_entries 'stream=codec_name,bit_rate' -of json $expectedOutput | Out-String)
    $ffprobeExitCode = $LASTEXITCODE
    if ($ffprobeExitCode -ne 0) {
        throw "ffprobe could not inspect $($case.PresetId) product output (exit code $ffprobeExitCode)."
    }
    $probe = $probeJson | ConvertFrom-Json
    $streams = @($probe.streams)
    if ($streams.Count -ne 1) {
        throw "Expected exactly one audio stream for $($case.PresetId); found $($streams.Count)."
    }
    if ([string]$streams[0].codec_name -ne $case.Codec) {
        throw "Expected codec '$($case.Codec)' for $($case.PresetId); ffprobe reported '$($streams[0].codec_name)'."
    }
    if ($null -ne $case.ExpectedBitRate -and [int64]$streams[0].bit_rate -ne [int64]$case.ExpectedBitRate) {
        throw "Expected $($case.ExpectedBitRate) bit/s for $($case.PresetId); ffprobe reported '$($streams[0].bit_rate)'."
    }

    Write-Host "Verified $($case.PresetId): $($case.Codec) -> $expectedOutput"
}

Write-Host 'Product conversion smoke: PASS'
Write-Host "Source preserved: $input"
Write-Host 'Existing destination preserved for every exercised target.'
Write-Host 'Verified audio targets: mp3 / aac-in-m4a / opus / vorbis-in-ogg'
