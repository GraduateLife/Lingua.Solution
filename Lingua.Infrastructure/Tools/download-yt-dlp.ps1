# Script to download yt-dlp to the project tools directory
# This script downloads the latest yt-dlp.exe for Windows

$ErrorActionPreference = "Stop"

$toolsDir = $PSScriptRoot
$ytDlpPath = Join-Path $toolsDir "yt-dlp.exe"

Write-Host "Downloading yt-dlp to: $ytDlpPath" -ForegroundColor Green

# Get the latest release URL from GitHub
$latestReleaseUrl = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest"

try {
    Write-Host "Fetching latest release information..." -ForegroundColor Yellow
    $releaseInfo = Invoke-RestMethod -Uri $latestReleaseUrl -Headers @{
        "Accept" = "application/vnd.github.v3+json"
        "User-Agent" = "Lingua-Solution"
    }
    
    # Find the Windows executable download URL
    $downloadUrl = $null
    foreach ($asset in $releaseInfo.assets) {
        if ($asset.name -eq "yt-dlp.exe") {
            $downloadUrl = $asset.browser_download_url
            break
        }
    }
    
    if ($null -eq $downloadUrl) {
        throw "Could not find yt-dlp.exe in the latest release"
    }
    
    Write-Host "Found latest version: $($releaseInfo.tag_name)" -ForegroundColor Green
    Write-Host "Downloading from: $downloadUrl" -ForegroundColor Yellow
    
    # Download the file
    Invoke-WebRequest -Uri $downloadUrl -OutFile $ytDlpPath -UseBasicParsing
    
    Write-Host "Download completed successfully!" -ForegroundColor Green
    Write-Host "yt-dlp.exe saved to: $ytDlpPath" -ForegroundColor Green
    
    # Verify the file exists
    if (Test-Path $ytDlpPath) {
        $fileInfo = Get-Item $ytDlpPath
        Write-Host "File size: $($fileInfo.Length) bytes" -ForegroundColor Cyan
        Write-Host "You can now use yt-dlp in the project!" -ForegroundColor Green
    } else {
        throw "Downloaded file not found at expected location"
    }
}
catch {
    Write-Host "Error downloading yt-dlp: $_" -ForegroundColor Red
    Write-Host "You can manually download yt-dlp.exe from:" -ForegroundColor Yellow
    Write-Host "https://github.com/yt-dlp/yt-dlp/releases/latest" -ForegroundColor Yellow
    Write-Host "And place it in the tools directory: $toolsDir" -ForegroundColor Yellow
    exit 1
}

