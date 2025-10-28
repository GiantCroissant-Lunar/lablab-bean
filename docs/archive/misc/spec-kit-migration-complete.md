---
doc_id: DOC-2025-00036
title: Migration to Real Spec-Kit Complete
doc_type: summary
status: active
canonical: false
created: 2025-10-21
tags: [spec-kit, migration, sdd, ai-development]
summary: >
  Summary of migration from fake template-based "spec-kit" to real
  GitHub Spec-Kit for Specification-Driven Development (SDD).
---

# Migration to Real Spec-Kit Complete

**Date:** 2025-10-21
**Status:** ✅ Complete
**Migration Time:** ~30 minutes

## What Was Done

### 1. Removed Fake "Spec-Kit" Implementation

The project previously had a **Handlebars-based template code generator** incorrectly named "spec-kit". This was completely removed:

#### Files Deleted

- `scripts/generate-template.js` (397 lines)
- `scripts/package.json`
- `scripts/node_modules/`
- `scripts/README.md` (485 lines)
- `templates/entity/monster.tmpl`
- `templates/docs/`
- `vars/examples/` (3 YAML files)
- `vars/README.md`
- `generated/` (4 test outputs)
- `SPEC-KIT.md` (537 lines)
- `SPEC-KIT-QUICK-REFERENCE.md`

#### Documentation Archived

- `docs/guides/spec-kit-quickstart.md` → `docs/archive/`
- `docs/guides/spec-kit-utilization.md` → `docs/archive/`
- `docs/_inbox/spec-kit-implementation-complete.md` → `docs/archive/fake-spec-kit-implementation.md`

#### Taskfile.yml Cleaned

Removed tasks:

- `speck-init`
- `speck-generate`
- `speck-list`
- `gen-monster`
- `gen-monster-from-env`

#### Configuration Updated

- `.lablab-bean.yaml` - Removed fake spec-kit configuration
- Updated project description

### 2. Installed Real Spec-Kit

**Spec-Kit** is GitHub's Specification-Driven Development (SDD) toolkit.

#### Installation

```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
# Installed version: 0.0.20
```

#### Initialization

```bash
specify init --here --ai claude
# Selected: Claude Code agent
# Script type: PowerShell (Windows)
# Template: v0.0.72
```

#### What Was Created

```
.specify/
├── memory/
│   └── constitution.md          # Project governance principles
├── scripts/
│   └── powershell/              # Helper scripts
│       ├── check-prerequisites.ps1
│       ├── common.ps1
│       ├── create-new-feature.ps1
│       ├── setup-plan.ps1
│       └── update-agent-context.ps1
└── templates/
    ├── agent-file-template.md
    ├── checklist-template.md
    ├── plan-template.md
    ├── spec-template.md
    └── tasks-template.md

.claude/
└── commands/                    # Slash commands for Claude Code
    ├── speckit.analyze.md
    ├── speckit.checklist.md
    ├── speckit.clarify.md
    ├── speckit.constitution.md
    ├── speckit.implement.md
    ├── speckit.plan.md
    ├── speckit.specify.md
    └── speckit.tasks.md
```

## Key Differences

### Old "Spec-Kit" (REMOVED)

**What it was:**

- Handlebars template code generator
- YAML variable files + templates → Generated C# code
- Node.js/JavaScript based

**Purpose:**

- Generate boilerplate C# monster classes
- Wrong approach for data-driven game design

**Example:**

```bash
task gen-monster NAME=Dragon
# Output: generated/Dragon.cs (boilerplate class)
```

### New Spec-Kit (INSTALLED)

**What it is:**

- Specification-Driven Development (SDD) methodology
- AI agents read specs and generate complete features
- Python-based CLI tool

**Purpose:**

- Write specifications that drive AI code generation
- Living documentation
- AI-native development workflow

**Example:**

```bash
/speckit.specify
# Prompt: Create a dungeon crawler with fog of war and monsters

/speckit.plan
# Prompt: Use .NET 8, Terminal.Gui, component-based entities

/speckit.implement
# AI generates complete working feature from specifications
```

## Available Spec-Kit Commands

Now available in Claude Code:

### Core Commands

| Command | Purpose |
|---------|---------|
| `/speckit.constitution` | Create project governance principles |
| `/speckit.specify` | Define feature requirements (WHAT & WHY) |
| `/speckit.plan` | Create technical implementation plan (HOW) |
| `/speckit.tasks` | Generate actionable task breakdown |
| `/speckit.implement` | Execute implementation from tasks |

### Enhancement Commands (Optional)

| Command | Purpose |
|---------|---------|
| `/speckit.clarify` | Ask structured clarifying questions |
| `/speckit.analyze` | Cross-artifact consistency analysis |
| `/speckit.checklist` | Generate quality validation checklists |

## Spec-Kit Workflow

### Traditional Development

```
Idea → Write Code → Debug → Document (maybe)
```

### Spec-Driven Development

