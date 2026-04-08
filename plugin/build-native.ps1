# Build the native N-API addon from a Windows-native path
# (node-gyp doesn't support UNC/WSL paths)

$ErrorActionPreference = "Stop"

$wslSource = "\\wsl.localhost\Ubuntu-24.04\home\niklas\projects\leides-ljuvliga-lilla-bt-toggle\plugin"
$tempDir = "$env:TEMP\bt-toggle-build"

Write-Host "Copying to $tempDir..."
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
Copy-Item $wslSource $tempDir -Recurse -Exclude @("node_modules", "build")

Set-Location $tempDir
Write-Host "Installing dependencies..."
npm install

Write-Host "Building native addon..."
node-gyp rebuild

Write-Host "Copying build output back..."
$buildDir = "$wslSource\build"
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
Copy-Item "$tempDir\build" $buildDir -Recurse

Write-Host "Done. Native addon at plugin/build/Release/bluetooth.node"
