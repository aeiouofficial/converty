[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$probeWorker = Join-Path $layout 'Converty.ProbeWorker.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $layout 'tools/ffmpeg/ffprobe.exe'
$work = Join-Path $root 'artifacts/probe-worker-smoke'
$contractMarker = 'media.probe.result.v1'

if (-not $IsWindows) {
    throw 'The packaged ProbeWorker/ffprobe smoke is Windows-only.'
}
foreach ($required in @($probeWorker, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required packaged probe qualification payload is missing: $required"
    }
}

if (Test-Path $work) {
    Remove-Item -Recurse -Force $work
}
$unicodeMetacharDirectory = Join-Path $work 'unicode Hör & metachar [x]'
New-Item -ItemType Directory -Force $unicodeMetacharDirectory | Out-Null

$validInput = Join-Path $unicodeMetacharDirectory 'probe ü & semi; [x].wav'
& $ffmpeg '-hide_banner' '-loglevel' 'error' '-y' '-f' 'lavfi' '-i' 'sine=frequency=1000:duration=0.20' '-c:a' 'pcm_s16le' $validInput
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $validInput)) {
    throw 'Packaged ffmpeg.exe could not create the deterministic ProbeWorker fixture.'
}

$malformedInput = Join-Path $unicodeMetacharDirectory 'unsupportedInput & metachar [x].bin'
[System.IO.File]::WriteAllText($malformedInput, 'this is not a supported media container')

function Invoke-ProbeWorker {
    param([Parameter(Mandatory)][string]$InputPath)

    $stderrPath = Join-Path $work ("probe-stderr-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
    try {
        $stdoutLines = @(& $probeWorker '--input' $InputPath 2> $stderrPath)
        $exitCode = $LASTEXITCODE
        $stderr = if (Test-Path $stderrPath) { Get-Content -Raw $stderrPath } else { '' }
        return [pscustomobject]@{
            ExitCode = $exitCode
            Stdout = ($stdoutLines -join [Environment]::NewLine)
            Stderr = $stderr
        }
    }
    finally {
        Remove-Item -LiteralPath $stderrPath -Force -ErrorAction SilentlyContinue
    }
}

$trackedNames = @('Converty.ProbeWorker.exe', 'ffprobe.exe')
$beforeProcessIds = @(
    Get-CimInstance Win32_Process |
        Where-Object { $_.Name -in $trackedNames } |
        ForEach-Object { [int]$_.ProcessId }
)

$valid = Invoke-ProbeWorker -InputPath $validInput
if ($valid.ExitCode -ne 0) {
    throw "Packaged ProbeWorker valid probe failed with exit code $($valid.ExitCode): $($valid.Stderr)"
}
$validResult = $valid.Stdout | ConvertFrom-Json -ErrorAction Stop
if ($validResult.schemaVersion -ne 1 -or
    $validResult.status -ne 'success' -or
    $validResult.failureReason -ne 'none' -or
    $null -eq $validResult.facts -or
    @($validResult.facts.streams).Count -lt 1) {
    throw "Packaged ProbeWorker did not return a valid $contractMarker success contract."
}

$unsupported = Invoke-ProbeWorker -InputPath $malformedInput
if ($unsupported.ExitCode -ne 0) {
    throw "Packaged ProbeWorker unsupported-input probe failed structurally with exit code $($unsupported.ExitCode): $($unsupported.Stderr)"
}
$unsupportedResult = $unsupported.Stdout | ConvertFrom-Json -ErrorAction Stop
if ($unsupportedResult.schemaVersion -ne 1 -or
    $unsupportedResult.status -ne 'failure' -or
    $unsupportedResult.failureReason -ne 'unsupportedInput' -or
    $null -ne $unsupportedResult.facts) {
    throw "Packaged ProbeWorker did not fail closed as unsupportedInput for malformed media."
}

Start-Sleep -Milliseconds 250
$afterProcessIds = @(
    Get-CimInstance Win32_Process |
        Where-Object { $_.Name -in $trackedNames } |
        ForEach-Object { [int]$_.ProcessId }
)
$newProcessIds = @($afterProcessIds | Where-Object { $_ -notin $beforeProcessIds })
if ($newProcessIds.Count -ne 0) {
    throw "ProbeWorker/ffprobe orphan processes remain after smoke: $($newProcessIds -join ', ')"
}

Write-Host 'Probe worker packaged ffprobe acceptance: PASS'
Write-Host "Contract: $contractMarker"
Write-Host 'Unicode/metachar input path probed successfully; malformed input returned unsupportedInput; no ProbeWorker/ffprobe orphan remained.'
