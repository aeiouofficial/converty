[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$ffprobe = Join-Path $root 'artifacts/dev-ffmpeg/ffprobe.exe'
$smokeRoot = Join-Path $root 'artifacts/image-input-acceptance-smoke'

if (-not $IsWindows) {
    throw 'The Converty Image input acceptance smoke is Windows-only.'
}
foreach ($requiredPath in @($bridge, $ffmpeg, $ffprobe)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required dev.18 acceptance dependency is missing: $requiredPath"
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

function Invoke-Bridge {
    param(
        [Parameter(Mandatory)] [string] $PresetId,
        [Parameter(Mandatory)] [object] $Source
    )

    # Mirror Explorer structurally: one fixed Bridge executable, one typed preset ID,
    # and the selected filesystem path as an independent ArgumentList token.
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
    $bridgeStartInfo.ArgumentList.Add($Source.Path)

    $bridgeProcess = [System.Diagnostics.Process]::Start($bridgeStartInfo)
    if ($null -eq $bridgeProcess) {
        throw "Converty.Bridge.exe could not start preset $PresetId."
    }

    try {
        $stdoutTask = $bridgeProcess.StandardOutput.ReadToEndAsync()
        $stderrTask = $bridgeProcess.StandardError.ReadToEndAsync()
        if (-not $bridgeProcess.WaitForExit(30000)) {
            try { $bridgeProcess.Kill($true) } catch [System.InvalidOperationException] { }
            throw "Converty.Bridge.exe exceeded the 30-second acceptance deadline for $PresetId."
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

    $partials = @(Get-ChildItem -LiteralPath $Directory -File -Recurse | Where-Object Name -Like '.converty-*.partial.*')
    if ($partials.Count -ne 0) {
        throw "Image acceptance left $($partials.Count) .converty-*.partial.* file(s) behind in $Directory."
    }
}

function Get-ImageProbe {
    param([Parameter(Mandatory)] [string] $Path)

    $probeArguments = @(
        '-v', 'error',
        '-select_streams', 'v:0',
        '-show_entries', 'stream=codec_name,width,height',
        '-of', 'json',
        $Path
    )
    $probe = Invoke-StructuredProcess -FileName $ffprobe -WorkingDirectory $smokeRoot -Arguments $probeArguments
    if ($probe.ExitCode -ne 0) {
        throw "ffprobe could not inspect Image acceptance output '$Path': $($probe.StdErr.Trim())"
    }

    $parsed = $probe.StdOut | ConvertFrom-Json
    if ($null -eq $parsed.streams -or @($parsed.streams).Count -ne 1) {
        throw "ffprobe returned no unique video stream for Image output '$Path'."
    }
    return @($parsed.streams)[0]
}

$fixturesRoot = Join-Path $smokeRoot 'fixtures'
New-Item -ItemType Directory -Force $fixturesRoot | Out-Null

# Pinned development FFmpeg is used only to create deterministic Image fixtures.
# Every conversion under test enters packaged Bridge -> Strict Worker/provider -> app-local FFmpeg.
$sourceFixtures = @(
    [pscustomobject]@{ Id = 'png'; Extension = '.png'; EncoderArgs = @('-c:v', 'png') },
    [pscustomobject]@{ Id = 'jpg'; Extension = '.jpg'; EncoderArgs = @('-c:v', 'mjpeg', '-q:v', '3') },
    [pscustomobject]@{ Id = 'jpeg'; Extension = '.jpeg'; EncoderArgs = @('-c:v', 'mjpeg', '-q:v', '3') },
    [pscustomobject]@{ Id = 'webp'; Extension = '.webp'; EncoderArgs = @('-c:v', 'libwebp', '-quality', '80') },
    [pscustomobject]@{ Id = 'bmp'; Extension = '.bmp'; EncoderArgs = @('-c:v', 'bmp') },
    [pscustomobject]@{ Id = 'gif'; Extension = '.gif'; EncoderArgs = @('-c:v', 'gif') },
    [pscustomobject]@{ Id = 'tif'; Extension = '.tif'; EncoderArgs = @('-c:v', 'tiff') },
    [pscustomobject]@{ Id = 'tiff'; Extension = '.tiff'; EncoderArgs = @('-c:v', 'tiff') }
)

foreach ($fixture in $sourceFixtures) {
    $fixturePath = Join-Path $fixturesRoot ("source-$($fixture.Id)$($fixture.Extension)")
    $fixtureArguments = @(
        '-hide_banner', '-loglevel', 'error',
        '-f', 'lavfi', '-i', 'color=c=0x336699:size=64x48:rate=1',
        '-frames:v', '1'
    ) + $fixture.EncoderArgs + @('-y', $fixturePath)

    $result = Invoke-StructuredProcess -FileName $ffmpeg -WorkingDirectory $fixturesRoot -Arguments $fixtureArguments
    if ($result.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $fixturePath)) {
        throw "Could not create $($fixture.Id) Image fixture: $($result.StdErr.Trim())"
    }
    Add-Member -InputObject $fixture -NotePropertyName Path -NotePropertyValue $fixturePath
}

$targets = @(
    [pscustomobject]@{ PresetId = 'image.png'; Extension = '.png'; Codec = 'png' },
    [pscustomobject]@{ PresetId = 'image.jpeg'; Extension = '.jpg'; Codec = 'mjpeg' },
    [pscustomobject]@{ PresetId = 'image.webp'; Extension = '.webp'; Codec = 'webp' }
)

$successRoot = Join-Path $smokeRoot 'success'
$successCount = 0
foreach ($fixture in $sourceFixtures) {
    foreach ($target in $targets) {
        $caseDirectory = Join-Path $successRoot ("$($fixture.Id)-to-$($target.PresetId)")
        New-Item -ItemType Directory -Force $caseDirectory | Out-Null

        $inputPath = Join-Path $caseDirectory ("Hör image & semi; -dash [x]$($fixture.Extension)")
        Copy-Item -LiteralPath $fixture.Path -Destination $inputPath
        $source = [pscustomobject]@{ Path = $inputPath }
        $sourceHash = (Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash

        $baseOutput = [System.IO.Path]::ChangeExtension($source.Path, $target.Extension)
        if (-not [string]::Equals($baseOutput, $source.Path, [StringComparison]::OrdinalIgnoreCase)) {
            # Reserve the base target to prove a pre-existing destination is never overwritten.
            [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](17, 34, 51, 68))
        }
        $baseOutputHash = (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash

        $stem = [System.IO.Path]::GetFileNameWithoutExtension($source.Path)
        $numberedOutput = Join-Path $caseDirectory ($stem + ' (1)' + $target.Extension)
        $result = Invoke-Bridge -PresetId $target.PresetId -Source $source
        if ($result.ExitCode -ne 0) {
            throw "Expected Image success for $($fixture.Id) -> $($target.PresetId), got exit $($result.ExitCode): $($result.StdErr.Trim())"
        }

        if ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash -ne $sourceHash) {
            throw "Source preserved invariant failed for $($fixture.Id) -> $($target.PresetId)."
        }
        if ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $baseOutputHash) {
            throw "Image conversion overwrote the pre-existing destination for $($fixture.Id) -> $($target.PresetId)."
        }
        if (-not (Test-Path -LiteralPath $numberedOutput) -or (Get-Item -LiteralPath $numberedOutput).Length -le 0) {
            throw "Image conversion did not publish the numbered output for $($fixture.Id) -> $($target.PresetId)."
        }
        Assert-NoPartialOutputs -Directory $caseDirectory

        $probe = Get-ImageProbe -Path $numberedOutput
        if ($probe.codec_name -ne $target.Codec) {
            throw "Expected Image codec '$($target.Codec)' for $($fixture.Id) -> $($target.PresetId); got '$($probe.codec_name)'."
        }
        if ([int]$probe.width -ne 64 -or [int]$probe.height -ne 48) {
            throw "Expected 64x48 Image dimensions for $($fixture.Id) -> $($target.PresetId); got $($probe.width)x$($probe.height)."
        }

        $successCount++
        Write-Host "Accepted $($fixture.Id) -> $($target.PresetId): $($probe.codec_name) 64x48"
    }
}

$negativeRoot = Join-Path $smokeRoot 'negative'
New-Item -ItemType Directory -Force $negativeRoot | Out-Null

$malformedPath = Join-Path $negativeRoot 'malformed Hör & [x].png'
[System.IO.File]::WriteAllBytes($malformedPath, [System.Text.Encoding]::UTF8.GetBytes('malformed image payload for dev18'))

$validTiff = ($sourceFixtures | Where-Object Id -EQ 'tiff').Path
$validTiffBytes = [System.IO.File]::ReadAllBytes($validTiff)
$truncatedPath = Join-Path $negativeRoot 'truncated Hör & [x].tiff'
$truncatedLength = [Math]::Min(16, $validTiffBytes.Length)
[System.IO.File]::WriteAllBytes($truncatedPath, $validTiffBytes[0..($truncatedLength - 1)])

$negativeCases = @(
    [pscustomobject]@{ Name = 'malformed'; Path = $malformedPath; PresetId = 'image.jpeg'; Extension = '.jpg' },
    [pscustomobject]@{ Name = 'truncated'; Path = $truncatedPath; PresetId = 'image.jpeg'; Extension = '.jpg' }
)

foreach ($source in $negativeCases) {
    $sourceHash = (Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash
    $baseOutput = [System.IO.Path]::ChangeExtension($source.Path, $source.Extension)
    [System.IO.File]::WriteAllBytes($baseOutput, [byte[]](85, 102, 119, 136))
    $destinationHash = (Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($source.Path)
    $numberedOutput = Join-Path $negativeRoot ($stem + ' (1)' + $source.Extension)

    foreach ($attempt in 1..2) {
        $result = Invoke-Bridge -PresetId $source.PresetId -Source $source
        if ($result.ExitCode -ne 4) {
            throw "Expected deterministic Image exit code 4 for $($source.Name) attempt $attempt, got $($result.ExitCode): $($result.StdErr.Trim())"
        }
        if ((Get-FileHash -LiteralPath $source.Path -Algorithm SHA256).Hash -ne $sourceHash) {
            throw "Source preserved invariant failed for $($source.Name) Image input."
        }
        if ((Get-FileHash -LiteralPath $baseOutput -Algorithm SHA256).Hash -ne $destinationHash) {
            throw "Image failure overwrote the pre-existing destination for $($source.Name)."
        }
        if (Test-Path -LiteralPath $numberedOutput) {
            throw "Image failure published a numbered output for $($source.Name)."
        }
        Assert-NoPartialOutputs -Directory $negativeRoot
    }

    Write-Host "Rejected $($source.Name) Image input deterministically with exit code 4."
}

Write-Host "Image source and malformed-input acceptance: PASS ($successCount successful conversions; $($negativeCases.Count) repeated negative cases)."
Write-Host 'Source preserved for every Image input; every pre-existing destination preserved; no partial outputs remained.'
