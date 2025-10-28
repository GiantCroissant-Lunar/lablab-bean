---
title: Root Documentation Files Organization
date: 2025-10-24
doc_id: DOC-2025-00045
status: organizing
category: documentation
---

## Root Documentation Files Organization

### Files Being Organized

Moving scattered documentation files from root directory to proper locations:

### Agent/System Documentation

- `AGENTS.md` → `docs/guides/` (agent system overview)
- `CLAUDE.md` → `docs/guides/` (Claude-specific instructions)

### Project History & Summaries

- `HIERARCHICAL_DI_COMPLETE.md` → `docs/archive/` (completed project summary)
- `PHASE_*_IMPLEMENTATION.md` → `docs/archive/phases/` (implementation logs)
- `PHASE_*_SUMMARY.md` → `docs/archive/phases/` (phase summaries)

### User Guides

- `LEADERBOARD_UI_GUIDE.md` → `docs/guides/` (UI guide)

### Project Metadata

- `CHANGELOG.md` → Keep in root (standard location)

## Organization Strategy

Following R-DOC-001: New docs go to `docs/_inbox/` first, then organized into proper structure.

## ✅ Completed Organization

### Moved to `docs/guides/`

- `AGENTS.md` → `docs/guides/agent-system-overview.md`
- `CLAUDE.md` → `docs/guides/claude-instructions.md`
- `LEADERBOARD_UI_GUIDE.md` → `docs/guides/leaderboard-ui-guide.md`

### Moved to `docs/archive/`

- `HIERARCHICAL_DI_COMPLETE.md` → `docs/archive/hierarchical-di-complete.md`

### Moved to `docs/archive/phases/`

- All `PHASE_*_IMPLEMENTATION.md` files
- All `PHASE_*_SUMMARY.md` files

### Kept in Root

- `CHANGELOG.md` (standard location for changelogs)
- `README.md` (project entry point)

## Result

✅ Root directory is now clean with only essential project files
✅ Documentation is properly categorized by type and purpose
✅ Historical implementation phases are archived for reference
✅ User guides are easily discoverable in the guides directory
