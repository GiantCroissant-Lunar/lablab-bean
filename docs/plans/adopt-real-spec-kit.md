---
doc_id: DOC-2025-00035
title: Adopting Real Spec-Kit Methodology
doc_type: plan
status: draft
canonical: false
created: 2025-10-21
tags: [spec-kit, sdd, ai-development, methodology]
summary: >
  Proposal to adopt GitHub's Spec-Kit methodology for AI-driven
  development based on Specification-Driven Development (SDD).
---

# Adopting Real Spec-Kit Methodology

## What We Discovered

The "spec-kit" referenced in `.lablab-bean.yaml` is NOT the same as GitHub's Spec-Kit project.

**GitHub Spec-Kit** is a Specification-Driven Development (SDD) methodology, not a template code generator.

## What Real Spec-Kit Is

### Core Concept: Specifications Drive AI Code Generation

Instead of writing code, you write **executable specifications** that AI agents convert into working code.

### The Workflow

```bash
# 1. Install Spec-Kit CLI
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

# 2. Initialize project
specify init lablab-bean --ai claude

# 3. Create constitution (project principles)
/speckit.constitution

# 4. Write feature specification
/speckit.specify Create a dungeon crawler game with fog of war, monsters, and turn-based combat

# 5. Create technical plan
/speckit.plan Use .NET 8, Terminal.Gui for rendering, and component-based entity system

# 6. Generate task breakdown
/speckit.tasks

# 7. AI implements from specifications
/speckit.implement
```

## How It Works

### 1. Specifications (WHAT and WHY)

File: `specs/001-dungeon-crawler/spec.md`

```markdown
# Feature: Dungeon Crawler Core Game

## User Stories

### US-001: Player Navigation
As a player, I want to move through a dungeon using arrow keys so I can explore the environment.

Acceptance Criteria:
- Arrow keys move player one tile in the chosen direction
- Movement is blocked by walls
- Movement is turn-based (one move = one turn)

### US-002: Fog of War
As a player, I want unexplored areas to be hidden so exploration feels rewarding.

Acceptance Criteria:
- Only areas within 5 tiles are visible
- Explored areas remain dimly visible
- Unexplored areas are completely black
```

### 2. Technical Plan (HOW)

File: `specs/001-dungeon-crawler/plan.md`

```markdown
# Implementation Plan: Dungeon Crawler

## Technology Stack
- .NET 8 Console Application
- Terminal.Gui for TUI rendering
- Component-based entity system
- Grid-based spatial system

## Architecture
- `Game.Core` - Entity/component system
- `Game.Rendering` - Terminal.Gui integration
- `Game.Data` - Dungeon and entity definitions

## Implementation Phases

### Phase 1: Core Game Loop
1. Initialize Terminal.Gui window
2. Implement turn-based game loop
3. Handle keyboard input

### Phase 2: Player Movement
1. Create Player entity
2. Implement movement component
3. Add collision detection
```

### 3. AI Implementation

Claude Code reads the specs and generates:

- C# classes
- Data models
- Tests
- Documentation

**All driven by the specification, not templates.**

## Benefits for Lablab-Bean

### 1. AI-Native Development

Spec-Kit is designed for AI coding agents:

- Claude Code
- GitHub Copilot
- Cursor
- Windsurf

Your development becomes:

```
Write Spec → AI Generates Code → Review → Iterate
```

### 2. Living Documentation

Specifications ARE the documentation:

- Always up-to-date
- Executable
- Versioned with code

### 3. Data-Driven by Default

Spec-Kit methodology encourages data-driven design:

```markdown
# In spec.md
Monsters are defined in YAML files with stats and behaviors.
```

AI generates:

- YAML schema
- Data loader
- Entity factory

**Not** individual monster C# classes!

### 4. Constitution-Based Development

Project principles guide AI:

File: `.specify/memory/constitution.md`

```markdown
# Lablab-Bean Constitution

## Article I: Data-Driven Design
All game entities MUST be defined in data files (YAML/JSON), not code.

## Article II: Component Composition
No inheritance for game entities. Use component-based architecture.

## Article III: Test-First Development
All features MUST have tests before implementation.
```

AI respects these principles when generating code.

## Migration Path

### Phase 1: Install Spec-Kit

```bash
# Install CLI
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git

# Initialize in current project
cd lablab-bean
specify init --here --ai claude --force
```

This creates:

```
.specify/
├── memory/
│   └── constitution.md
├── scripts/
│   ├── create-new-feature.sh
│   ├── setup-plan.sh
│   └── ...
├── specs/
└── templates/
    ├── spec-template.md
    ├── plan-template.md
    └── tasks-template.md
```

### Phase 2: Create Constitution

```bash
claude  # Launch Claude Code

/speckit.constitution

# Prompt:
Create constitution focused on data-driven game design, component-based
entities, test-driven development, and Terminal.Gui best practices.
```

### Phase 3: Document Existing Systems

Convert existing features to specs:

```bash
/speckit.specify

# Prompt:
Document existing dungeon generation system. It generates room-based
dungeons with corridors, implements fog of war, and has basic monsters
(Goblin, Orc, Troll, Skeleton).
```

AI creates `specs/001-dungeon-system/spec.md` from existing code.

### Phase 4: New Features via Spec-Kit

For new features:

```bash
/speckit.specify

# Prompt:
Add inventory system with equipment slots and item pickup/drop.

/speckit.plan

# Prompt:
Use data-driven item definitions in YAML. Component-based inventory.

/speckit.tasks

/speckit.implement
```

AI generates everything from the spec.

## Comparison

### Old "Spec-Kit" (Our Template Generator)

```bash
# Generate C# code from Handlebars template
task gen-monster NAME=Dragon

# Output: generated/Dragon.cs (boilerplate code)
```

**Purpose:** Code scaffolding
**Approach:** Template expansion
**Result:** Boilerplate C# classes

### Real Spec-Kit (SDD Methodology)

```bash
# Write specification
/speckit.specify Add fire-breathing dragon boss

# AI generates everything
/speckit.implement

# Output: Complete feature with:
# - Data definitions
# - Component classes
# - Tests
# - Integration code
```

**Purpose:** AI-driven development
**Approach:** Specification → AI generation
**Result:** Complete working features

## Recommendation

### Option A: Adopt Real Spec-Kit

**Pros:**

- AI-native development workflow
- Living specifications
- Constitution ensures quality
- Data-driven by design

**Cons:**

- Learning curve
- Requires Python/uv
- Different workflow

### Option B: Rename Our Tool

Keep our template generator but:

- Rename to `lablab-codegen` or `template-gen`
- Remove "spec-kit" references
- Use for config/data file generation only

### Option C: Hybrid Approach

- Adopt real Spec-Kit for feature development
- Keep our template-gen for config files
- Clear separation of concerns

## Next Steps

1. **Decide on approach**
   - Pure Spec-Kit?
   - Hybrid?
   - Just rename our tool?

2. **If adopting Spec-Kit:**

   ```bash
   uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
   specify init --here --ai claude --force
   ```

3. **Update documentation**
   - Clarify what "spec-kit" means in this project
   - Document actual usage

## Resources

- **GitHub Spec-Kit:** <https://github.com/github/spec-kit>
- **Documentation:** <https://github.github.io/spec-kit/>
- **Video Overview:** <https://www.youtube.com/watch?v=a9eR1xsfvHg>
- **SDD Methodology:** `ref-projects/spec-kit/spec-driven.md`

---

**Status:** Awaiting decision on which approach to take.
