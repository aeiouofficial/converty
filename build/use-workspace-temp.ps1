[CmdletBinding()]
param([string]$Workspace = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($Workspace)
if ([IO.Path]::GetPathRoot($root) -ieq 'C:\') {
    throw 'Converty project scratch and caches are forbidden on C:.'
}

$tempRoot = Join-Path $root '_temp'
$cacheRoot = Join-Path $root '_cache'
foreach ($path in @($tempRoot, $cacheRoot, (Join-Path $cacheRoot 'pip'),
    (Join-Path $cacheRoot 'nuget'), (Join-Path $cacheRoot 'npm'),
    (Join-Path $cacheRoot 'dotnet'))) {
    [void](New-Item -ItemType Directory -Force -Path $path)
}

$env:CONVERTY_WORKSPACE_ROOT = $root
$env:TEMP = $tempRoot
$env:TMP = $tempRoot
$env:TMPDIR = $tempRoot
$env:PIP_CACHE_DIR = Join-Path $cacheRoot 'pip'
$env:NUGET_PACKAGES = Join-Path $cacheRoot 'nuget'
$env:NPM_CONFIG_CACHE = Join-Path $cacheRoot 'npm'
$env:DOTNET_CLI_HOME = Join-Path $cacheRoot 'dotnet'
Write-Output "Converty project scratch/cache: $root"
