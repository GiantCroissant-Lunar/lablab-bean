# Agent Instructions System

Multi-agent instruction system for Lablab-Bean project.

## Structure

```
.agent/
├── README.md              # This file
├── base/                  # Canonical rules (source of truth)
│   ├── 00-index.md       # Version 1.0.0, structure, naming conventions
│   ├── 10-principles.md  # Core principles for development
│   ├── 20-rules.md       # Normative rules with IDs
│   ├── 30-glossary.md    # Domain terminology
│   └── 40-documentation.md # Documentation standards
├── adapters/             # Agent-specific implementations
│   ├── claude.md         # Claude Code (v1.2.0, with Spec-Kit)
│   ├── copilot.md        # GitHub Copilot (v1.1.0, with Spec-Kit)
│   ├── windsurf.md       # Windsurf/Codeium (v1.1.0, with Spec-Kit)
│   └── codex.md          # OpenAI Codex (v1.0.0, with Spec-Kit)
├── integrations/         # External tool integrations
│   └── spec-kit.md       # GitHub Spec-Kit (SDD methodology)
└── meta/                 # Versioning and governance
    ├── changelog.md      # Version history
    ├── versioning.md     # Version sync protocol
    └── adapter-template.md # Template for new adapters
```

## Pointer Files

Agents discover rules via pointer files:

- `/CLAUDE.md` → `.agent/adapters/claude.md`
- `/.github/copilot-instructions.md` → `.agent/adapters/copilot.md`
- `/.windsurf/rules.md` → `.agent/adapters/windsurf.md`
- `/.codex/rules.md` → `.agent/adapters/codex.md` (planned)

## Rule Categories

- **R-CODE-xxx**: Code quality & architecture
- **R-SEC-xxx**: Security guidelines
- **R-TST-xxx**: Testing standards
- **R-DOC-xxx**: Documentation conventions
- **R-GIT-xxx**: Git workflow
- **R-PRC-xxx**: Process guidelines
- **R-TOOL-xxx**: Tool integration (e.g., Spec-Kit)

## Version Sync

All adapters declare `Base-Version-Expected: 1.0.0` and must sync with base version in `base/00-index.md`.

## Adding a New Agent

1. Copy `.agent/meta/adapter-template.md`
2. Fill in agent-specific details
3. Save to `.agent/adapters/{agent-name}.md`
4. Create pointer file in appropriate location
5. Update `Base-Version-Expected` to current version

## Modifying Rules

1. Update rule in `.agent/base/20-rules.md`
2. Update version in `.agent/base/00-index.md` (following semver)
3. Document change in `.agent/meta/changelog.md`
4. Never reuse or renumber existing rule IDs

## Design Philosophy

**Pragmatic over Perfect**: Rules focus on security, quality, and maintainability without blocking rapid development.

**Documentation-First**: Comprehensive documentation is a requirement, not an afterthought.

**Version Stability**: Rule IDs are immutable. Deprecated rules are marked but never removed.

**Adapter Composability**: Adapters reference rule IDs rather than duplicating text, ensuring consistency.

## Current Version

**1.0.0** - Initial release (2025-10-21)

See `.agent/meta/changelog.md` for version history.
