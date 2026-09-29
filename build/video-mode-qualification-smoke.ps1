[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$fixtureFfmpeg = Join-Path $root 'artifacts/dev-ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/video-mode-qualification-smoke'

if (-not $IsWindows) {
    throw 'The Converty packaged Video mode qualification smoke is Windows-only.'
}
foreach ($requiredPath in @($bridge, $fixtureFfmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required Task 10 dependency is missing: $requiredPath"
    }
}

if (Test-Path -LiteralPath $smokeRoot) {
    Remove-Item -LiteralPath $smokeRoot -Recurse -Force
}
New-Item -ItemType Directory -Force $smokeRoot | Out-Null

function Invoke-StructuredProcess {
    param(
        [Parameter(Mandatory)] [string] $FileName,
        [Parameter(Mandatory)] [string[]] $Arguments,
        [Parameter(Mandatory)] [string] $WorkingDirectory,
        [int] $TimeoutMilliseconds = 30000
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw "Could not start process: $FileName"
    }

    try {
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutMilliseconds)) {
            try { $process.Kill($true) } catch [System.InvalidOperationException] {}
            throw "Process exceeded the Task 10 deadline: $FileName"
        }

        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StdOut = $stdoutTask.GetAwaiter().GetResult()
            StdErr = $stderrTask.GetAwaiter().GetResult()
        }
    }
    finally {
        $process.Dispose()
    }
}

function Invoke-Bridge {
    param(
        [Parameter(Mandatory)] [string] $PresetId,
        [Parameter(Mandatory)] [string] $InputPath
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $bridge
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WorkingDirectory = $layout
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment['CONVERTY_BRIDGE_NONINTERACTIVE'] = '1'
    $startInfo.ArgumentList.Add('--preset')
    $startInfo.ArgumentList.Add($PresetId)
    $startInfo.ArgumentList.Add('--')
    $startInfo.ArgumentList.Add($InputPath)

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw 'Converty.Bridge.exe could not start for Task 10.'
    }

    try {
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(30000)) {
            try { $process.Kill($true) } catch [System.InvalidOperationException] {}
            throw "Converty.Bridge.exe exceeded the Task 10 deadline for $PresetId."
        }

        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StdOut = $stdoutTask.GetAwaiter().GetResult()
            StdErr = $stderrTask.GetAwaiter().GetResult()
        }
    }
    finally {
        $process.Dispose()
    }
}

function New-VideoFixture {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [string[]] $EncoderArgs
    )

    $arguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi', '-i', 'testsrc2=size=96x64:rate=12',
        '-f', 'lavfi', '-i', 'sine=frequency=440:sample_rate=44100',
        '-map', '0:v:0', '-map', '1:a:0',
        '-vf', 'setparams=colorspace=bt709:color_primaries=bt709:color_trc=bt709',
        '-t', '0.8', '-shortest'
    ) + $EncoderArgs + @('-y', $Path)

    $result = Invoke-StructuredProcess -FileName $fixtureFfmpeg -WorkingDirectory (Split-Path -Parent $Path) -Arguments $arguments
    if ($result.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $Path) -or (Get-Item -LiteralPath $Path).Length -le 0) {
        throw "Could not create Task 10 fixture '$Path': $($result.StdErr.Trim())"
    }
}

function Get-MediaFacts {
    param([Parameter(Mandatory)] [string] $Path)

    $result = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error',
        '-show_entries', 'format=format_name:stream=codec_type,codec_name,pix_fmt,color_space,color_transfer,color_primaries',
        '-of', 'json',
        $Path
    )
    if ($result.ExitCode -ne 0) {
        throw "ffprobe could not inspect '$Path': $($result.StdErr.Trim())"
    }

    $document = $result.StdOut | ConvertFrom-Json
    $streams = @($document.streams)
    return [pscustomobject]@{
        Format = [string]$document.format.format_name
        Video = @($streams | Where-Object codec_type -EQ 'video' | Select-Object -First 1)
        Audio = @($streams | Where-Object codec_type -EQ 'audio' | Select-Object -First 1)
    }
}

