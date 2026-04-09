# Maintenance — leides-ljuvliga-lilla-bt-toggle

> Fill this in THE MOMENT you get the project running. Not later. Now.
> If you can't run the project from these instructions alone, they're not done yet.
> Run /project:resume when returning — it reads this file first.

## How to run this project

```bash
# 1. Navigate to project
cd /home/niklas/projects/leides-ljuvliga-lilla-bt-toggle

# 2. Build the C# CLI (optional — the Stream Deck plugin is the main deliverable)
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe build

# 3. Build the native addon (run from PowerShell or WSL)
# From WSL:
powershell.exe -ExecutionPolicy Bypass -File plugin/build-native.ps1
# This copies to a Windows temp dir, compiles with MSVC, copies .node back.

# 4. Install Stream Deck plugin (run from PowerShell, Stream Deck must be QUIT first)
powershell.exe -ExecutionPolicy Bypass -File plugin/install.ps1
# Then restart Stream Deck.

# 5. Configure
# In Stream Deck app: drag "Bluetooth Toggle" to a button, enter MAC address in settings.
```

## Environment variables needed
| Variable | Where to get it | Required? |
|----------|----------------|-----------|
| `BT_MAC_ADDRESS` | Windows Settings > Bluetooth > Device > Properties | Yes (in src/.env for CLI, or via Property Inspector for plugin) |

## Dependencies and versions
| Tool/Library | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 10.0.201 | Windows-side only, called via `/mnt/c/Program Files/dotnet/dotnet.exe` |
| Node.js | 25.7.0 | Windows-side |
| MSVC Build Tools | 2022 (17.14) | For compiling C++ N-API addon |
| Python | 3.13.3 | Required by node-gyp |
| node-addon-api | ^8.0.0 | N-API C++ wrapper |
| ws | ^8.0.0 | WebSocket client for Stream Deck SDK |

## Data file locations
- Stream Deck plugin: `%APPDATA%\Elgato\StreamDeck\Plugins\com.niklasleide.bt-toggle.sdPlugin\`
- Stream Deck logs: `%APPDATA%\Elgato\StreamDeck\logs\StreamDeck.log`
- MAC address (CLI): `src/.env` (gitignored)
- MAC address (plugin): stored in Stream Deck settings (per-button)

## Known environment quirks
- node-gyp cannot build from WSL UNC paths — `build-native.ps1` copies to Windows temp dir first
- `npm install` in plugin/ triggers node-gyp rebuild which fails on UNC — use `--ignore-scripts`
- Stream Deck locks `.node` file while running — must quit before reinstalling
- manifest.json `"Nodejs"` key is case-sensitive — lowercase 's' or plugin won't launch

## How to update dependencies safely
```bash
# Node (from plugin/ directory via PowerShell):
npm outdated
npm update [package] --ignore-scripts

# Rebuild native addon after any node-addon-api update:
powershell.exe -ExecutionPolicy Bypass -File plugin/build-native.ps1
```

## Last parked
**2026-04-09** — Sprint 3 partial. Button mode setting added (toggle/connect only/disconnect only). Multi-device and connection listener still todo.

---
> Update the "How to run" section the moment you figure out the setup.
> Do it while it's fresh — not when you're returning cold in 3 months.
