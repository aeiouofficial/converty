[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/audio-input-acceptance-smoke'

if (-not $IsWindows) {
    throw 'The Converty Audio input acceptance smoke is Windows-only.'
}
foreach ($requiredPath in @($bridge, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required dev.16 acceptance dependency is missing: $requiredPath"
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
            throw "Process exceeded the 30-second acceptance deadline: $FileName"
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
        [Parameter(Mandatory)] [string] $presetId,
        [Parameter(Mandatory)] [string] $inputPath
    )

    # Keep the Explorer-to-Bridge contract structurally identical to production:
    # a fixed executable plus typed preset ID and literal filesystem path arguments.
    $bridgeStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $bridgeStartInfo.FileName = $bridge
    $bridgeStartInfo.UseShellExecute = $false
    $bridgeStartInfo.CreateNoWindow = $true
    $bridgeStartInfo.WorkingDirectory = $layout
    $bridgeStartInfo.RedirectStandardOutput = $true
    $bridgeStartInfo.RedirectStandardError = $true
    # Explicit automation-only opt-in: Explorer never sets this and retains modal errors.
    $bridgeStartInfo.Environment['CONVERTY_BRIDGE_NONINTERACTIVE'] = '1'
    $bridgeStartInfo.ArgumentList.Add('--preset')
    $bridgeStartInfo.ArgumentList.Add($presetId)
    $bridgeStartInfo.ArgumentList.Add('--')
    $bridgeStartInfo.ArgumentList.Add($inputPath)

    $bridgeProcess = [System.Diagnostics.Process]::Start($bridgeStartInfo)
    if ($null -eq $bridgeProcess) {
        throw "Converty.Bridge.exe could not start preset $presetId."
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
            throw "Converty.Bridge.exe exceeded the 30-second acceptance deadline for $presetId."
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
        throw "Acceptance case left $($partials.Count) partial output file(s) behind in $Directory."
    }
}

function Get-AudioCodecName {
    param([Parameter(Mandatory)] [string] $Path)

    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error',
        '-select_streams', 'a:0',
        '-show_entries', 'stream=codec_name',
        '-of', 'default=noprint_wrappers=1:nokey=1',
        $Path
    )
    if ($probe.ExitCode -ne 0) {
        throw "ffprobe could not inspect acceptance output '$Path': $($probe.StdErr.Trim())"
    }

    return $probe.StdOut.Trim()
}

$fixturesRoot = Join-Path $smokeRoot 'fixtures'
New-Item -ItemType Directory -Force $fixturesRoot | Out-Null

# These are deterministic test fixtures only. The conversion under test always enters
# through Bridge -> Strict Worker/provider -> the staged app-local FFmpeg executable.
$sourceFixtures = @(
    [pscustomobject]@{ Name = 'wav'; Extension = '.wav'; EncoderArgs = @('-c:a', 'pcm_s16le') },
    [pscustomobject]@{ Name = 'flac'; Extension = '.flac'; EncoderArgs = @('-c:a', 'flac') },
    [pscustomobject]@{ Name = 'mp3'; Extension = '.mp3'; EncoderArgs = @('-c:a', 'libmp3lame', '-b:a', '192k') },
    [pscustomobject]@{ Name = 'm4a'; Extension = '.m4a'; EncoderArgs = @('-c:a', 'aac', '-b:a', '128k') },
    [pscustomobject]@{ Name = 'ogg'; Extension = '.ogg'; EncoderArgs = @('-c:a', 'libvorbis', '-q:a', '4') },
    [pscustomobject]@{ Name = 'opus'; Extension = '.opus'; EncoderArgs = @('-c:a', 'libopus', '-b:a', '96k') }
)

foreach ($fixture in $sourceFixtures) {
    $fixturePath = Join-Path $fixturesRoot ("source-$($fixture.Name)$($fixture.Extension)")
    $arguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi',
        '-i', 'sine=frequency=440:sample_rate=44100',
        '-t', '0.25'
    ) + $fixture.EncoderArgs + @('-y', $fixturePath)

    $result = Invoke-StructuredProcess -FileName $ffmpeg -WorkingDirectory $fixturesRoot -Arguments $arguments
    if ($result.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $fixturePath)) {
        throw "Could not create $($fixture.Name) acceptance fixture: $($result.StdErr.Trim())"
    }
    Add-Member -InputObject $fixture -NotePropertyName Path -NotePropertyValue $fixturePath
}

$targets = @(
    [pscustomobject]@{ PresetId = 'audio.mp3'; Extension = '.mp3'; Codec = 'mp3' },
    [pscustomobject]@{ PresetId = 'audio.flac'; Extension = '.flac'; Codec = 'flac' },
    [pscustomobject]@{ PresetId = 'audio.m4a.aac'; Extension = '.m4a'; Codec = 'aac' },
    [pscustomobject]@{ PresetId = 'audio.opus'; Extension = '.opus'; Codec = 'opus' },
    [pscustomobject]@{ PresetId = 'audio.ogg.vorbis'; Extension = '.ogg'; Codec = 'vorbis' },
    [pscustomobject]@{ PresetId = 'audio.wav'; Extension = '.wav'; Codec = 'pcm_s16le' }
)