function Get-CompressedStreamHash {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [ValidateSet('v', 'a')] [string] $Kind
    )

    # REMUX_WITNESS: the pinned FFmpeg streamhash muxer hashes the compressed
    # packet payload selected with -c copy. Comparing source and published
    # output proves that Remux changed only the container, not encoded media.
    $selector = if ($Kind -eq 'v') { '0:v:0' } else { '0:a:0' }
    $result = Invoke-StructuredProcess -FileName $fixtureFfmpeg -WorkingDirectory $smokeRoot -Arguments @(
        '-hide_banner', '-loglevel', 'error',
        '-i', $Path,
        '-map', $selector,
        '-c', 'copy',
        '-f', 'streamhash',
        '-hash', 'sha256',
        '-'
    )
    if ($result.ExitCode -ne 0) {
        throw "Pinned FFmpeg streamhash failed for '$Path' stream '$Kind': $($result.StdErr.Trim())"
    }

    $match = [regex]::Match($result.StdOut, 'SHA256=([0-9A-Fa-f]{64})')
    if (-not $match.Success) {
        throw "Pinned FFmpeg streamhash returned no SHA256 digest for '$Path' stream '$Kind': $($result.StdOut.Trim())"
    }
    return $match.Groups[1].Value.ToUpperInvariant()
}

function Assert-NoPartialOutputs {
    param([Parameter(Mandatory)] [string] $Directory)
    $partials = @(Get-ChildItem -LiteralPath $Directory -File | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) {
        throw "Task 10 left $($partials.Count) partial output file(s) in $Directory."
    }
}

function Assert-NoOrphanProductProcesses {
    Start-Sleep -Milliseconds 250
    $names = @('Converty.ProbeWorker.exe', 'Converty.EngineWorker.exe', 'ffprobe.exe', 'ffmpeg.exe')
    $orphans = @(Get-CimInstance Win32_Process | Where-Object {
        $name = [string]$_.Name
        $path = [string]$_.ExecutablePath
        $name -in $names -and
        -not [string]::IsNullOrWhiteSpace($path) -and
        $path.StartsWith($layout, [StringComparison]::OrdinalIgnoreCase)
    })
    if ($orphans.Count -ne 0) {
        $details = ($orphans | ForEach-Object { "$($_.Name) pid=$($_.ProcessId) path=$($_.ExecutablePath)" }) -join '; '
        throw "Task 10 left orphan product processes: $details"
    }
}

function Assert-SentinelUnchanged {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [string] $ExpectedHash
    )
    if ((Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $ExpectedHash) {
        throw "Task 10 overwrote reserved destination '$Path'."
    }
}

$casesRoot = Join-Path $smokeRoot 'unicode Hör & mode [x]'
New-Item -ItemType Directory -Force $casesRoot | Out-Null

# COPY_WITNESS — a target-compliant MP4 must use managed byte Copy. Reserve the
# first numbered destination so publication also proves deterministic no-overwrite.
$copyDirectory = Join-Path $casesRoot 'copy semi; [x]'
New-Item -ItemType Directory -Force $copyDirectory | Out-Null
$copySource = Join-Path $copyDirectory 'Copy Hör & source [x].mp4'
New-VideoFixture -Path $copySource -EncoderArgs @(
    '-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p',
    '-c:a', 'aac', '-b:a', '96k', '-ar', '44100', '-ac', '1', '-f', 'mp4'
)
$copySourceHash = (Get-FileHash -LiteralPath $copySource -Algorithm SHA256).Hash
$copyReserved = Join-Path $copyDirectory 'Copy Hör & source [x] (1).mp4'
[System.IO.File]::WriteAllBytes($copyReserved, [byte[]](11, 22, 33, 44, 55))
$copyReservedHash = (Get-FileHash -LiteralPath $copyReserved -Algorithm SHA256).Hash
$copyOutput = Join-Path $copyDirectory 'Copy Hör & source [x] (2).mp4'
$copyResult = Invoke-Bridge -PresetId 'video.mp4.h264' -InputPath $copySource
if ($copyResult.ExitCode -ne 0) {
    throw "COPY_WITNESS Bridge failure: $($copyResult.StdErr.Trim())"
}
if (-not (Test-Path -LiteralPath $copyOutput)) {
    throw "COPY_WITNESS did not publish expected numbered output '$copyOutput'."
}
if ((Get-FileHash -LiteralPath $copySource -Algorithm SHA256).Hash -ne $copySourceHash) {
    throw 'COPY_WITNESS modified source bytes.'
}
if ((Get-FileHash -LiteralPath $copyOutput -Algorithm SHA256).Hash -ne $copySourceHash) {
    throw 'COPY_WITNESS output is not byte-identical to the compliant MP4 source.'
}
Assert-SentinelUnchanged -Path $copyReserved -ExpectedHash $copyReservedHash
Assert-NoPartialOutputs -Directory $copyDirectory
Write-Host "COPY_WITNESS PASS sha256=$copySourceHash"

