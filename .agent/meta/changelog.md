# Agent Rules Changelog

All notable changes to the agent rules will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-21

### Added

- Initial release of agent instruction system
- Base rules structure (00-index.md, 10-principles.md, 20-rules.md, 30-glossary.md, 40-documentation.md)
- Claude Code adapter
- Documentation rules (R-DOC-001 through R-DOC-005)
- Code rules (R-CODE-001 through R-CODE-003)
- Testing rules (R-TST-001 through R-TST-002)
- Git rules (R-GIT-001 through R-GIT-002)
- Process rules (R-PRC-001 through R-PRC-002)
- Security rules (R-SEC-001 through R-SEC-002)
- Ten core principles (P-1 through P-10)
- Enhanced validation script with registry generation and duplicate detection
- Meta documentation for versioning and governance

### Documentation

- Created comprehensive documentation schema
- Established inbox-first workflow
- Registry-based duplicate detection
- Front-matter validation

---

## Version Guidelines

### Major Version (X.0.0)

- Breaking changes to existing rules
- Removal of rules
- Fundamental changes to rule semantics
- Changes requiring adapter updates

### Minor Version (0.X.0)

- New rules added
- Non-breaking clarifications
- New principles added
- Documentation improvements

### Patch Version (0.0.X)

- Typo fixes
- Minor clarifications
- Documentation formatting
- No semantic changes

---

## Rule ID Policy

- Rule IDs are **immutable** once published
- Never reuse or renumber rule IDs
- Deprecated rules stay in place with "DEPRECATED" marker
- New rules get new sequential IDs
