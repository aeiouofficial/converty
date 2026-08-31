[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/audio-batch-isolation-smoke'

if (-not $IsWindows) {
    throw 'The Converty Audio mixed-batch failure-isolation smoke is Windows-only.'
}
foreach ($requiredPath in @($bridge, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required dev.17 batch-isolation dependency is missing: $requiredPath"
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
            try { $process.Kill($true) } catch [System.InvalidOperationException] { }
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

function Invoke-MixedBridgeBatch {
    param([Parameter(Mandatory)] [object[]] $Sources)

    # One Bridge process mirrors one same-family Explorer multi-selection. Each path remains
    # a literal ArgumentList item; automation only suppresses modal UI so failure is bounded.
    $bridgeStartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $bridgeStartInfo.FileName = $bridge
    $bridgeStartInfo.UseShellExecute = $false
    $bridgeStartInfo.CreateNoWindow = $true
    $bridgeStartInfo.WorkingDirectory = $layout
    $bridgeStartInfo.RedirectStandardOutput = $true
    $bridgeStartInfo.RedirectStandardError = $true
    $bridgeStartInfo.Environment['CONVERTY_BRIDGE_NONINTERACTIVE'] = '1'
    $bridgeStartInfo.ArgumentList.Add('--preset')
    $bridgeStartInfo.ArgumentList.Add('audio.mp3')
    $bridgeStartInfo.ArgumentList.Add('--')
    foreach ($source in $Sources) {
        $bridgeStartInfo.ArgumentList.Add($source.Path)
    }

    $bridgeProcess = [System.Diagnostics.Process]::Start($bridgeStartInfo)
    if ($null -eq $bridgeProcess) {
        throw 'Converty.Bridge.exe could not start the mixed batch.'
    }

    try {
        $stdoutTask = $bridgeProcess.StandardOutput.ReadToEndAsync()
        $stderrTask = $bridgeProcess.StandardError.ReadToEndAsync()
        if (-not $bridgeProcess.WaitForExit(30000)) {
            try { $bridgeProcess.Kill($true) } catch [System.InvalidOperationException] { }
            throw 'Converty.Bridge.exe exceeded the 30-second mixed-batch deadline.'
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
    $partials = @(Get-ChildItem -LiteralPath $smokeRoot -File -Recurse | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) {
        throw "Mixed batch left $($partials.Count) .converty-*.partial.* file(s) behind."
    }
}

function Assert-Mp3Codec {
    param([Parameter(Mandatory)] [string] $Path)

    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error',
        '-select_streams', 'a:0',
        '-show_entries', 'stream=codec_name',
        '-of', 'default=noprint_wrappers=1:nokey=1',
        $Path
    )
    if ($probe.ExitCode -ne 0 -or $probe.StdOut.Trim() -ne 'mp3') {
        throw "Expected MP3 output at '$Path', ffprobe returned '$($probe.StdOut.Trim())': $($probe.StdErr.Trim())"
    }
}

# FFmpeg is used here only to create deterministic fixtures. The conversion under test is
# always packaged Bridge -> Strict Worker/provider -> fixed app-local FFmpeg.
$validWavFixture = Join-Path $smokeRoot 'fixture-valid.wav'
$validFlacFixture = Join-Path $smokeRoot 'fixture-valid.flac'
foreach ($fixture in @(
    [pscustomobject]@{ Path = $validWavFixture; Encoder = @('-c:a', 'pcm_s16le') },
    [pscustomobject]@{ Path = $validFlacFixture; Encoder = @('-c:a', 'flac') }
)) {
    $fixtureArguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi', '-i', 'sine=frequency=523.25:sample_rate=44100',
        '-t', '0.25'
    ) + $fixture.Encoder + @('-y', $fixture.Path)
    $fixtureResult = Invoke-StructuredProcess -FileName $ffmpeg -WorkingDirectory $smokeRoot -Arguments $fixtureArguments
    if ($fixtureResult.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $fixture.Path)) {
        throw "Could not create mixed-batch fixture '$($fixture.Path)': $($fixtureResult.StdErr.Trim())"
    }
}

$caseRoot = Join-Path $smokeRoot 'mixed-batch'
New-Item -ItemType Directory -Force $caseRoot | Out-Null

$validBefore = Join-Path $caseRoot '01 valid-before Hör & [x].wav'
$malformed = Join-Path $caseRoot '02 malformed Hör & [x].wav'
$validAfter = Join-Path $caseRoot '03 valid-after Hör & [x].flac'
$truncated = Join-Path $caseRoot '04 truncated Hör & [x].flac'
$validAfterSecond = Join-Path $caseRoot '05 valid-after-second Hör & [x].wav'

Copy-Item -LiteralPath $validWavFixture -Destination $validBefore
[System.IO.File]::WriteAllBytes($malformed, [System.Text.Encoding]::UTF8.GetBytes('malformed audio payload for batch isolation'))
Copy-Item -LiteralPath $validFlacFixture -Destination $validAfter
$flacBytes = [System.IO.File]::ReadAllBytes($validFlacFixture)
$truncatedLength = [Math]::Min(16, $flacBytes.Length)
[System.IO.File]::WriteAllBytes($truncated, $flacBytes[0..($truncatedLength - 1)])
Copy-Item -LiteralPath $validWavFixture -Destination $validAfterSecond

$sources = @(
    [pscustomobject]@{ Role = 'valid-before'; Path = $validBefore; Valid = $true },
    [pscustomobject]@{ Role = 'malformed'; Path = $malformed; Valid = $false },
    [pscustomobject]@{ Role = 'valid-after'; Path = $validAfter; Valid = $true },
    [pscustomobject]@{ Role = 'truncated'; Path = $truncated; Valid = $false },
    [pscustomobject]@{ Role = 'valid-after-second'; Path = $validAfterSecond; Valid = $true }
)

foreach ($source in $sources) {
    Add-Member -InputObject $source -NotePropertyName SourceHash -NotePropertyValue ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash)
    $baseOutput = [System.IO.Path]::ChangeExtension($source.Path, '.mp3')
    [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](17, 34, 51, 68))
    Add-Member -InputObject $source -NotePropertyName BaseOutput -NotePropertyValue $baseOutput
    Add-Member -InputObject $source -NotePropertyName DestinationHash -NotePropertyValue ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash)
}

