---
doc_id: DOC-2025-00002
title: Documentation Index
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [readme, navigation]
summary: Central navigation and overview of lablab-bean documentation
source:
  author: agent
  agent: copilot
---

# Lablab-Bean Documentation

Welcome to the lablab-bean project documentation.

## 📚 Documentation Sections

### Getting Started

- **[QUICKSTART.md](../QUICKSTART.md)** - Quick start guide for users
- **[QUICKSTART-DEV.md](../QUICKSTART-DEV.md)** - Developer quick start guide
- **[Migration Guide](MIGRATION.md)** - Migration notes and version upgrades

### Project Management

- **[CHANGELOG.md](../CHANGELOG.md)** - Project changelog
- **[CHANGES.md](../CHANGES.md)** - Detailed change history
- **[RELEASE.md](../RELEASE.md)** - Release process and notes
- **[HANDOVER.md](../HANDOVER.md)** - Project handover documentation

### Development

- **[Architecture Decisions (ADRs)](adrs/)** - Architecture decision records
- **[RFCs](rfcs/)** - Proposals and design documents
- **[Plans](plans/)** - Implementation plans and milestones
- **[Guides](guides/)** - How-tos and tutorials

### Reference

- **[Documentation Schema](DOCUMENTATION-SCHEMA.md)** - Documentation structure and standards
- **[Glossary](glossary/)** - Term definitions
- **[Findings](findings/)** - Research and analysis results

## 📁 Documentation Structure

```
docs/
├── README.md              ← You are here
├── DOCUMENTATION-SCHEMA.md
│
├── _inbox/                ← New docs land here first
├── adrs/                  ← Architecture Decision Records
├── rfcs/                  ← Request for Comments
├── plans/                 ← Implementation plans
├── findings/              ← Research and benchmarks
├── guides/                ← How-tos and tutorials
├── glossary/              ← Term definitions
├── specs/                 ← Technical specifications
├── archive/               ← Superseded documents
└── index/                 ← Machine-readable registry
    └── registry.json
```

## 🔍 Finding What You Need

### I want to...

- **Understand the project structure** → See main [README.md](../README.md)
- **Get started as a user** → See [QUICKSTART.md](../QUICKSTART.md)
- **Set up development environment** → See [QUICKSTART-DEV.md](../QUICKSTART-DEV.md)
- **Understand architecture decisions** → See [ADRs](adrs/)
- **Propose a change** → See [RFCs](rfcs/)
- **Learn how to do something** → See [Guides](guides/)

## 📝 Contributing Documentation

### For AI Agents

1. **Always write to `docs/_inbox/` first**
2. **Include complete front-matter** (see [DOCUMENTATION-SCHEMA.md](DOCUMENTATION-SCHEMA.md))
3. **Check registry** at `docs/index/registry.json` before creating new docs
4. **Update existing canonical docs** instead of creating duplicates

### For Humans

1. Review [DOCUMENTATION-SCHEMA.md](DOCUMENTATION-SCHEMA.md) for standards
2. Place new docs in appropriate category folder
3. Include required front-matter metadata
4. Update this README if adding new top-level sections

## 📞 Getting Help

- **Main README**: [../README.md](../README.md)
- **Issues**: Create an issue in the repository
- **Documentation Standards**: [DOCUMENTATION-SCHEMA.md](DOCUMENTATION-SCHEMA.md)

---

**Navigation**: [← Back to Project Root](../README.md)
