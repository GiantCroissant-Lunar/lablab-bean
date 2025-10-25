---
doc_id: DOC-2025-00016
title: Documentation Quick Reference
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [reference, quickstart, cheatsheet]
summary: Quick reference for the documentation system
source:
  author: agent
  agent: copilot
---

# Documentation Quick Reference

## For AI Agents

### Before Creating a Document

```bash
# Check registry for existing docs
cat docs/index/registry.json | grep -i "topic-name"
```

### Creating a New Document

**Location:** Always write to `docs/_inbox/` first

**Filename:** `YYYY-MM-DD-topic-name--DOC-YYYY-NNNNN.md`

**Next Available ID:** DOC-2025-00017

**Template:**

```yaml
---
doc_id: DOC-2025-00017
title: Your Title Here
doc_type: guide|spec|rfc|adr|plan|finding|glossary|reference
status: draft
canonical: false
created: 2025-10-20
tags: [tag1, tag2, tag3]
summary: >
  One-line description of what this document covers.
source:
  author: agent
  agent: copilot|claude|windsurf|gemini
  model: model-name-optional
---

# Your Title Here

Content goes here...
```

## Document Types

| Type | Use For | Examples |
|------|---------|----------|
| `guide` | How-tos, tutorials | Setup guides, workflows |
| `spec` | Technical specs | API specs, feature specs |
| `rfc` | Proposals | Design proposals |
| `adr` | Architecture decisions | Tech stack choices |
| `plan` | Implementation plans | Roadmaps, milestones |
| `finding` | Research results | Benchmarks, analysis |
| `glossary` | Term definitions | Acronyms, terminology |
| `reference` | Technical reference | Architecture overviews |

## Status Values

- `draft` - Work in progress
- `active` - Current and maintained
- `superseded` - Replaced by newer doc
- `rejected` - Proposal not accepted
- `archived` - Historical reference only

## Validation

```bash
# Validate all documentation
python scripts/validate_docs.py
```

## Registry Location

**Registry:** `docs/index/registry.json`

Current documents: 14 canonical + 2 inbox

## Key Rules

1. ✅ **Write to `_inbox/` first** - Never write directly to canonical locations
2. ✅ **Check registry** - Before creating, check if doc exists
3. ✅ **Update over create** - Prefer updating existing canonical docs
4. ✅ **Include metadata** - Always add source.agent info
5. ✅ **Use next ID** - Increment from highest in registry

## Quick Links

- [Full Schema](DOCUMENTATION-SCHEMA.md)
- [Main README](README.md)
- [Agent Instructions](../AGENTS.md)
- [Registry](..\index\registry.json)