foreach ($attempt in 1..2) {
    $result = Invoke-MixedBridgeBatch -Sources $sources
    if ($result.ExitCode -ne 4) {
        throw "Expected mixed batch exit code 4 on attempt $attempt, got $($result.ExitCode): $($result.StdErr.Trim())"
    }

    foreach ($source in $sources) {
        if ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash -ne $source.SourceHash) {
            throw "Source preserved invariant failed for $($source.Role)."
        }
        if ((Get-FileHash -LiteralPath $source.BaseOutput -Algorithm SHA256).Hash -ne $source.DestinationHash) {
            throw "Mixed batch overwrote the pre-existing destination for $($source.Role)."
        }

        $stem = [System.IO.Path]::GetFileNameWithoutExtension($source.Path)
        $numbered = Join-Path $caseRoot ($stem + " ($attempt).mp3")
        if ($source.Valid) {
            if (-not (Test-Path -LiteralPath $numbered) -or (Get-Item -LiteralPath $numbered).Length -le 0) {
                throw "Mixed batch suppressed the numbered success output for $($source.Role) on attempt $attempt."
            }
            Assert-Mp3Codec -Path $numbered
        }
        elseif (Test-Path -LiteralPath $numbered) {
            throw "Mixed batch published an output for failing $($source.Role) on attempt $attempt."
        }
    }

    Assert-NoPartialOutputs
    Write-Host "Mixed batch attempt ${attempt}: exit code 4; valid-before and valid-after files published numbered MP3 outputs."
}

Write-Host 'Audio mixed-batch failure isolation: PASS.'
Write-Host 'Source preserved for every selected file; every pre-existing destination preserved; no partial outputs remained.'
