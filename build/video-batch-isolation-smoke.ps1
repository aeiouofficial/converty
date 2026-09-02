[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$fixtureFfmpeg = Join-Path $root 'artifacts/dev-ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/video-batch-isolation-smoke'

if (-not $IsWindows) { throw 'The Converty Video mixed-batch failure-isolation smoke is Windows-only.' }
foreach ($requiredPath in @($bridge, $fixtureFfmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Required dev.20 Video batch dependency is missing: $requiredPath" }
}
if (Test-Path -LiteralPath $smokeRoot) { Remove-Item -LiteralPath $smokeRoot -Recurse -Force }
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
    foreach ($argument in $Arguments) { $startInfo.ArgumentList.Add($argument) }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) { throw "Could not start process: $FileName" }
    try {
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(30000)) {
            try { $process.Kill($true) } catch [System.InvalidOperationException] { }
            throw "Process exceeded the 30-second Video batch deadline: $FileName"
        }
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StdOut = $stdoutTask.GetAwaiter().GetResult()
            StdErr = $stderrTask.GetAwaiter().GetResult()
        }
    }
    finally { $process.Dispose() }
}

function Invoke-VideoBatch {
    param([Parameter(Mandatory)] [object[]] $Sources)

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $bridge
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WorkingDirectory = $layout
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.Environment['CONVERTY_BRIDGE_NONINTERACTIVE'] = '1'
    $startInfo.ArgumentList.Add('--preset')
    $startInfo.ArgumentList.Add('video.mp4.h264')
    $startInfo.ArgumentList.Add('--')
    foreach ($source in $Sources) { $startInfo.ArgumentList.Add($source.Path) }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) { throw 'Converty.Bridge.exe could not start the Video mixed batch.' }
    try {
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(30000)) {
            try { $process.Kill($true) } catch [System.InvalidOperationException] { }
            throw 'Converty.Bridge.exe exceeded the 30-second Video mixed-batch deadline.'
        }
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StdOut = $stdoutTask.GetAwaiter().GetResult()
            StdErr = $stderrTask.GetAwaiter().GetResult()
        }
    }
    finally { $process.Dispose() }
}

function Assert-NoPartialOutputs {
    $partials = @(Get-ChildItem -LiteralPath $smokeRoot -File -Recurse | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) { throw "Video mixed batch left $($partials.Count) partial file(s) behind." }
}

function Assert-Mp4Output {
    param([Parameter(Mandatory)] [string] $Path)

    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error',
        '-show_entries', 'stream=codec_type,codec_name',
        '-of', 'json',
        $Path
    )
    if ($probe.ExitCode -ne 0) {
        throw "ffprobe could not inspect mixed-batch output '$Path': $($probe.StdErr.Trim())"
    }

    $document = $probe.StdOut | ConvertFrom-Json
    $streams = @($document.streams)
    $video = @($streams | Where-Object codec_type -EQ 'video' | ForEach-Object { [string]$_.codec_name })
    $audio = @($streams | Where-Object codec_type -EQ 'audio' | ForEach-Object { [string]$_.codec_name })
    if ($video.Count -ne 1 -or $video[0] -ne 'h264' -or $audio.Count -ne 1 -or $audio[0] -ne 'aac') {
        throw "Expected H.264 + AAC MP4 output at '$Path'; ffprobe reported video=$($video -join ',') audio=$($audio -join ',')."
    }
}

function New-VideoFixture {
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [string[]] $EncoderArgs
    )

    $arguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi', '-i', 'testsrc2=size=64x48:rate=10',
        '-f', 'lavfi', '-i', 'sine=frequency=440:sample_rate=44100',
        '-map', '0:v:0', '-map', '1:a:0',
        '-t', '0.5', '-shortest'
    ) + $EncoderArgs + @('-y', $Path)

    $result = Invoke-StructuredProcess -FileName $fixtureFfmpeg -WorkingDirectory $smokeRoot -Arguments $arguments
    if ($result.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $Path) -or (Get-Item -LiteralPath $Path).Length -le 0) {
        throw "Could not create Video mixed-batch fixture '$Path': $($result.StdErr.Trim())"
    }
}

$caseRoot = Join-Path $smokeRoot 'mixed-batch'
New-Item -ItemType Directory -Force $caseRoot | Out-Null

# Stable role tokens are deliberate test evidence for the exact ordered batch contract.
$validMp4 = Join-Path $caseRoot '01 valid-mp4 Hör & [x].mp4'
$malformedAvi = Join-Path $caseRoot '02 malformed-avi Hör & [x].avi'
$validMov = Join-Path $caseRoot '03 valid-mov Hör & [x].mov'
$validMkvFixture = Join-Path $caseRoot 'fixture-valid-mkv.mkv'
$truncatedMkv = Join-Path $caseRoot '04 truncated-mkv Hör & [x].mkv'
$validWebm = Join-Path $caseRoot '05 valid-webm Hör & [x].webm'

