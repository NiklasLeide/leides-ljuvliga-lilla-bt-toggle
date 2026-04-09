# Project Status — leides-ljuvliga-lilla-bt-toggle

> **Last updated:** 2026-04-09
> **Current sprint:** Parked (Sprint 3 partially complete)
> **Sprint dates:** 2026-04-09

---

## Parked on 2026-04-09:
- **Working:** Stream Deck plugin with configurable button mode (toggle / connect only / disconnect only). Connects/disconnects Sony WH-1000XM5 via A2DP. State icons, auto-detect, Property Inspector config all functional.
- **What's next:** Sprint 3 remainder — multi-device support, connection event listener.
- **Gotchas on return:**
  - Must build native addon from Windows PowerShell in `plugin/` dir — use `.\build-native.ps1` then `.\install.ps1`
  - Navigate via `cd \\wsl.localhost\Ubuntu-24.04\home\niklas\projects\leides-ljuvliga-lilla-bt-toggle\plugin`
  - Must quit Stream Deck before reinstalling plugin (locks .node file)
  - Handsfree (HFP) service is deliberately excluded — re-adding it re-enables mic and causes mono audio
  - manifest.json uses `"Nodejs"` (lowercase s) not `"NodeJs"` — Stream Deck won't launch the plugin otherwise

---

## Sprint 0 – Setup (Done)

| # | Task | Status | Notes |
|---|------|--------|-------|
| 0.1 | Define first feature | ✅ Done | bt-toggle connect/disconnect for Sony WH-1000XM5 |
| 0.2 | Create first GitHub issue | ✅ Done | #1 |

**Status legend:** ⬜ Todo | 🔄 In Progress | ✅ Done | 🚫 Blocked | ⏸️ Paused

---

## What's Working Now
Stream Deck plugin working — button connects/disconnects Sony WH-1000XM5 via A2DP only (no HFP/mic). Button mode configurable in Property Inspector: toggle (default), connect only, or disconnect only.

```bash
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe build          # build
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe run -- connect  # connect
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe run -- disconnect # disconnect
```

---

## Blockers
_None_

---

## Sprint Backlog

### Sprint 1 – Connect/Disconnect (Done)
- [x] Scaffold .NET 10 console project with WinRT Bluetooth APIs
- [x] `bt-toggle connect` — working (#1)
- [x] `bt-toggle disconnect` — working (#1)
- [x] MAC address loaded from .env (gitignored)
- [x] Tested connect on Windows
- [x] Tested disconnect on Windows

### Sprint 2 – Stream Deck Plugin (Done)
- [x] C++ N-API addon wrapping BluetoothSetServiceState (#2)
- [x] Stream Deck plugin scaffold (Elgato SDK) (#2)
- [x] Single toggle button with connect/disconnect (#2)
- [x] State icons: connected (green), disconnected (grey), error (red) (#2)
- [x] Auto-detect connection state on load (#2)
- [x] Status text on button (#2)
- [x] Property Inspector for MAC address config (#2)

### Sprint 3 – Enhancements (In Progress)
- [x] Button mode setting: toggle, connect only, disconnect only
- [ ] Multi-device support
- [ ] Connection event listener

---

## Key Metrics to Track
<!-- What does "working" actually mean for this project? Define it here. -->
- TBD

---
> Update this at the **end** of each Claude Code session, not the beginning.
> Move completed tasks to ✅ Done. Keep Blockers current.
