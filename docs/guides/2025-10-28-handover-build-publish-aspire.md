---
doc_id: DOC-2025-00077
title: "Handover — Build/Publish pipeline and Aspire AppHost"
doc_type: guide
status: active
canonical: true
created: 2025-10-28
tags: [build, publish, aspire, apphost, pipeline]
summary: Summary of changes, current status, and next steps for the .NET build/publish pipeline and Aspire AppHost orchestration
---

# Handover — Build/Publish pipeline and Aspire AppHost

This document summarizes the changes, current status, and next steps for the .NET build/publish pipeline and the Aspire AppHost orchestration.

## What changed this session

- **Taskfile fixes** (`Taskfile.yml`)
  - Restored/fixed tasks: `docs:status`, `dotnet:format`, `format`, `analyze`, `dev`.
  - Removed stray placeholders and normalized YAML so pre-commit hooks pass.
  - `dev` now sets `DOTNET_ASPIRE_DASHBOARD_ENABLED=1` (parity with `aspire:*`).

- **Solution sanitation** (`dotnet/LablabBean.sln`)
  - Cleaned duplicate/malformed sections that caused `MSB5023` and `MSB5009`.
  - Created helper scripts:
    - `scripts/fix-sln.ps1` — removes bad `NestedProjects` blocks and duplicates (e.g., solution folder "tests").
    - `scripts/add-apphost-to-sln.ps1` — ensures `LablabBean.AppHost` project entry exists.
  - The script backs up to `dotnet/LablabBean.sln.bak` on run.

- **NUKE build hardening** (`build/nuke/Build.cs`)
  - `Restore` now attempts solution restore first, then falls back to per-project restores for console/windows if needed.
  - `PublishAll` behavior unchanged (publishes Console, Plugins, and Windows app into `publish/`).

- **Terminal UI compile fixes**
  - `dotnet/console-app/LablabBean.Game.TerminalUI/TerminalUiAdapter.cs`: added `using LablabBean.Contracts.Game.UI.Services;` to resolve `IActivityLog`.
  - `dotnet/plugins/LablabBean.Plugins.UI.Terminal/TerminalUiPlugin.cs`: replaced invalid `with` expressions on a non-record class (`TerminalRenderStyles`) with direct property assignments.

- **Commit + merge**
  - Feature commit: `426053b` fix(build): stabilize PublishAll and Taskfile; sanitize .sln; fix Terminal UI compile
  - Merged into `main` with `a7e98cd` and pushed to origin.
  - A temporary stash with unrelated changes was created to keep the commit focused (see Stash section below).

## Current status

- `Taskfile.yml` passes pre-commit YAML checks.
- `.sln` is sanitized; helper scripts available if issues reappear.
- `PublishAll` runs through Console and most plugins; Terminal UI 'with' issue was fixed; re-run publish if needed.
- Aspire AppHost is integrated and runnable via `task dev`.

## Producing artifacts

- **Console + Plugins + Windows app**
  - Taskfile:

    ```powershell
    task publish
    ```

  - NUKE direct:

    ```powershell
    pwsh -NoProfile -ExecutionPolicy Bypass -File build/nuke/build.ps1 --target PublishAll --configuration Debug
    ```

  - Output: `build/_artifacts/<SemVer>/publish/console` and `publish/windows`

- **NuGet packages**
  - Taskfile (runs `SyncNugetLocal` which depends on `Pack`; sync is skipped if not configured):

    ```powershell
    task nuget:pack
    ```

  - NUKE direct:

    ```powershell
    pwsh -NoProfile -ExecutionPolicy Bypass -File build/nuke/build.ps1 --target Pack --configuration Release
    ```

  - Output: `build/_artifacts/<SemVer>/nuget/*.nupkg`

- **Full release bundle**
  - Taskfile:

    ```powershell
    task release
    ```

  - Includes: publish outputs, NuGet packages, `version.json`, and optional website copy (requires web build in `website/apps/web/dist`).

## Running the Aspire AppHost (dev)

- Taskfile (dashboard enabled):

  ```powershell
  task dev
  ```

- With WezTerm auto-launch from latest artifacts:

  ```powershell
  task aspire:wezterm:auto
  ```

- Environment variables honored:
  - `NUKE_TARGET` (default `Compile` for `dev`, `PublishAll` for `aspire:*`)
  - `ASPIRE_LAUNCH_WEZTERM=1` to enable launcher
  - `LABLAB_WEZTERM_PATH` for portable WezTerm path (defaults to `tools/wezterm/wezterm.exe` if present)
  - `LABLAB_ARTIFACT_DIR` auto-detected when using `aspire:wezterm:auto`

## Troubleshooting

- **Solution errors (MSB5023/MSB5009)**
  - Run `scripts/fix-sln.ps1` to sanitize; if AppHost is missing in `.sln`, run `scripts/add-apphost-to-sln.ps1`.

- **Taskfile issues**
  - Validate: `task --list`
  - Pre-commit YAML: `pre-commit run check-yaml -a` and `pre-commit run pretty-format-yaml -a`

- **Docs organization**
  - The repo uses an organizer tool. If pre-commit flags scattered docs, run:

    ```powershell
    python git-hooks/checks/python/organize_docs.py --auto-move
    ```

## Useful paths

- Build scripts: `build/nuke/Build.cs`
- Task runner: `Taskfile.yml`
- Solution helpers: `scripts/fix-sln.ps1`, `scripts/add-apphost-to-sln.ps1`
- AppHost project: `dotnet/apphost/LablabBean.AppHost/`
- Console app: `dotnet/console-app/LablabBean.Console/`
- Plugins: `dotnet/plugins/`
- Publish root: `build/_artifacts/<SemVer>/publish/`
- NuGet output: `build/_artifacts/<SemVer>/nuget/`

## Stash created in this session

A temporary stash was created to keep the commit minimal:

```powershell
git stash list
# Look for: On 024-terminal-font-bundling: temp stash for commit

git stash show -p stash@{0}

git stash pop stash@{0}
```

This stash contains newly added docs and apphost files not included in the targeted commit.

## Next steps for the new session

- **Publish artifacts**: `task publish` and verify `LablabBean.Console.exe` under `publish/console`.
- **Build NuGets**: `task nuget:pack` (or NUKE `Pack`) and confirm `.nupkg` under `nuget/`.
- **Run Aspire dev**: `task dev` and verify ports 3000 (web) and 3001 (PTY WS) respond.
- **Optional**: `task aspire:wezterm:auto` to auto-open console from the latest artifact.
- **Optional**: `task release` to assemble a complete bundle + metadata.

## Backlog / nice-to-haves

- Add a dedicated NUKE target `PublishAppHost` if packaging AppHost artifacts is desired (currently run-only).
- Unit tests around `.sln` sanitation to guard against reintroducing malformed sections.
- Reduce analyzer warnings (Meziantou) in Terminal UI and plugins.
