[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Join-Path $root 'artifacts/dev-package-layout'
$bridge = Join-Path $layout 'Converty.Bridge.exe'
$ffmpeg = Join-Path $layout 'tools/ffmpeg/ffmpeg.exe'
$smokeRoot = Join-Path $root 'artifacts/product-conversion-smoke'

if (-not $IsWindows) {
    throw 'The Converty product conversion smoke is Windows-only.'
}
if (-not (Test-Path $bridge)) {
    throw 'Staged Converty.Bridge.exe is missing.'
}
if (-not (Test-Path $ffmpeg)) {
    throw 'Staged trusted ffmpeg.exe is missing.'
}

if (Test-Path $smokeRoot) {
    Remove-Item -Recurse -Force $smokeRoot
}
New-Item -ItemType Directory -Force $smokeRoot | Out-Null

$input = Join-Path $smokeRoot 'Hör test & semi; -dash [x].wav'
$existingOutput = [System.IO.Path]::ChangeExtension($input, '.mp3')
$expectedOutput = Join-Path $smokeRoot 'Hör test & semi; -dash [x] (1).mp3'

# Write a tiny mono PCM WAV without using another media tool. This makes the
# Bridge-to-FFmpeg path the only conversion operation under test.
$sampleRate = 8000
$sampleCount = 1600
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

# Reserve the base destination to prove Converty never silently overwrites it.
[System.IO.File]::WriteAllBytes($existingOutput, [byte[]](1, 2, 3, 4))
$existingHash = (Get-FileHash -Algorithm SHA256 $existingOutput).Hash

& $bridge '--preset' 'audio.mp3' '--' $input
if ($LASTEXITCODE -ne 0) {
    throw "Converty.Bridge.exe product smoke failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path $input)) {
    throw 'Product smoke removed or replaced the source file.'
}
if (-not (Test-Path $expectedOutput)) {
    throw "Expected numbered conversion output was not created: $expectedOutput"
}
if ((Get-Item $expectedOutput).Length -le 0) {
    throw 'Product smoke produced an empty conversion output.'
}
if ((Get-FileHash -Algorithm SHA256 $existingOutput).Hash -ne $existingHash) {
    throw 'Product smoke overwrote the pre-existing destination file.'
}

Write-Host 'Product conversion smoke: PASS'
Write-Host "Source preserved: $input"
Write-Host "Existing destination preserved: $existingOutput"
Write-Host "Converted output: $expectedOutput"