$successRoot = Join-Path $smokeRoot 'success'
$successCount = 0
foreach ($fixture in $sourceFixtures) {
    foreach ($target in $targets) {
        $caseDirectory = Join-Path $successRoot ("$($fixture.Name)-to-$($target.PresetId)")
        New-Item -ItemType Directory -Force $caseDirectory | Out-Null

        $inputPath = Join-Path $caseDirectory ("Hör source & semi; -dash [x]$($fixture.Extension)")
        Copy-Item -LiteralPath $fixture.Path -Destination $inputPath
        $sourceHash = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash

        $baseOutput = [System.IO.Path]::ChangeExtension($inputPath, $target.Extension)
        if (-not [string]::Equals($baseOutput, $inputPath, [StringComparison]::OrdinalIgnoreCase)) {
            # Reserve the base name to prove the pre-existing destination is never overwritten.
            [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](17, 34, 51, 68))
        }
        $baseOutputHash = (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash

        $stem = [System.IO.Path]::GetFileNameWithoutExtension($inputPath)
        $expectedOutput = Join-Path $caseDirectory ($stem + ' (1)' + $target.Extension)
        $result = Invoke-Bridge -presetId $target.PresetId -inputPath $inputPath
        if ($result.ExitCode -ne 0) {
            throw "Expected success for $($fixture.Name) -> $($target.PresetId), got exit $($result.ExitCode): $($result.StdErr.Trim())"
        }

        if ((Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash -ne $sourceHash) {
            throw "Successful acceptance modified source bytes for $($fixture.Name) -> $($target.PresetId)."
        }
        if ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $baseOutputHash) {
            throw "Successful acceptance overwrote the pre-existing destination for $($fixture.Name) -> $($target.PresetId)."
        }
        if (-not (Test-Path -LiteralPath $expectedOutput) -or (Get-Item -LiteralPath $expectedOutput).Length -le 0) {
            throw "Successful acceptance did not publish the numbered output for $($fixture.Name) -> $($target.PresetId)."
        }
        Assert-NoPartialOutputs -Directory $caseDirectory

        $codec = Get-AudioCodecName -Path $expectedOutput
        if ($codec -ne $target.Codec) {
            throw "Expected codec '$($target.Codec)' for $($fixture.Name) -> $($target.PresetId); ffprobe reported '$codec'."
        }

        $successCount++
        Write-Host "Accepted $($fixture.Name) -> $($target.PresetId): $codec"
    }
}

$negativeRoot = Join-Path $smokeRoot 'negative'
New-Item -ItemType Directory -Force $negativeRoot | Out-Null

$malformedPath = Join-Path $negativeRoot 'malformed Hör & [x].wav'
[System.IO.File]::WriteAllBytes($malformedPath, [System.Text.Encoding]::UTF8.GetBytes('not a valid wave payload'))

$validFlac = ($sourceFixtures | Where-Object Name -EQ 'flac').Path
$validFlacBytes = [System.IO.File]::ReadAllBytes($validFlac)
$truncatedPath = Join-Path $negativeRoot 'truncated Hör & [x].flac'
$truncatedLength = [Math]::Min(16, $validFlacBytes.Length)
[System.IO.File]::WriteAllBytes($truncatedPath, $validFlacBytes[0..($truncatedLength - 1)])

$negativeCases = @(
    [pscustomobject]@{ Name = 'malformed'; InputPath = $malformedPath; PresetId = 'audio.mp3'; Extension = '.mp3' },
    [pscustomobject]@{ Name = 'truncated'; InputPath = $truncatedPath; PresetId = 'audio.wav'; Extension = '.wav' }
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
        $result = Invoke-Bridge -presetId $negative.PresetId -inputPath $negative.InputPath
        $bridgeExitCode = $result.ExitCode
        if ($bridgeExitCode -eq 0) {
            throw "Expected failure for $($negative.Name) input, but Bridge returned success on attempt $attempt."
        }
        $observedExitCodes += $bridgeExitCode

        if ((Get-FileHash -LiteralPath $negative.InputPath -Algorithm SHA256).Hash -ne $inputHash) {
            throw "Expected failure changed source bytes for $($negative.Name) input."
        }
        if ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $destinationHash) {
            throw "Expected failure overwrote the pre-existing destination for $($negative.Name) input."
        }
        if (Test-Path -LiteralPath $numberedOutput) {
            throw "Expected failure published an output for $($negative.Name) input."
        }
        Assert-NoPartialOutputs -Directory $negativeRoot
    }

    if (@($observedExitCodes | Select-Object -Unique).Count -ne 1) {
        throw "Expected deterministic failure exit code for $($negative.Name); observed: $($observedExitCodes -join ', ')."
    }
    Write-Host "Rejected $($negative.Name) input deterministically with exit code $($observedExitCodes[0])."
}

Write-Host "Audio source and malformed-input acceptance: PASS ($successCount successful conversions; $($negativeCases.Count) repeated negative cases)."
Write-Host 'All source files and pre-existing destinations were preserved; no partial outputs remained.'
