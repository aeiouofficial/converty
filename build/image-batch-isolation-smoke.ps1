[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/image-batch-isolation-smoke'

if (-not $IsWindows) { throw 'The Converty Image mixed-batch failure-isolation smoke is Windows-only.' }
foreach ($requiredPath in @($bridge, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Required dev.19 dependency is missing: $requiredPath" }
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
            throw "Process exceeded the 30-second acceptance deadline: $FileName"
        }
        return [pscustomobject]@{ ExitCode = $process.ExitCode; StdOut = $stdoutTask.GetAwaiter().GetResult(); StdErr = $stderrTask.GetAwaiter().GetResult() }
    }
    finally { $process.Dispose() }
}

function Invoke-ImageBatch {
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
    $startInfo.ArgumentList.Add('image.png')
    $startInfo.ArgumentList.Add('--')
    foreach ($source in $Sources) { $startInfo.ArgumentList.Add($source.Path) }
    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) { throw 'Converty.Bridge.exe could not start the Image mixed batch.' }
    try {
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(30000)) {
            try { $process.Kill($true) } catch [System.InvalidOperationException] { }
            throw 'Converty.Bridge.exe exceeded the 30-second Image mixed-batch deadline.'
        }
        return [pscustomobject]@{ ExitCode = $process.ExitCode; StdOut = $stdoutTask.GetAwaiter().GetResult(); StdErr = $stderrTask.GetAwaiter().GetResult() }
    }
    finally { $process.Dispose() }
}

function Assert-NoPartialOutputs {
    $partials = @(Get-ChildItem -LiteralPath $smokeRoot -File -Recurse | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) { throw "Image mixed batch left $($partials.Count) partial file(s) behind." }
}

function Assert-PngOutput {
    param([Parameter(Mandatory)] [string] $Path)
    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments @(
        '-v', 'error', '-select_streams', 'v:0',
        '-show_entries', 'stream=codec_name,width,height',
        '-of', 'default=noprint_wrappers=1:nokey=0', $Path
    )
    if ($probe.ExitCode -ne 0 -or $probe.StdOut -notmatch 'codec_name=png' -or $probe.StdOut -notmatch 'width=64' -or $probe.StdOut -notmatch 'height=48') {
        throw "Expected 64x48 PNG output at '$Path': $($probe.StdOut.Trim()) $($probe.StdErr.Trim())"
    }
}

# Development FFmpeg creates fixtures only. The conversion under test remains packaged Bridge -> Strict Worker/provider -> app-local FFmpeg.
$fixturePng = Join-Path $smokeRoot 'fixture-valid.png'
$fixtureArgs = @('-hide_banner','-loglevel','error','-f','lavfi','-i','color=c=gray:s=64x48','-frames:v','1','-y',$fixturePng)
$fixtureResult = Invoke-StructuredProcess -FileName $ffmpeg -WorkingDirectory $smokeRoot -Arguments $fixtureArgs
if ($fixtureResult.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $fixturePng)) { throw "Could not create Image fixture: $($fixtureResult.StdErr.Trim())" }

$caseRoot = Join-Path $smokeRoot 'mixed-batch'
New-Item -ItemType Directory -Force $caseRoot | Out-Null
$validBefore = Join-Path $caseRoot '01 valid-before Hör & [x].png'
$malformed = Join-Path $caseRoot '02 malformed Hör & [x].jpg'
$validAfter = Join-Path $caseRoot '03 valid-after Hör & [x].webp'
$truncated = Join-Path $caseRoot '04 truncated Hör & [x].bmp'
$validLast = Join-Path $caseRoot '05 valid-last Hör & [x].jpeg'

Copy-Item -LiteralPath $fixturePng -Destination $validBefore
Copy-Item -LiteralPath $fixturePng -Destination $validAfter
Copy-Item -LiteralPath $fixturePng -Destination $validLast
[System.IO.File]::WriteAllBytes($malformed, [System.Text.Encoding]::UTF8.GetBytes('malformed image payload for batch isolation'))
$pngBytes = [System.IO.File]::ReadAllBytes($fixturePng)
$truncatedLength = [Math]::Min(12, $pngBytes.Length)
[System.IO.File]::WriteAllBytes($truncated, $pngBytes[0..($truncatedLength - 1)])

$sources = @(
    [pscustomobject]@{ Role='valid-before'; Path=$validBefore; Valid=$true },
    [pscustomobject]@{ Role='malformed'; Path=$malformed; Valid=$false },
    [pscustomobject]@{ Role='valid-after'; Path=$validAfter; Valid=$true },
    [pscustomobject]@{ Role='truncated'; Path=$truncated; Valid=$false },
    [pscustomobject]@{ Role='valid-last'; Path=$validLast; Valid=$true }
)
foreach ($source in $sources) {
    Add-Member -InputObject $source -NotePropertyName SourceHash -NotePropertyValue ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash)
    $baseOutput = [System.IO.Path]::ChangeExtension($source.Path, '.png')
    [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](91,82,73,67))
    Add-Member -InputObject $source -NotePropertyName BaseOutput -NotePropertyValue $baseOutput
    Add-Member -InputObject $source -NotePropertyName DestinationHash -NotePropertyValue ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash)
}

for ($attempt = 1; $attempt -le 2; $attempt++) {
    $result = Invoke-ImageBatch -Sources $sources
    if ($result.ExitCode -ne 4) { throw "Expected Image mixed batch exit code 4 on attempt $attempt, got $($result.ExitCode): $($result.StdErr.Trim())" }
    foreach ($source in $sources) {
        if ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash -ne $source.SourceHash) { throw "Source preserved invariant failed for $($source.Role)." }
        if ((Get-FileHash -LiteralPath $source.BaseOutput -Algorithm SHA256).Hash -ne $source.DestinationHash) { throw "Pre-existing destination changed for $($source.Role)." }
        $stem = [System.IO.Path]::GetFileNameWithoutExtension($source.Path)
        $numbered = Join-Path $caseRoot ($stem + " ($attempt).png")
        if ($source.Valid) {
            if (-not (Test-Path -LiteralPath $numbered) -or (Get-Item -LiteralPath $numbered).Length -le 0) { throw "Later valid Image was suppressed for $($source.Role) on attempt $attempt." }
            Assert-PngOutput -Path $numbered
        } elseif (Test-Path -LiteralPath $numbered) { throw "Failing Image $($source.Role) unexpectedly published an output." }
    }
    Assert-NoPartialOutputs
    Write-Host "Image mixed batch attempt ${attempt}: exit code 4; all later valid files published." 
}

Start-Sleep -Seconds 1
$orphaned = @(Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -and $_.CommandLine -like "*$layout*" -and ($_.Name -in @('Converty.EngineWorker.exe','ffmpeg.exe','ffprobe.exe')) })
if ($orphaned.Count -ne 0) { throw "Image mixed batch left $($orphaned.Count) converter worker/process instance(s) running." }

Write-Host 'Image mixed-batch failure isolation: PASS.'
Write-Host 'Later valid Images survived malformed/truncated members; sources and existing destinations remained unchanged; no partials or orphan converter processes remained.'