New-VideoFixture -Path $validMp4 -EncoderArgs @('-c:v','libx264','-preset','ultrafast','-crf','30','-pix_fmt','yuv420p','-c:a','aac','-b:a','96k','-f','mp4')
New-VideoFixture -Path $validMov -EncoderArgs @('-c:v','libx264','-preset','ultrafast','-crf','30','-pix_fmt','yuv420p','-c:a','aac','-b:a','96k','-f','mov')
New-VideoFixture -Path $validMkvFixture -EncoderArgs @('-c:v','libx264','-preset','ultrafast','-crf','30','-pix_fmt','yuv420p','-c:a','aac','-b:a','96k','-f','matroska')
New-VideoFixture -Path $validWebm -EncoderArgs @('-c:v','libvpx-vp9','-crf','36','-b:v','0','-pix_fmt','yuv420p','-c:a','libopus','-b:a','64k','-f','webm')

[System.IO.File]::WriteAllBytes($malformedAvi, [System.Text.Encoding]::UTF8.GetBytes('malformed avi payload for Video mixed-batch isolation'))
$mkvBytes = [System.IO.File]::ReadAllBytes($validMkvFixture)
if ($mkvBytes.Length -lt 16) { throw 'Valid MKV fixture is unexpectedly too small for the truncated-mkv case.' }
[System.IO.File]::WriteAllBytes($truncatedMkv, $mkvBytes[0..15])
Remove-Item -LiteralPath $validMkvFixture -Force

$sources = @(
    [pscustomobject]@{ Role = 'valid-mp4'; Path = $validMp4; Valid = $true },
    [pscustomobject]@{ Role = 'malformed-avi'; Path = $malformedAvi; Valid = $false },
    [pscustomobject]@{ Role = 'valid-mov'; Path = $validMov; Valid = $true },
    [pscustomobject]@{ Role = 'truncated-mkv'; Path = $truncatedMkv; Valid = $false },
    [pscustomobject]@{ Role = 'valid-webm'; Path = $validWebm; Valid = $true }
)

foreach ($source in $sources) {
    Add-Member -InputObject $source -NotePropertyName SourceHash -NotePropertyValue ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash)
    $baseOutput = [System.IO.Path]::ChangeExtension($source.Path, '.mp4')
    if (-not [string]::Equals($baseOutput, $source.Path, [StringComparison]::OrdinalIgnoreCase)) {
        # Reserve base output so every non-alias case proves pre-existing destination preservation.
        [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](91, 82, 73, 67))
    }
    Add-Member -InputObject $source -NotePropertyName BaseOutput -NotePropertyValue $baseOutput
    Add-Member -InputObject $source -NotePropertyName DestinationHash -NotePropertyValue ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash)
}

foreach ($attempt in 1..2) {
    $result = Invoke-VideoBatch -Sources $sources
    if ($result.ExitCode -ne 4) {
        throw "Expected Video mixed batch exit code 4 on attempt $attempt, got $($result.ExitCode): $($result.StdErr.Trim())"
    }

    foreach ($source in $sources) {
        if ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash -ne $source.SourceHash) {
            throw "Source preserved invariant failed for Video $($source.Role)."
        }
        if ((Get-FileHash -LiteralPath $source.BaseOutput -Algorithm SHA256).Hash -ne $source.DestinationHash) {
            throw "Pre-existing destination changed for Video $($source.Role)."
        }

        $stem = [System.IO.Path]::GetFileNameWithoutExtension($source.Path)
        $numbered = Join-Path $caseRoot ($stem + " ($attempt).mp4")
        if ($source.Valid) {
            if (-not (Test-Path -LiteralPath $numbered) -or (Get-Item -LiteralPath $numbered).Length -le 0) {
                throw "Later valid Video was suppressed for $($source.Role) on attempt $attempt."
            }
            Assert-Mp4Output -Path $numbered
        }
        elseif (Test-Path -LiteralPath $numbered) {
            throw "Failing Video $($source.Role) unexpectedly published an output on attempt $attempt."
        }
    }

    Assert-NoPartialOutputs
    Write-Host "Video mixed batch attempt ${attempt}: exit code 4; all later valid files published."
}

Start-Sleep -Seconds 1
$orphaned = @(Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and
    $_.CommandLine -like "*$layout*" -and
    ($_.Name -in @('Converty.EngineWorker.exe', 'ffmpeg.exe', 'ffprobe.exe'))
})
if ($orphaned.Count -ne 0) {
    $details = ($orphaned | ForEach-Object { "$($_.Name) pid=$($_.ProcessId) command=$($_.CommandLine)" }) -join '; '
    throw "Video mixed batch left orphan converter processes running: $details"
}

Assert-NoPartialOutputs
Write-Host 'Video mixed-batch failure isolation: PASS.'
Write-Host 'valid-mp4 -> malformed-avi -> valid-mov -> truncated-mkv -> valid-webm was attempted twice in one Bridge process per run; later valid Videos published; sources and pre-existing destinations stayed unchanged; no partials or orphan converter processes remained.'