# REMUX_WITNESS — compatible H.264/AAC media starts in Matroska and must publish
# MP4 while preserving compressed video and audio packet hashes.
$remuxDirectory = Join-Path $casesRoot 'remux semi; [x]'
New-Item -ItemType Directory -Force $remuxDirectory | Out-Null
$remuxSource = Join-Path $remuxDirectory 'Remux Hör & source [x].mkv'
New-VideoFixture -Path $remuxSource -EncoderArgs @(
    '-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p',
    '-c:a', 'aac', '-b:a', '96k', '-ar', '44100', '-ac', '1', '-f', 'matroska'
)
$remuxSourceHash = (Get-FileHash -LiteralPath $remuxSource -Algorithm SHA256).Hash
$remuxVideoHash = Get-CompressedStreamHash -Path $remuxSource -Kind 'v'
$remuxAudioHash = Get-CompressedStreamHash -Path $remuxSource -Kind 'a'
$remuxReserved = Join-Path $remuxDirectory 'Remux Hör & source [x].mp4'
[System.IO.File]::WriteAllBytes($remuxReserved, [byte[]](66, 77, 88, 99))
$remuxReservedHash = (Get-FileHash -LiteralPath $remuxReserved -Algorithm SHA256).Hash
$remuxOutput = Join-Path $remuxDirectory 'Remux Hör & source [x] (1).mp4'
$remuxResult = Invoke-Bridge -PresetId 'video.mp4.h264' -InputPath $remuxSource
if ($remuxResult.ExitCode -ne 0) {
    throw "REMUX_WITNESS Bridge failure: $($remuxResult.StdErr.Trim())"
}
if (-not (Test-Path -LiteralPath $remuxOutput)) {
    throw "REMUX_WITNESS did not publish expected MP4 '$remuxOutput'."
}
if ((Get-FileHash -LiteralPath $remuxSource -Algorithm SHA256).Hash -ne $remuxSourceHash) {
    throw 'REMUX_WITNESS modified source bytes.'
}
Assert-SentinelUnchanged -Path $remuxReserved -ExpectedHash $remuxReservedHash
$remuxFacts = Get-MediaFacts -Path $remuxOutput
if ($remuxFacts.Format -notmatch '(^|,)mov(,|$)|mp4|m4a|3gp|3g2|mj2') {
    throw "REMUX_WITNESS output is not an MP4-family container: $($remuxFacts.Format)"
}
if ($remuxFacts.Video.Count -ne 1 -or [string]$remuxFacts.Video[0].codec_name -ne 'h264') {
    throw 'REMUX_WITNESS output video codec is not H.264.'
}
if ($remuxFacts.Audio.Count -ne 1 -or [string]$remuxFacts.Audio[0].codec_name -ne 'aac') {
    throw 'REMUX_WITNESS output audio codec is not AAC.'
}
$remuxOutputVideoHash = Get-CompressedStreamHash -Path $remuxOutput -Kind 'v'
$remuxOutputAudioHash = Get-CompressedStreamHash -Path $remuxOutput -Kind 'a'
if ($remuxOutputVideoHash -ne $remuxVideoHash -or $remuxOutputAudioHash -ne $remuxAudioHash) {
    throw "REMUX_WITNESS compressed packet hash changed. video=$remuxVideoHash->$remuxOutputVideoHash audio=$remuxAudioHash->$remuxOutputAudioHash"
}
if ((Get-FileHash -LiteralPath $remuxOutput -Algorithm SHA256).Hash -eq $remuxSourceHash) {
    throw 'REMUX_WITNESS container output unexpectedly remained byte-identical to Matroska source.'
}
Assert-NoPartialOutputs -Directory $remuxDirectory
Write-Host "REMUX_WITNESS PASS video=$remuxVideoHash audio=$remuxAudioHash"

