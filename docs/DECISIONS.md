# Decision Log — leides-ljuvliga-lilla-bt-toggle

Record of key decisions made during the project. **Newest first.**

> The alternatives you *rejected* are as important as what you chose.
> Future sessions will read this — make the reasoning explicit.

---

## Format
```
### DEC-NNN: Title
**Date:** YYYY-MM-DD
**Decision:** What we chose
**Reasoning:** Why this option over the others
**Alternatives considered:** What was rejected and why
```

---

### DEC-005: Exclude Handsfree (HFP) service from toggle
**Date:** 2026-04-09
**Decision:** Only toggle A2DP Sink and A2DP Source. Never toggle Handsfree.
**Reasoning:** Toggling Handsfree re-enables the Bluetooth mic. Windows then switches from A2DP (stereo, high quality) to HFP (mono, 8kHz) in any app that requests mic access. User had already disabled Handsfree in Windows Bluetooth settings to avoid this.
**Alternatives considered:**
- **Toggle all three services** — rejected. Tested, re-activated mic and caused low-quality audio in games.

---

### DEC-004: C++ N-API addon for Stream Deck plugin Bluetooth calls
**Date:** 2026-04-08
**Decision:** Write a C++ N-API addon wrapping BluetoothSetServiceState, compiled with node-gyp/MSVC.
**Reasoning:** Stream Deck SDK v2 uses Node.js. Need to call native Windows Bluetooth APIs from JS. N-API is the official Node.js native addon interface — stable across versions, no third-party dependency.
**Alternatives considered:**
- **Shell out to .exe** — rejected. User preferred clean single-process architecture.
- **node-ffi-napi** — rejected. Community project, unmaintained fork, fragile.
- **edge-js (.NET bridge)** — rejected. Adds native module dependency, fragile with Stream Deck's bundled Node.
- **Koffi** — rejected. Newer FFI lib but still community, not official.

---

### DEC-003: BluetoothSetServiceState P/Invoke for connect/disconnect
**Date:** 2026-04-08
**Decision:** Use native `BluetoothSetServiceState` via P/Invoke to toggle A2DP/Handsfree services on the device.
**Reasoning:** WinRT `BluetoothDevice` API can discover and query devices but has no method to disconnect Bluetooth Classic audio. Disposing the WinRT device object and forcing GC does not actually disconnect. The native Windows Bluetooth API (`BluetoothApis.dll`) provides `BluetoothSetServiceState` which enables/disables specific services — this triggers a real OS-level connect/disconnect without unpairing.
**Alternatives considered:**
- **WinRT dispose + GC** — rejected. Tested, does not disconnect audio devices despite being commonly suggested online.
- **DeviceInformation enable/disable** — rejected. Equivalent to Disable-PnpDevice, removes device from system.

---

### DEC-002: Dev Environment — Windows-side .NET SDK called from WSL
**Date:** 2026-04-08
**Decision:** Install .NET SDK on Windows, call `dotnet.exe` from WSL. Source stays on WSL filesystem.
**Reasoning:** Avoids flaky .NET-in-WSL installs (apt repo missing for Ubuntu Noble, script installer fragile). Since the app must run on Windows anyway (Bluetooth APIs), using the Windows SDK is the natural fit. Source stays in WSL to avoid /mnt/c/ permission issues.
**Alternatives considered:**
- **.NET SDK in WSL via apt** — rejected. Package not in Ubuntu Noble default repos, install failed.
- **.NET SDK in WSL via install script** — rejected. Fragile, adds another moving part for no gain.
- **Develop entirely on Windows side** — rejected. Git and tooling already set up in WSL, /mnt/c/ has known permission issues.

---

### DEC-001: Initial Stack Choice
**Date:** 2026-04-08
**Decision:** C# / .NET 10 LTS — Windows.Devices.Bluetooth API built-in, zero third-party dependencies
**Reasoning:** The project needs direct Bluetooth Classic control (connect/disconnect) on Windows. .NET 10 (current LTS, supported until Nov 2028) provides first-party access to the Windows.Devices.Bluetooth WinRT API without any external libraries. .NET 8 was originally chosen but is nearing end-of-life (Nov 2026).
**Alternatives considered:**
- **.NET 8 LTS** — rejected. Expires Nov 2026, only ~7 months of support left. No reason to start a new project on it.
- **.NET 11 preview** — rejected. Bleeding edge, not production-ready.
- **Python with pybluez2** — rejected. Poor Windows support; it wraps the legacy Windows Bluetooth stack and is unreliable on modern Windows.
- **PowerShell via Disable-PnpDevice** — rejected. This removes the device from the system entirely, which breaks Bluetooth multipoint pairing. We need connect/disconnect, not enable/disable.

---
