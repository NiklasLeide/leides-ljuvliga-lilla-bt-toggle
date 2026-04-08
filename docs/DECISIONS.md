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

### DEC-001: Initial Stack Choice
**Date:** 2026-04-08
**Decision:** C# / .NET 8 — Windows.Devices.Bluetooth API built-in, zero third-party dependencies
**Reasoning:** The project needs direct Bluetooth Classic control (connect/disconnect) on Windows. .NET 8 provides first-party access to the Windows.Devices.Bluetooth WinRT API without any external libraries. This keeps the dependency tree empty and avoids wrapper quirks.
**Alternatives considered:**
- **Python with pybluez2** — rejected. Poor Windows support; it wraps the legacy Windows Bluetooth stack and is unreliable on modern Windows.
- **PowerShell via Disable-PnpDevice** — rejected. This removes the device from the system entirely, which breaks Bluetooth multipoint pairing. We need connect/disconnect, not enable/disable.

---
