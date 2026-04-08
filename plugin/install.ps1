# Install the Stream Deck plugin by copying into the Plugins directory.
# Stream Deck must be restarted after install.
# Re-run this script after any code changes.

$ErrorActionPreference = "Stop"

$pluginName = "com.niklasleide.bt-toggle.sdPlugin"
$pluginsDir = "$env:APPDATA\Elgato\StreamDeck\Plugins"
$targetPath = "$pluginsDir\$pluginName"
$sourcePath = "\\wsl.localhost\Ubuntu-24.04\home\niklas\projects\leides-ljuvliga-lilla-bt-toggle\plugin"

if (-not (Test-Path $pluginsDir)) {
    Write-Error "Stream Deck plugins directory not found: $pluginsDir"
    Write-Error "Is Stream Deck installed?"
    exit 1
}

# Remove existing install
if (Test-Path $targetPath) {
    Write-Host "Removing existing plugin..."
    Remove-Item $targetPath -Recurse -Force
}

Write-Host "Copying plugin to $targetPath..."
Copy-Item $sourcePath $targetPath -Recurse -Exclude @(".git", "*.ps1", "*.gyp", "generate-icons.*")

Write-Host ""
Write-Host "Plugin installed. Restart Stream Deck to load it."
Write-Host "  Right-click Stream Deck tray icon > Quit"
Write-Host "  Then reopen Stream Deck"
