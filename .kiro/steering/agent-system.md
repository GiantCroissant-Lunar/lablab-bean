---
title: Agent Instruction System Integration
mode: always
---

# Agent System Integration

**→ This project uses a unified agent instruction system located in `.agent/`**

Kiro's steering files work in conjunction with the canonical `.agent/` system. This file serves as a bridge between Kiro's steering mechanism and the project-wide agent rules.

## Quick Reference

- **Kiro Adapter**: See `.agent/adapters/kiro.md` for Kiro-specific instructions
- **Base Rules**: See `.agent/base/20-rules.md` for all canonical rules
- **Principles**: See `.agent/base/10-principles.md` for development principles

## How This Works

### Two-Tier Configuration

1. **Kiro Steering (`.kiro/steering/`)**: Kiro-specific, always-loaded context
   - `product.md` - What we're building
   - `tech.md` - How we're building it
   - `structure.md` - Where things go
   - `agent-system.md` - This file (integration point)

2. **Canonical Rules (`.agent/`)**: Shared across all AI agents
   - Base rules that all agents follow
   - Agent-specific adapters for each AI assistant
   - Integration documentation for external tools

### Integration Strategy

```
Kiro reads steering files → References .agent/adapters/kiro.md → Follows .agent/base/ rules
```

This ensures:
- ✅ Kiro has persistent project context via steering files
- ✅ All agents follow the same base rules
- ✅ No duplication of canonical rules
- ✅ Easy updates (change base rules once, all agents see it)

## Core Rules Quick Reference

All rules are documented in `.agent/base/20-rules.md`. Here's a summary:

### Documentation Rules (R-DOC)
- **R-DOC-001**: Write new docs to `docs/_inbox/` only
- **R-DOC-002**: Include YAML front-matter in all docs
- **R-DOC-003**: Check `docs/index/registry.json` before creating docs
- **R-DOC-004**: Update existing canonical docs instead of duplicating

### Code Rules (R-CODE)
- **R-CODE-001**: No hardcoded secrets or credentials
- **R-CODE-002**: Use meaningful variable and function names
- **R-CODE-003**: Comment non-obvious code
- **R-CODE-004**: 🚨 **ALWAYS use relative paths** - Never absolute paths

### Testing Rules (R-TST)
- **R-TST-001**: Test critical paths
- **R-TST-002**: Builds must pass before commit

### Git Rules (R-GIT)
- **R-GIT-001**: Use descriptive commit messages (conventional commits)
- **R-GIT-002**: Never commit secrets

### Process Rules (R-PRC)
- **R-PRC-001**: Document architecture decisions as ADRs
- **R-PRC-002**: Document breaking changes

### Security Rules (R-SEC)
- **R-SEC-001**: Validate all external input
- **R-SEC-002**: Apply principle of least privilege

### Tool Integration Rules (R-TOOL)
- **R-TOOL-001**: Use Spec-Kit for feature development (REQUIRED)
- **R-TOOL-002**: Use task runner (`task` command) for operations
- **R-TOOL-003**: Follow spec maintenance strategy

## Spec-Kit Integration (REQUIRED)

This project **REQUIRES** GitHub Spec-Kit for feature development.

### Workflow
```
task speckit:specify → task speckit:plan → task speckit:tasks → task speckit:implement
```

Or with slash commands:
```
/speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement
```

**Full Documentation**: See `.agent/integrations/spec-kit.md`

## Development Principles

From `.agent/base/10-principles.md`:

- **P-1**: Documentation-First Development
- **P-2**: Clear Code Over Clever Code
- **P-3**: Testing Matters
- **P-4**: Security Consciousness
- **P-5**: Git as Source of Truth
- **P-6**: Automate Repetitive Tasks
- **P-7**: Fail Fast, Fix Fast
- **P-8**: Respect User Experience
- **P-9**: Continuous Improvement
- **P-10**: When in doubt, ask

## Navigation Guide

### For Kiro to Understand the Project
1. Read `.kiro/steering/product.md` - Product purpose
2. Read `.kiro/steering/tech.md` - Tech stack
3. Read `.kiro/steering/structure.md` - File organization
4. Read `.agent/adapters/kiro.md` - Kiro-specific instructions

### For Detailed Rules
1. Read `.agent/base/20-rules.md` - All enforceable rules with IDs
2. Read `.agent/base/10-principles.md` - Development principles
3. Read `.agent/base/40-documentation.md` - Documentation standards

### For Specific Tasks
- **Feature Development**: See `.agent/integrations/spec-kit.md`
- **Documentation**: See `.agent/base/40-documentation.md` and `docs/DOCUMENTATION-SCHEMA.md`
- **Testing**: See `.agent/base/20-rules.md` (R-TST section)
- **Git Workflow**: See `.agent/base/20-rules.md` (R-GIT section)

## Why This Structure?

### Benefits of Two-Tier System

1. **Separation of Concerns**
   - Steering files: Project-specific context (product, tech, structure)
   - Agent system: Universal rules (code quality, security, process)

2. **DRY Principle**
   - Rules defined once in `.agent/base/`
   - All agents (Claude, Copilot, Windsurf, Gemini, Kiro) reference same rules

3. **Maintainability**
   - Update rules in one place
   - All agents automatically stay in sync

4. **Flexibility**
   - Each agent can have specific adaptations
   - Core rules remain consistent

## Compliance

When working with this project, Kiro MUST:
- ✅ Follow all rules defined in `.agent/base/20-rules.md`
- ✅ Apply principles from `.agent/base/10-principles.md`
- ✅ Use Spec-Kit workflow for features (R-TOOL-001)
- ✅ Write new docs to `docs/_inbox/` (R-DOC-001)
- ✅ Use relative paths only (R-CODE-004)

## Additional Steering Files

Feel free to create additional steering files for domain-specific knowledge:

```
.kiro/steering/
├── api-standards.md       # API design patterns
├── testing-standards.md   # Testing approaches
├── security-policies.md   # Security requirements
└── deployment-workflow.md # Deployment process
```

All custom steering files should reference back to `.agent/base/` for canonical rules.

---

**For complete Kiro configuration, see**: `.agent/adapters/kiro.md`
**For all canonical rules, see**: `.agent/base/20-rules.md`
**For system overview, see**: `.agent/README.md`
