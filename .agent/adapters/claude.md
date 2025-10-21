# Claude Code Adapter

**Base-Version-Expected**: 1.0.0

This adapter configures Claude Code behavior for the Lablab-Bean project.

## Quick Reference

You are working on a dungeon crawler game with:
- **.NET 8 backend**: Console application with Terminal.Gui
- **Web frontend**: xterm.js terminal emulator
- **Process management**: PM2 for running services

## Core Rules to Follow

### Documentation Rules (R-DOC)
- **R-DOC-001**: Write new docs to `docs/_inbox/` only
- **R-DOC-002**: Include YAML front-matter in all docs
- **R-DOC-003**: Check `docs/index/registry.json` before creating new docs
- **R-DOC-004**: Update existing canonical docs instead of duplicating

### Code Rules (R-CODE)
- **R-CODE-001**: No hardcoded secrets
- **R-CODE-002**: Use meaningful names
- **R-CODE-003**: Comment non-obvious code

### Testing Rules (R-TST)
- **R-TST-001**: Test critical paths
- **R-TST-002**: Builds must pass before commit

### Git Rules (R-GIT)
- **R-GIT-001**: Use descriptive commit messages
- **R-GIT-002**: Never commit secrets

### Process Rules (R-PRC)
- **R-PRC-001**: Document architecture decisions as ADRs
- **R-PRC-002**: Document breaking changes

### Security Rules (R-SEC)
- **R-SEC-001**: Validate external input
- **R-SEC-002**: Principle of least privilege

## Documentation Workflow

When creating documentation:

1. **Check registry first**: Review `docs/index/registry.json`
2. **Write to inbox**: Save to `docs/_inbox/YYYY-MM-DD-title--DOC-YYYY-NNNNN.md`
3. **Include front-matter**:
   ```yaml
   ---
   doc_id: DOC-2025-XXXXX
   title: Your Title
   doc_type: guide|spec|adr|rfc|plan|finding|glossary|reference
   status: draft
   canonical: false
   created: 2025-10-21
   tags: [relevant, tags]
   summary: >
     One-line description
   source:
     author: agent
     agent: claude
     model: sonnet-4.5
   ---
   ```

## Principles to Follow

See `.agent/base/10-principles.md` for full list. Key principles:

- **P-1**: Documentation-First Development
- **P-2**: Clear Code Over Clever Code
- **P-3**: Testing Matters
- **P-4**: Security Consciousness
- **P-10**: When in doubt, ask

## Tech Stack

- **Backend**: .NET 8, C#, Terminal.Gui
- **Frontend**: TypeScript, xterm.js, Node.js
- **Process Management**: PM2
- **Build**: npm scripts, dotnet CLI

## File Structure

```
lablab-bean/
├── dotnet/                    # .NET backend
│   ├── console-app/          # Terminal.Gui console app
│   └── framework/            # Game logic libraries
├── website/                   # Web frontend
│   └── packages/terminal/    # xterm.js terminal
├── docs/                      # Documentation
│   ├── _inbox/               # New docs staging
│   ├── guides/               # How-to guides
│   ├── adrs/                 # Architecture decisions
│   └── index/registry.json   # Doc registry
└── .agent/                    # Agent instructions
```

## Common Tasks

### Running the App
```bash
npm run dev          # Start web terminal
npm run console      # Run .NET console app
```

### Building
```bash
npm run build        # Build frontend
dotnet build         # Build backend
```

### Testing
```bash
npm test             # Run frontend tests
dotnet test          # Run backend tests
```

### Documentation
```bash
python scripts/validate_docs.py  # Validate docs
```

## References

- **Base Rules**: See `.agent/base/20-rules.md`
- **Principles**: See `.agent/base/10-principles.md`
- **Glossary**: See `.agent/base/30-glossary.md`
- **Documentation Standards**: See `.agent/base/40-documentation.md`
- **Schema**: See `docs/DOCUMENTATION-SCHEMA.md`

---

**Version**: 1.0.0
**Last Updated**: 2025-10-21
**Sync Status**: ✅ Synced with base rules
