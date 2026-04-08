# Project Status — leides-ljuvliga-lilla-bt-toggle

> **Last updated:** 2026-04-08
> **Current sprint:** Sprint 1 – Connect/Disconnect
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
Project builds. Connect/disconnect implemented but **untested** — needs real MAC address and Windows-side testing.

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

### Sprint 1 – Connect/Disconnect (Active)
- [x] Scaffold .NET 10 console project with WinRT Bluetooth APIs
- [x] `bt-toggle connect` — initial implementation (#1)
- [x] `bt-toggle disconnect` — initial implementation (#1)
- [ ] Replace placeholder MAC address with real Sony WH-1000XM5 address
- [ ] Test connect on Windows
- [ ] Test disconnect on Windows

### Sprint 2 – [Name] (Target: TBD)
- [ ] [Feature/task]

---

## Key Metrics to Track
<!-- What does "working" actually mean for this project? Define it here. -->
- TBD

---
> Update this at the **end** of each Claude Code session, not the beginning.
> Move completed tasks to ✅ Done. Keep Blockers current.
