# leides-ljuvliga-lilla-bt-toggle

CLI-verktyg för att koppla/koppla från Bluetooth-enheter på Windows via Stream Deck

**Stack:** C# / .NET 10 LTS (Windows.Devices.Bluetooth API)
**Started:** 2026-04-08
**GitHub:** github.com/niklasleide/leides-ljuvliga-lilla-bt-toggle

## Session Start — ALWAYS do this first
0. If Stack or Commands say TBD or unfilled — stop and resolve before anything else.
1. Read `@docs/PROJECT_STATUS.md` — understand current state
2. Read `@docs/DECISIONS.md` — don't propose changes that contradict past decisions
3. Check `@docs/TROUBLESHOOTING.md` before proposing solutions to errors

These files ARE Claude's memory between sessions. Keep them accurate.

## Commands
```bash
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe build   # build
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe run -- connect   # run (connect)
cd src && /mnt/c/Program\ Files/dotnet/dotnet.exe run -- disconnect # run (disconnect)
# [test command TBD — add when tests exist]
./commit.sh "message"       # ALWAYS use this to commit — never bare git commit
```

## Commit Rule (non-negotiable)
**Always use `./commit.sh "message"` — never bare `git commit`.**
Before every commit, update:
- `docs/CHANGELOG.md` — always, for every code change
- `docs/PROJECT_STATUS.md` — if any task changed state
- `docs/DECISIONS.md` — if an architectural decision was made
- `docs/TROUBLESHOOTING.md` — if a bug was hit and fixed

## Design System (if applicable)
If this project has a UI, create a design tokens file as single source of truth
for colours, typography, spacing. All components import from it — no hardcoded
values in component files.

## Data Migration (if applicable)
If this project stores data locally, use a schema version number from day one.
Every data structure change gets a migration. Bump schema version with every migration.

## Definition of Done (non-negotiable)
Before calling any feature or fix "done", complete ALL 5:
1. Code works — manually verify the happy path
2. Tests written — new functionality has test coverage
3. All existing tests pass — run the test command, zero failures
4. `docs/CHANGELOG.md` updated — one line per logical change
5. `./commit.sh "message"` run successfully — commit with docs included

Never say "done" until all 5 are complete.

## What Claude Gets Wrong on This Project
<!-- Update this as you discover patterns — highest-value section -->
- Forgets to update docs — enforced by commit.sh, but verify before committing
- Says "done" before verifying — always run tests/type-check before declaring done
- Burns tokens on planning when task is already scoped — just execute
- Creates giant files (>300 lines) — propose a split before implementing
- Drifts from visual specs over multiple sprints — use design tokens file as code
