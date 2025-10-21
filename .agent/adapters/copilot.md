# GitHub Copilot Adapter

**Base-Version-Expected**: 1.0.0

This adapter configures GitHub Copilot behavior for the Lablab-Bean project.

## Quick Reference

You are working on a dungeon crawler game with:
- **.NET 8 backend**: Console application with Terminal.Gui
- **Web frontend**: xterm.js terminal emulator
- **Process management**: PM2 for running services

## Core Rules to Follow

### Documentation Rules (R-DOC)
- **R-DOC-001**: All docs require YAML front-matter
- **R-DOC-002**: Write new docs to `docs/_inbox/` only
- **R-DOC-003**: Check `docs/index/registry.json` before creating new docs
- **R-DOC-004**: Only one canonical doc per concept

### Code Rules (R-CODE)
- **R-CODE-001**: No hardcoded secrets
- **R-CODE-002**: Use meaningful names
- **R-CODE-003**: Comment non-obvious code

### Testing Rules (R-TST)
- **R-TST-001**: Test critical paths
- **R-TST-002**: Builds must pass

### Git Rules (R-GIT)
- **R-GIT-001**: Descriptive commit messages
- **R-GIT-002**: Never commit secrets

### Security Rules (R-SEC)
- **R-SEC-001**: Validate external input
- **R-SEC-002**: Principle of least privilege

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
│   └── index/registry.json   # Doc registry
└── specs/                     # Spec-Kit specifications
```

## Copilot-Specific Notes

When generating code suggestions:

1. **Context Awareness**: Consider surrounding code patterns and project conventions
2. **Documentation**: Include JSDoc/XML comments for public APIs
3. **Type Safety**: Prefer strongly-typed solutions in both C# and TypeScript
4. **Error Handling**: Include appropriate try-catch blocks and null checks
5. **Testing**: Suggest test cases when generating new functions

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
