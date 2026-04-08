# Changelog — leides-ljuvliga-lilla-bt-toggle

Format: `[YYYY-MM-DD] type: description`
Types: `feat` | `fix` | `refactor` | `docs` | `chore` | `perf`

---

[2026-04-09] docs: park project — Sprint 2 complete, all docs updated
[2026-04-09] chore: remove personal MAC from Property Inspector placeholder
[2026-04-09] fix: remove Handsfree (HFP) service — was re-enabling mic and forcing low-quality mono audio
[2026-04-09] perf: make connect/disconnect async via N-API AsyncWorker, show immediate UI feedback
[2026-04-08] feat: add ws dependency, generate icon SVGs, wire up error state (#2)
[2026-04-08] feat: Stream Deck plugin scaffold with C++ N-API Bluetooth addon (#2)
[2026-04-08] fix: use BluetoothSetServiceState P/Invoke for real connect/disconnect (#1)
[2026-04-08] refactor: move MAC address from hardcoded constant to .env file
[2026-04-08] chore: add .NET bin/obj to .gitignore, remove tracked build artifacts
[2026-04-08] feat: scaffold bt-toggle CLI with connect/disconnect commands (#1)
[2026-04-08] docs: update stack to .NET 10 LTS, add DEC-002 dev environment decision
[2026-04-08] docs: fill in DEC-001 stack decision with alternatives, define first feature
[2026-04-08] chore: project initialized via starter kit
