$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$ProgressPreference = 'SilentlyContinue'

$repository = 'ledung2411/MutiManagerForMe'
$assetPattern = 'MutiManagerForMe-Setup-*-win-x64.exe'
$githubHeaders = @{
    'User-Agent' = 'MutiManagerForMe installer'
    'Accept' = 'application/vnd.github+json'
}

if ([Net.ServicePointManager]::SecurityProtocol -band [Net.SecurityProtocolType]::Tls12) {
    # TLS 1.2 is already enabled.
} else {
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
}

$release = Invoke-RestMethod `
    -Uri "https://api.github.com/repos/$repository/releases/latest" `
    -Headers $githubHeaders

$asset = $release.assets |
    Where-Object { $_.name -like $assetPattern } |
    Sort-Object -Property name -Descending |
    Select-Object -First 1

if (-not $asset) {
    throw "No installer asset matching '$assetPattern' was found in the latest release."
}

$installerPath = Join-Path $env:TEMP $asset.name

Write-Host "Downloading $($asset.name)..."
Invoke-WebRequest `
    -Uri $asset.browser_download_url `
    -OutFile $installerPath `
    -UseBasicParsing

Write-Host 'Installing MutiManagerForMe...'
$installArgs = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART')
$process = Start-Process `
    -FilePath $installerPath `
    -ArgumentList $installArgs `
    -Wait `
    -PassThru

if ($process.ExitCode -ne 0) {
    throw "Installer exited with code $($process.ExitCode)."
}

Write-Host 'MutiManagerForMe installed successfully.'
