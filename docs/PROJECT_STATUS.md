# Project Status — leides-ljuvliga-lilla-bt-toggle

> **Last updated:** 2026-04-08
> **Current sprint:** Sprint 2 – Stream Deck Plugin
> **Sprint dates:** 2026-04-08 → TBD

---

## Current Sprint: Sprint 0 – Setup

| # | Task | Status | Notes |
|---|------|--------|-------|
| 0.1 | Define first feature | ✅ Done | bt-toggle connect/disconnect for Sony WH-1000XM5 |
| 0.2 | Create first GitHub issue | ✅ Done | #1 |

**Status legend:** ⬜ Todo | 🔄 In Progress | ✅ Done | 🚫 Blocked | ⏸️ Paused

---

## What's Working Now
Connect and disconnect working on Sony WH-1000XM5.

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

### Sprint 2 – Stream Deck Plugin (Active)
- [x] C++ N-API addon wrapping BluetoothSetServiceState (#2)
- [x] Stream Deck plugin scaffold (Elgato SDK) (#2)
- [ ] Single toggle button with connect/disconnect (#2)
- [ ] State icons: connected (green), disconnected (grey), error (red) (#2)
- [ ] Auto-detect connection state on load (#2)
- [ ] Status text on button (#2)
- [ ] Property Inspector for MAC address config (#2)

### Sprint 3 – [TBD]
- [ ] Multi-device support
- [ ] Connection event listener

---

## Key Metrics to Track
<!-- What does "working" actually mean for this project? Define it here. -->
- TBD

---
> Update this at the **end** of each Claude Code session, not the beginning.
> Move completed tasks to ✅ Done. Keep Blockers current.
