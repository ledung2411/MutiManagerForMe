$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'src\MutiManagerForMe.App\MutiManagerForMe.App.csproj'
$publishDir = Join-Path $repoRoot 'artifacts\MutiManagerForMe-win-x64'
$installerScript = Join-Path $repoRoot 'installer\MutiManagerForMe.iss'
$installerOutput = Join-Path $repoRoot 'artifacts\installer\MutiManagerForMe-Setup-1.1.0-win-x64.exe'

$isccCommand = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
$iscc = if ($isccCommand) {
    $isccCommand.Source
} else {
    @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
        'C:\Program Files\Inno Setup 6\ISCC.exe'
    ) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if (-not $iscc) {
    throw 'Inno Setup compiler (ISCC.exe) was not found. Install it with: winget install --id JRSoftware.InnoSetup -e'
}

New-Item -ItemType Directory -Path (Split-Path -Parent $publishDir) -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path -Parent $installerOutput) -Force | Out-Null

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:WindowsAppSDKSelfContained=true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $publishDir

& $iscc $installerScript

if (-not (Test-Path -LiteralPath $installerOutput)) {
    throw "Installer was not created: $installerOutput"
}

Write-Host "Installer created: $installerOutput"
