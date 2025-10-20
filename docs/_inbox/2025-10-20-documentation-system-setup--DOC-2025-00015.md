---
doc_id: DOC-2025-00015
title: Documentation System Setup
doc_type: finding
status: draft
canonical: false
created: 2025-10-20
tags: [documentation, setup, meta]
summary: Summary of the documentation management system setup inspired by sango-card
source:
  author: agent
  agent: copilot
---

# Documentation System Setup

## Overview

This document describes the documentation management system that was set up for the lablab-bean project, inspired by the sango-card project's approach.

## What Was Implemented

### 1. Structured Documentation Folders

Created the following directory structure under `docs/`:

- `_inbox/` - Landing zone for new documentation from AI agents
- `adrs/` - Architecture Decision Records
- `rfcs/` - Request for Comments (proposals)
- `plans/` - Implementation plans and milestones
- `findings/` - Research, benchmarks, and analysis
- `guides/` - How-tos, tutorials, and guides
- `specs/` - Technical specifications
- `glossary/` - Term definitions
- `archive/` - Superseded and archived documents
- `index/` - Registry and indexing metadata

### 2. Documentation Schema

Created `DOCUMENTATION-SCHEMA.md` that defines:

- Required YAML front-matter fields for all docs
- Document types and their purposes
- Status lifecycle (draft → active → superseded/rejected/archived)
- Canonical document rules (only one canonical per concept)
- File naming conventions
- Agent instructions for creating documentation

### 3. YAML Front-Matter

All documentation files now include structured metadata:

```yaml
---
doc_id: DOC-YYYY-NNNNN
title: Short descriptive title
doc_type: spec|rfc|adr|plan|finding|guide|glossary|reference
status: draft|active|superseded|rejected|archived
canonical: true|false
created: YYYY-MM-DD
tags: [tag1, tag2]
summary: >
  One-line description
---
```

### 4. Documentation Registry

Created `docs/index/registry.json` that contains:

- Machine-readable index of all documentation
- Metadata for quick lookups
- Current highest doc_id for increment tracking
- Statistics by type and status

### 5. Validation Script

Created `scripts/validate_docs.py` that validates:

- Required front-matter fields presence
- Valid doc_type and status values
- Canonical uniqueness (no duplicate canonical docs)
- YAML syntax correctness

### 6. Agent Instructions

Updated `AGENTS.md` with documentation rules:

- R-DOC-001: Inbox-First Writing
- R-DOC-002: Mandatory Front-Matter
- R-DOC-003: Registry-First Search
- R-DOC-004: Update Over Create

### 7. Updated Existing Documentation

Added front-matter to all existing documentation files:

- DOC-2025-00001 through DOC-2025-00014 assigned
- All docs now have proper metadata
- Ready for canonical document management

## Benefits

### For AI Agents

1. **Clear guidelines** on where to write documentation
2. **Structured metadata** that makes documents discoverable
3. **Registry check** prevents duplicate documentation
4. **Inbox workflow** allows human review before canonization

### For Humans

1. **Organized structure** makes finding docs easier
2. **Version tracking** through supersedes chains
3. **Status clarity** through lifecycle stages
4. **Quality control** through inbox review process

### For the Project

1. **Prevents doc sprawl** by enforcing update-over-create
2. **Maintains history** through archive and supersedes
3. **Enables automation** through structured metadata
4. **Improves discoverability** through tags and registry

## Comparison to Sango-Card

### Adopted from Sango-Card

- ✅ Structured folder hierarchy
- ✅ YAML front-matter schema
- ✅ Documentation registry (registry.json)
- ✅ Inbox-first workflow for agents
- ✅ Canonical document rules
- ✅ Validation script concept

### Simplified for Lablab-Bean

- 📦 Simpler validation (no simhash/fuzzy matching dependencies)
- 📦 Manual registry updates (no auto-generation yet)
- 📦 No CI/CD integration yet
- 📦 No git hooks integration yet

### Future Enhancements

Potential improvements that could be added:

1. **Auto-generate registry** from front-matter during pre-commit
2. **Simhash duplicate detection** like sango-card
3. **CI/CD validation** in GitHub Actions
4. **Dead link checker** integration
5. **Markdown linting** enforcement

## Usage Example

### For AI Agents Creating New Documentation

1. Check registry: `docs/index/registry.json`
2. Get next doc_id: DOC-2025-00016 (current highest: 00015)
3. Create file: `docs/_inbox/2025-10-20-my-topic--DOC-2025-00016.md`
4. Include full front-matter with source.agent info
5. Human reviews and promotes to canonical location

### For Humans Promoting Documentation

1. Review inbox document for quality
2. Check for duplicates in registry
3. If new concept: move to appropriate folder, set canonical: true
4. If updating existing: merge content into canonical doc
5. Update registry with new entry
6. Run validation: `python scripts/validate_docs.py`

## Validation Test

The validation script was tested and confirms all 14 existing documents are valid:

```
✅ All 14 documents validated successfully!
```

## Next Steps

1. ✅ Documentation system is operational
2. 📋 Agents can now create documentation following the schema
3. 📋 Humans can review and promote from inbox
4. 📋 Consider adding CI/CD validation in future
5. 📋 Consider adding auto-registry generation

## References

- Source project: `D:\lunar-snake\constract-work\card-projects\sango-card`
- Schema: `docs/DOCUMENTATION-SCHEMA.md`
- Agent rules: `AGENTS.md`
- Registry: `docs/index/registry.json`
- Validation: `scripts/validate_docs.py`