# TRANSCODE_WITNESS — qualified MPEG-2/MP3 input cannot satisfy the MP4 target
# without decode/encode and must emerge as H.264/AAC.
$transcodeDirectory = Join-Path $casesRoot 'transcode semi; [x]'
New-Item -ItemType Directory -Force $transcodeDirectory | Out-Null
$transcodeSource = Join-Path $transcodeDirectory 'Transcode Hör & source [x].avi'
New-VideoFixture -Path $transcodeSource -EncoderArgs @(
    '-c:v', 'mpeg2video', '-q:v', '5', '-pix_fmt', 'yuv420p',
    '-c:a', 'libmp3lame', '-b:a', '96k', '-ar', '44100', '-ac', '1', '-f', 'avi'
)
$transcodeSourceFacts = Get-MediaFacts -Path $transcodeSource
if ($transcodeSourceFacts.Video.Count -ne 1 -or [string]$transcodeSourceFacts.Video[0].codec_name -ne 'mpeg2video') {
    throw 'TRANSCODE_WITNESS source fixture is not MPEG-2 video.'
}
if ($transcodeSourceFacts.Audio.Count -ne 1 -or [string]$transcodeSourceFacts.Audio[0].codec_name -ne 'mp3') {
    throw 'TRANSCODE_WITNESS source fixture is not MP3 audio.'
}
$transcodeSourceHash = (Get-FileHash -LiteralPath $transcodeSource -Algorithm SHA256).Hash
$transcodeReserved = Join-Path $transcodeDirectory 'Transcode Hör & source [x].mp4'
[System.IO.File]::WriteAllBytes($transcodeReserved, [byte[]](101, 102, 103, 104))
$transcodeReservedHash = (Get-FileHash -LiteralPath $transcodeReserved -Algorithm SHA256).Hash
$transcodeOutput = Join-Path $transcodeDirectory 'Transcode Hör & source [x] (1).mp4'
$transcodeResult = Invoke-Bridge -PresetId 'video.mp4.h264' -InputPath $transcodeSource
if ($transcodeResult.ExitCode -ne 0) {
    throw "TRANSCODE_WITNESS Bridge failure: $($transcodeResult.StdErr.Trim())"
}
if (-not (Test-Path -LiteralPath $transcodeOutput)) {
    throw "TRANSCODE_WITNESS did not publish expected MP4 '$transcodeOutput'."
}
if ((Get-FileHash -LiteralPath $transcodeSource -Algorithm SHA256).Hash -ne $transcodeSourceHash) {
    throw 'TRANSCODE_WITNESS modified source bytes.'
}
Assert-SentinelUnchanged -Path $transcodeReserved -ExpectedHash $transcodeReservedHash
$transcodeOutputFacts = Get-MediaFacts -Path $transcodeOutput
if ($transcodeOutputFacts.Video.Count -ne 1 -or [string]$transcodeOutputFacts.Video[0].codec_name -ne 'h264') {
    throw 'TRANSCODE_WITNESS output video codec is not H.264.'
}
if ($transcodeOutputFacts.Audio.Count -ne 1 -or [string]$transcodeOutputFacts.Audio[0].codec_name -ne 'aac') {
    throw 'TRANSCODE_WITNESS output audio codec is not AAC.'
}
if ([string]$transcodeOutputFacts.Video[0].pix_fmt -ne 'yuv420p') {
    throw "TRANSCODE_WITNESS output pixel format is not yuv420p: $($transcodeOutputFacts.Video[0].pix_fmt)"
}
Assert-NoPartialOutputs -Directory $transcodeDirectory
Write-Host 'TRANSCODE_WITNESS PASS source=mpeg2video+mp3 output=h264+aac'

Assert-NoOrphanProductProcesses
Write-Host 'Video Copy/Remux/Transcode packaged mode qualification: PASS'