```
1. /speckit.constitution → Project principles
2. /speckit.specify → Feature specification (WHAT & WHY)
3. /speckit.clarify → Resolve ambiguities (optional)
4. /speckit.plan → Technical plan (HOW)
5. /speckit.tasks → Task breakdown
6. /speckit.implement → AI generates code
7. Review → Iterate
```

## Benefits for Lablab-Bean

### 1. Data-Driven Design Alignment

Spec-Kit encourages data-driven approaches in specifications:

```markdown
# In spec.md
Monsters are defined in YAML files with stats and behaviors.
Players customize loadouts via JSON configuration.
```

AI generates:

- YAML schemas
- Data loaders
- Component-based entities

**Not** individual C# monster classes!

### 2. Living Documentation

Specifications:

- ARE the documentation
- Generate the code
- Stay in sync automatically
- Version-controlled with code

### 3. AI-Native Development

Built for Claude Code and other AI agents:

- Write intent, not implementation
- Focus on WHAT and WHY
- AI handles the HOW
- Faster iteration

### 4. Constitution-Based Quality

Project principles guide all AI decisions:

```markdown
# .specify/memory/constitution.md

Article I: Data-Driven Design
All game entities MUST be defined in data files.

Article II: Component Composition
No inheritance hierarchies for game entities.

Article III: Test-First Development
All features require tests before implementation.
```

AI respects these principles automatically.

### 5. Specification Versioning

Features get specification branches:

```
specs/
├── 001-dungeon-crawler/
│   ├── spec.md
│   ├── plan.md
│   ├── tasks.md
│   └── data-model.md
├── 002-inventory-system/
│   ├── spec.md
│   └── plan.md
```

Each feature:

- Has its own branch
- Documented requirements
- Technical plan
- Task breakdown

## Next Steps

### 1. Create Project Constitution

```bash
/speckit.constitution

# Prompt:
Create constitution for a dungeon crawler game project focusing on:
- Data-driven entity design (YAML/JSON)
- Component-based architecture
- Test-driven development
- Terminal.Gui rendering best practices
- .NET 8 conventions
```

### 2. Document Existing Features

Convert existing systems to specifications:

```bash
/speckit.specify

# Prompt:
Document existing dungeon generation system with:
- Room-based procedural generation
- Fog of war implementation
- Basic monsters (Goblin, Orc, Troll, Skeleton)
- Turn-based player movement
```

### 3. Use Spec-Kit for New Features

For future features:

```bash
/speckit.specify
# Define new feature (e.g., inventory system)

/speckit.plan
# Specify tech approach

/speckit.tasks
# Generate tasks

/speckit.implement
# AI builds feature
```

## File Cleanup Summary

### Removed (Total: ~5,000 lines)

- 21 files deleted
- 3 directories removed
- 6 documentation files archived

### Added (Spec-Kit)

- 18 new files (templates, scripts, commands)
- 2 directories (`.specify/`, `.claude/`)
- Proper SDD infrastructure

## Migration Impact

### Positive Changes

✅ **Correct methodology** - Real SDD instead of fake templates
✅ **AI-native workflow** - Built for Claude Code
✅ **Data-driven friendly** - Encourages proper game architecture
✅ **Living documentation** - Specs stay synchronized
✅ **Quality framework** - Constitution guides decisions

### What We Lost

❌ Quick boilerplate generation - But this was the **wrong approach** anyway
✅ **Better alternative** - Write data files directly (YAML/JSON)

### No Breaking Changes

- No code impact (fake spec-kit wasn't used in production)
- No build impact
- No runtime impact
- Only development workflow affected

## References

### Spec-Kit Documentation

- **GitHub Repo:** <https://github.com/github/spec-kit>
- **Reference Project:** `ref-projects/spec-kit/`
- **Methodology:** `ref-projects/spec-kit/spec-driven.md`

### Internal Documentation

- **Data-Driven Proposal:** `docs/_inbox/data-driven-monster-approach.md`
- **Adoption Guide:** `docs/_inbox/adopt-real-spec-kit.md`
- **This Document:** `docs/_inbox/spec-kit-migration-complete.md`

## Verification

### Check Installation

```bash
# Verify CLI installed
specify --help

# Check project structure
ls .specify/
ls .claude/commands/

# Verify commands available in Claude Code
# Launch Claude Code and type: /speckit
```

### Expected Output

```
.specify/memory/constitution.md      ✅
.specify/templates/spec-template.md  ✅
.claude/commands/speckit.specify.md  ✅
```

## Security Note

From spec-kit initialization:

```
⚠️  Agent Folder Security
Some agents may store credentials in .claude/
Consider adding .claude/ to .gitignore
```

**Action:** Already in `.gitignore` ✅

## Summary

**Migration:** ✅ Complete
**Old "spec-kit":** ❌ Removed
**Real Spec-Kit:** ✅ Installed
**Status:** Ready to use

**Next Action:**
Use `/speckit.constitution` to create project governance principles.

---

**Migration completed successfully on 2025-10-21**
