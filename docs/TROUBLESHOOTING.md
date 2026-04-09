# Troubleshooting — leides-ljuvliga-lilla-bt-toggle

Known issues and solutions. Check here before debugging. Add here when you fix something.

---

## Format
```
### Issue title
**Symptom:** What you observed
**Cause:** Why it happened
**Solution:** What fixed it
```

---

## WSL2 / Environment

### WSL2: permission errors on /mnt/c/
**Symptom:** Permission errors running scripts or tools on files under `/mnt/c/`.
**Cause:** Windows filesystem mounted at `/mnt/c/` doesn't support Linux permissions.
**Solution:** Keep the project on the native WSL filesystem (`~/projects/`). Only use `/mnt/c/` for dropping files from Windows.

### git init fails or behaves unexpectedly
**Symptom:** `git init` or git operations fail on `/mnt/c/`.
**Cause:** Same filesystem permission issue as above.
**Solution:** Keep the git repo on native WSL: `~/projects/leides-ljuvliga-lilla-bt-toggle`.

### Python venv fails
**Symptom:** `python -m venv` fails or venv doesn't work.
**Cause:** Symlinks and permissions broken on mounted Windows filesystem.
**Solution:** Create venv on native WSL: `python3 -m venv ~/venv-leides-ljuvliga-lilla-bt-toggle`

### Shell glob patterns fail on /mnt/c/ (e.g. git add *.md)
**Symptom:** `git add *.md` or similar glob commands silently do nothing or error on `/mnt/c/`.
**Cause:** Bash glob expansion behaves differently on the Windows-mounted filesystem. If no files match, the literal `*.md` is passed to git, which fails.
**Solution:** Use `git add -A` (stages everything, relies on `.gitignore`) instead of individual glob patterns. The project's `commit.sh` already does this.

---

## Claude Code

### Claude Code auto-update fails on startup
**Symptom:** Warning on startup that Claude Code failed to auto-update.
**Cause:** Global npm packages need sudo; auto-updater doesn't use it.
**Solution:** `sudo npm install -g @anthropic-ai/claude-code`
Not critical — Claude Code still works, it's just a warning.

### Claude Code forgets to update PROJECT_STATUS.md
**Symptom:** Tasks get done but PROJECT_STATUS.md stays stale.
**Cause:** Prompt-based rules in CLAUDE.md get missed when Claude is focused on code.
**Solution:** Don't rely on prompts — enforce with tooling. Use a `commit.sh` script
or git hooks that check documentation is updated before pushing.
**General principle:** If something needs to happen every time, automate it. Never rely on Claude remembering.

---

## Git / GitHub

### npm global install needs sudo
**Symptom:** `npm install -g` fails with permission errors.
**Solution:** `sudo npm install -g <package>`

---
## Bluetooth

### Headphones switch to low-quality mono audio after connect
**Symptom:** After bt-toggle connect, audio quality drops in games/apps that use mic.
**Cause:** Toggling the Handsfree (HFP) Bluetooth service re-enables the mic. Windows then switches from A2DP (stereo, high quality) to HFP (mono, low quality) when an app requests mic access.
**Solution:** Only toggle A2DP Sink and A2DP Source services. Never toggle Handsfree. Fixed in bluetooth.cc and Program.cs by removing HANDSFREE_SERVICE from the services array.

---

## Stream Deck Plugin

### Plugin shows yellow exclamation mark / "has no attached client"
**Symptom:** Plugin appears in action list but crashes on press, shows yellow exclamation mark.
**Cause:** `"NodeJs"` in manifest.json (capital S) — Stream Deck expects `"Nodejs"` (lowercase s) and silently fails to launch the Node process.
**Solution:** Use `"Nodejs": { "Version": "20" }` in manifest.json.

### install.ps1 fails with "Access to bluetooth.node is denied"
**Symptom:** `Remove-Item` fails when reinstalling the plugin.
**Cause:** Stream Deck has the `.node` native addon file locked while running.
**Solution:** Quit Stream Deck first (tray icon > Quit), then run install.ps1, then reopen Stream Deck.

### node-gyp fails with "EPERM mkdir C:\Windows\build"
**Symptom:** `node-gyp rebuild` fails when run from WSL.
**Cause:** UNC paths (`\\wsl.localhost\...`) default to `C:\Windows` as working directory, where node-gyp can't write.
**Solution:** Use `build-native.ps1` which copies to a Windows temp directory before building.

### How to rebuild and reinstall the plugin
**Steps:**
1. Quit Stream Deck (tray icon > Quit)
2. Open PowerShell on Windows
3. `cd \\wsl.localhost\Ubuntu-24.04\home\niklas\projects\leides-ljuvliga-lilla-bt-toggle\plugin`
4. `.\build-native.ps1`
5. `.\install.ps1`
6. Reopen Stream Deck

**Note:** The scripts are in `plugin/`, not the repo root. The WSL distro is `Ubuntu-24.04`, not `Ubuntu`.

---

## C# / .NET 10 (Windows.Devices.Bluetooth API) / App

_Add issues here as you encounter them._

---
