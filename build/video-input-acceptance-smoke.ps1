[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$packagedFfmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$fixtureFfmpeg = Join-Path $root 'artifacts/dev-ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/video-input-acceptance-smoke'

if (-not $IsWindows) {
    throw 'The Converty Video input acceptance smoke is Windows-only.'
}
foreach ($requiredPath in @($bridge, $packagedFfmpeg, $fixtureFfmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required dev.20 Video acceptance dependency is missing: $requiredPath"
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
        [Parameter(Mandatory)] [string] $WorkingDirectory
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
        if (-not $process.WaitForExit(30000)) {
            try {
                $process.Kill($true)
            }
            catch [System.InvalidOperationException] {
            }
            throw "Process exceeded the 30-second Video acceptance deadline: $FileName"
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

    $bridgeStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $bridgeStartInfo.FileName = $bridge
    $bridgeStartInfo.UseShellExecute = $false
    $bridgeStartInfo.CreateNoWindow = $true
    $bridgeStartInfo.WorkingDirectory = $layout
    $bridgeStartInfo.RedirectStandardOutput = $true
    $bridgeStartInfo.RedirectStandardError = $true
    $bridgeStartInfo.Environment['CONVERTY_BRIDGE_NONINTERACTIVE'] = '1'
    $bridgeStartInfo.ArgumentList.Add('--preset')
    $bridgeStartInfo.ArgumentList.Add($PresetId)
    $bridgeStartInfo.ArgumentList.Add('--')
    $bridgeStartInfo.ArgumentList.Add($InputPath)

    $bridgeProcess = [System.Diagnostics.Process]::Start($bridgeStartInfo)
    if ($null -eq $bridgeProcess) {
        throw "Converty.Bridge.exe could not start preset $PresetId."
    }

    try {
        $stdoutTask = $bridgeProcess.StandardOutput.ReadToEndAsync()
        $stderrTask = $bridgeProcess.StandardError.ReadToEndAsync()
        if (-not $bridgeProcess.WaitForExit(30000)) {
            try {
                $bridgeProcess.Kill($true)
            }
            catch [System.InvalidOperationException] {
            }
            throw "Converty.Bridge.exe exceeded the 30-second Video acceptance deadline for $PresetId."
        }

        return [pscustomobject]@{
            ExitCode = $bridgeProcess.ExitCode
            StdOut = $stdoutTask.GetAwaiter().GetResult()
            StdErr = $stderrTask.GetAwaiter().GetResult()
        }
    }
    finally {
        $bridgeProcess.Dispose()
    }
}

function Assert-NoPartialOutputs {
    param([Parameter(Mandatory)] [string] $Directory)

    $partials = @(Get-ChildItem -LiteralPath $Directory -File | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) {
        throw "Video acceptance case left $($partials.Count) partial output file(s) behind in $Directory."
    }
}

function Assert-NoOrphanConverterProcesses {
    $orphans = @(Get-CimInstance Win32_Process | Where-Object {
        $name = [string]$_.Name
        $executablePath = [string]$_.ExecutablePath
        ($name -ieq 'Converty.EngineWorker.exe' -or $name -ieq 'ffmpeg.exe') -and
        -not [string]::IsNullOrWhiteSpace($executablePath) -and
        $executablePath.StartsWith($layout, [StringComparison]::OrdinalIgnoreCase)
    })
    if ($orphans.Count -ne 0) {
        $details = ($orphans | ForEach-Object { "$($_.Name) pid=$($_.ProcessId) path=$($_.ExecutablePath)" }) -join '; '
        throw "Video acceptance left orphan converter processes: $details"
    }
}

function Get-StreamCodecNames {
    param([Parameter(Mandatory)] [string] $Path)

    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error',
        '-show_entries', 'stream=codec_type,codec_name',
        '-of', 'json',
        $Path
    )
    if ($probe.ExitCode -ne 0) {
        throw "ffprobe could not inspect Video acceptance output '$Path': $($probe.StdErr.Trim())"
    }

    $document = $probe.StdOut | ConvertFrom-Json
    $streams = @($document.streams)
    return [pscustomobject]@{
        Video = @($streams | Where-Object codec_type -EQ 'video' | ForEach-Object { [string]$_.codec_name })
        Audio = @($streams | Where-Object codec_type -EQ 'audio' | ForEach-Object { [string]$_.codec_name })
    }
}

$fixturesRoot = Join-Path $smokeRoot 'fixtures'
New-Item -ItemType Directory -Force $fixturesRoot | Out-Null

# Deterministic development fixtures only. Fixture generation uses the pinned development
# FFmpeg; every conversion under test enters through packaged Bridge -> Strict Worker/provider
# -> the fixed staged app-local FFmpeg executable.
$sourceFixtures = @(
    [pscustomobject]@{ Name = 'mp4'; Extension = '.mp4'; EncoderArgs = @('-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '96k', '-f', 'mp4') },
    [pscustomobject]@{ Name = 'mov'; Extension = '.mov'; EncoderArgs = @('-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '96k', '-f', 'mov') },
    [pscustomobject]@{ Name = 'mkv'; Extension = '.mkv'; EncoderArgs = @('-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '96k', '-f', 'matroska') },
    [pscustomobject]@{ Name = 'avi'; Extension = '.avi'; EncoderArgs = @('-c:v', 'mpeg4', '-q:v', '5', '-pix_fmt', 'yuv420p', '-c:a', 'libmp3lame', '-b:a', '96k', '-f', 'avi') },
    [pscustomobject]@{ Name = 'webm'; Extension = '.webm'; EncoderArgs = @('-c:v', 'libvpx-vp9', '-crf', '36', '-b:v', '0', '-pix_fmt', 'yuv420p', '-c:a', 'libopus', '-b:a', '64k', '-f', 'webm') },
    [pscustomobject]@{ Name = 'm4v'; Extension = '.m4v'; EncoderArgs = @('-c:v', 'libx264', '-preset', 'ultrafast', '-crf', '30', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '96k', '-f', 'mp4') },
    [pscustomobject]@{ Name = 'mpeg'; Extension = '.mpeg'; EncoderArgs = @('-c:v', 'mpeg2video', '-q:v', '5', '-pix_fmt', 'yuv420p', '-c:a', 'mp2', '-b:a', '128k', '-f', 'mpeg') },
    [pscustomobject]@{ Name = 'mpg'; Extension = '.mpg'; EncoderArgs = @('-c:v', 'mpeg2video', '-q:v', '5', '-pix_fmt', 'yuv420p', '-c:a', 'mp2', '-b:a', '128k', '-f', 'mpeg') },
    [pscustomobject]@{ Name = 'wmv'; Extension = '.wmv'; EncoderArgs = @('-c:v', 'wmv2', '-b:v', '256k', '-pix_fmt', 'yuv420p', '-c:a', 'wmav2', '-b:a', '96k', '-f', 'asf') }
)

foreach ($fixture in $sourceFixtures) {
    $fixturePath = Join-Path $fixturesRoot ("source-$($fixture.Name)$($fixture.Extension)")
    $arguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi', '-i', 'testsrc2=size=64x48:rate=10',
        '-f', 'lavfi', '-i', 'sine=frequency=440:sample_rate=44100',
        '-map', '0:v:0', '-map', '1:a:0',
        '-t', '0.5', '-shortest'
    ) + $fixture.EncoderArgs + @('-y', $fixturePath)

    $result = Invoke-StructuredProcess -FileName $fixtureFfmpeg -WorkingDirectory $fixturesRoot -Arguments $arguments
    if ($result.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $fixturePath) -or (Get-Item -LiteralPath $fixturePath).Length -le 0) {
        throw "Could not create native $($fixture.Name) Video acceptance fixture; advertised product-surface evidence failed: $($result.StdErr.Trim())"
    }
    Add-Member -InputObject $fixture -NotePropertyName Path -NotePropertyValue $fixturePath
}

$targets = @(
    [pscustomobject]@{ PresetId = 'video.mp4.h264'; Extension = '.mp4'; VideoCodec = 'h264'; AudioCodec = 'aac'; ExpectVideo = $true },
    [pscustomobject]@{ PresetId = 'video.webm.vp9'; Extension = '.webm'; VideoCodec = 'vp9'; AudioCodec = 'opus'; ExpectVideo = $true },
    [pscustomobject]@{ PresetId = 'extract.audio.mp3'; Extension = '.mp3'; VideoCodec = $null; AudioCodec = 'mp3'; ExpectVideo = $false }
)

$successRoot = Join-Path $smokeRoot 'success'
$successCount = 0
foreach ($fixture in $sourceFixtures) {
    foreach ($target in $targets) {
        $caseDirectory = Join-Path $successRoot ("$($fixture.Name)-to-$($target.PresetId)")
        New-Item -ItemType Directory -Force $caseDirectory | Out-Null

        $inputPath = Join-Path $caseDirectory ("Hör clip & semi; -dash [x]$($fixture.Extension)")
        Copy-Item -LiteralPath $fixture.Path -Destination $inputPath
        $sourceHash = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash

        $baseOutput = [System.IO.Path]::ChangeExtension($inputPath, $target.Extension)
        $baseOutputHash = $null
        if (-not [string]::Equals($baseOutput, $inputPath, [StringComparison]::OrdinalIgnoreCase)) {
            # Reserve the base name to prove the pre-existing destination is never overwritten.
            [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](17, 34, 51, 68))
            $baseOutputHash = (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash
        }

        $stem = [System.IO.Path]::GetFileNameWithoutExtension($inputPath)
        $expectedOutput = Join-Path $caseDirectory ($stem + ' (1)' + $target.Extension)
        $result = Invoke-Bridge -PresetId $target.PresetId -InputPath $inputPath
        if ($result.ExitCode -ne 0) {
            throw "Expected Video success for $($fixture.Name) -> $($target.PresetId), got exit $($result.ExitCode): $($result.StdErr.Trim())"
        }

        if ((Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash -ne $sourceHash) {
            throw "Successful Video acceptance modified source bytes for $($fixture.Name) -> $($target.PresetId)."
        }
        if ($null -ne $baseOutputHash -and (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $baseOutputHash) {
            throw "Successful Video acceptance overwrote the pre-existing destination for $($fixture.Name) -> $($target.PresetId)."
        }
        if (-not (Test-Path -LiteralPath $expectedOutput) -or (Get-Item -LiteralPath $expectedOutput).Length -le 0) {
            throw "Successful Video acceptance did not publish the numbered output for $($fixture.Name) -> $($target.PresetId)."
        }
        Assert-NoPartialOutputs -Directory $caseDirectory

        $codecs = Get-StreamCodecNames -Path $expectedOutput
        if ($target.ExpectVideo) {
            if ($codecs.Video.Count -ne 1 -or $codecs.Video[0] -ne $target.VideoCodec) {
                throw "Expected Video codec '$($target.VideoCodec)' for $($fixture.Name) -> $($target.PresetId); ffprobe reported '$($codecs.Video -join ',')'."
            }
        }
        elseif ($codecs.Video.Count -ne 0) {
            throw "Expected no Video stream for $($fixture.Name) -> $($target.PresetId); ffprobe reported '$($codecs.Video -join ',')'."
        }
        if ($codecs.Audio.Count -ne 1 -or $codecs.Audio[0] -ne $target.AudioCodec) {
            throw "Expected Audio codec '$($target.AudioCodec)' for $($fixture.Name) -> $($target.PresetId); ffprobe reported '$($codecs.Audio -join ',')'."
        }

        $successCount++
        Write-Host "Accepted $($fixture.Name) -> $($target.PresetId): video=$($codecs.Video -join ',') audio=$($codecs.Audio -join ',')"
    }
}

if ($successCount -ne 27) {
    throw "Expected exactly 27 packaged Video source/action conversions, observed $successCount."
}

$negativeRoot = Join-Path $smokeRoot 'negative'
New-Item -ItemType Directory -Force $negativeRoot | Out-Null

$malformedPath = Join-Path $negativeRoot 'malformed Hör & [x].avi'
[System.IO.File]::WriteAllBytes($malformedPath, [System.Text.Encoding]::UTF8.GetBytes('not a valid avi payload'))

$validMkv = ($sourceFixtures | Where-Object Name -EQ 'mkv').Path
$validMkvBytes = [System.IO.File]::ReadAllBytes($validMkv)
if ($validMkvBytes.Length -lt 16) {
    throw 'Valid MKV fixture is unexpectedly too small to create the physically truncated Video case.'
}
$truncatedPath = Join-Path $negativeRoot 'truncated Hör & [x].mkv'
[System.IO.File]::WriteAllBytes($truncatedPath, $validMkvBytes[0..15])

$negativeCases = @(
    [pscustomobject]@{ Name = 'malformed'; InputPath = $malformedPath; PresetId = 'video.mp4.h264'; Extension = '.mp4' },
    [pscustomobject]@{ Name = 'truncated'; InputPath = $truncatedPath; PresetId = 'video.webm.vp9'; Extension = '.webm' }
)

foreach ($negative in $negativeCases) {
    $inputHash = (Get-FileHash -LiteralPath $negative.InputPath -Algorithm SHA256).Hash
    $baseOutput = [System.IO.Path]::ChangeExtension($negative.InputPath, $negative.Extension)
    [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](85, 102, 119, 136))
    $destinationHash = (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($negative.InputPath)
    $numberedOutput = Join-Path $negativeRoot ($stem + ' (1)' + $negative.Extension)
    $observedExitCodes = @()

    foreach ($attempt in 1..2) {
        $result = Invoke-Bridge -PresetId $negative.PresetId -InputPath $negative.InputPath
        $bridgeExitCode = $result.ExitCode
        if ($bridgeExitCode -eq 0) {
            throw "Expected failure for $($negative.Name) Video input, but Bridge returned success on attempt $attempt."
        }
        $observedExitCodes += $bridgeExitCode

        if ((Get-FileHash -LiteralPath $negative.InputPath -Algorithm SHA256).Hash -ne $inputHash) {
            throw "Expected failure changed source bytes for $($negative.Name) Video input."
        }
        if ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $destinationHash) {
            throw "Expected failure overwrote the pre-existing destination for $($negative.Name) Video input."
        }
        if (Test-Path -LiteralPath $numberedOutput) {
            throw "Expected failure published a numbered output for $($negative.Name) Video input."
        }
        Assert-NoPartialOutputs -Directory $negativeRoot
        Assert-NoOrphanConverterProcesses
    }

    if (@($observedExitCodes | Select-Object -Unique).Count -ne 1) {
        throw "Expected deterministic Video failure exit code for $($negative.Name); observed: $($observedExitCodes -join ', ')."
    }
    Write-Host "Rejected $($negative.Name) Video input deterministically with exit code $($observedExitCodes[0])."
}

Assert-NoOrphanConverterProcesses
Write-Host "Video source and malformed-input acceptance: PASS ($successCount successful conversions; $($negativeCases.Count) repeated negative cases)."
Write-Host 'All Video source files and pre-existing destinations were preserved; numbered publication and partial cleanup passed.'
