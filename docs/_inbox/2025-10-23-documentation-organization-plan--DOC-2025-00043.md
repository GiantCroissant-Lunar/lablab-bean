---
title: Documentation Organization Plan
date: 2025-10-23
type: organization-plan
status: in-progress
---

# Documentation Organization Plan

## Current State Analysis

### Root Level Documentation (31 files)
**Phase Implementation Files** (should be archived):
- PHASE_3_INTEGRATION_SUMMARY.md
- PHASE_4_IMPLEMENTATION.md, PHASE_4_SUMMARY.md
- PHASE_5_IMPLEMENTATION.md, PHASE_5_SUMMARY.md
- PHASE_6_IMPLEMENTATION.md, PHASE_6_SUMMARY.md
- PHASE_7_IMPLEMENTATION.md, PHASE_7_SUMMARY.md
- PHASE_8_* (9 files)
- PHASE_9_IMPLEMENTATION.md, PHASE_9_SUMMARY.md

**System Documentation** (should stay in root):
- README.md (main project readme)
- CHANGELOG.md (project changelog)
- AGENTS.md, CLAUDE.md (agent instructions)

**Feature Documentation** (should be moved):
- INVENTORY_SYSTEM_COMPLETE.md → docs/archive/implementation-summaries/
- LEADERBOARD_UI_GUIDE.md → docs/guides/
- SPELL_SYSTEM_DECISION.md → docs/adrs/
- WHATS_NEXT.md → docs/plans/

**Temporary Files** (should be cleaned up):
- temp_git_hooks_*.md (3 files)

### Dotnet Directory Documentation (20 files)
**Plugin Documentation** (should stay in place):
- dotnet/plugins/*/README.md (10 files)
- dotnet/plugins/*/ARCHITECTURE_*.md (3 files)
- dotnet/plugins/*/INTEGRATION_*.md (2 files)
- dotnet/plugins/*/INSTALLATION.md (1 file)

**Framework Documentation** (should stay in place):
- dotnet/framework/*/README.md (2 files)
- dotnet/framework/*/JSON_INTEGRATION.md (1 file)
- dotnet/framework/*/UNITY_DESIGN_ADOPTION.md (1 file)

**Implementation Documentation** (should be moved):
- dotnet/DUNGEON_CRAWLER_IMPLEMENTATION.md → docs/archive/implementation-summaries/

## Organization Actions

### 1. Archive Phase Documentation
Move all PHASE_* files to `docs/archive/implementation-summaries/phases/`

### 2. Move Feature Documentation
- INVENTORY_SYSTEM_COMPLETE.md → docs/archive/implementation-summaries/
- LEADERBOARD_UI_GUIDE.md → docs/guides/
- SPELL_SYSTEM_DECISION.md → docs/adrs/
- WHATS_NEXT.md → docs/plans/

### 3. Clean Up Temporary Files
Remove temp_* files from root

### 4. Move Implementation Documentation
- dotnet/DUNGEON_CRAWLER_IMPLEMENTATION.md → docs/archive/implementation-summaries/

### 5. Update References
Update any cross-references in moved files

## Expected Result

**Root Directory**: Clean, only essential project files
- README.md, CHANGELOG.md, AGENTS.md, CLAUDE.md

**docs/ Structure**: Well-organized documentation
- docs/guides/ - User guides and how-tos
- docs/adrs/ - Architecture decisions
- docs/plans/ - Project planning documents
- docs/archive/implementation-summaries/ - Historical implementation docs

**dotnet/ Structure**: Technical documentation stays with code
- Plugin and framework README files remain in place