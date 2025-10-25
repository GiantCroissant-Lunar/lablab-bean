---
doc_id: DOC-2025-00036
title: Project Specifications Index
doc_type: reference
status: active
canonical: true
created: 2025-10-20
tags: [specifications, index, reference, spec-kit]
summary: >
  Index and overview of all formal specifications for major systems and
  features in the Lablab Bean project.
---

# Project Specifications

This directory contains formal specifications for all major systems and features in the Lablab Bean project.

## Purpose

Specifications serve as:

- 📋 **Single source of truth** for feature behavior
- 📚 **Documentation** for developers and maintainers
- ✅ **Acceptance criteria** for feature completion
- 🔍 **Reference** during code reviews and debugging

## Directory Structure

```
specs/
├── README.md                          # This file
├── dungeon-generation-system.md       # Dungeon generation specification
├── monster-template-example.md        # Monster template usage guide
└── [feature-name].md                  # Additional feature specs
```

## Specification Template

All specifications should follow the standard format defined in:

- `templates/docs/spec-template.tmpl`

Key sections include:

- Overview
- Requirements (Functional & Non-Functional)
- Architecture & Components
- Implementation Details
- Testing & Acceptance Criteria
- Known Issues & Future Enhancements

## Creating New Specifications

### Method 1: Using Spec-Kit (Recommended)

```bash
# Generate from template
task spec-new NAME=feature-name TYPE=game-system

# Edit the generated file
code docs/specs/feature-name.md
```

### Method 2: Manual Creation

1. Copy an existing spec as a starting point
2. Follow the standard format
3. Fill in all required sections
4. Validate YAML frontmatter (if any)

## Existing Specifications

### Game Systems

| Specification | Status | Version | Description |
|--------------|--------|---------|-------------|
| [dungeon-generation-system.md](..\specs\dungeon-generation-system.md) | ✅ Implemented | 0.0.2 | Dungeon generation with rooms and corridors |

### Templates & Examples

| Document | Purpose |
|----------|---------|
| [monster-template-example.md](./monster-template-example.md) | Guide for using monster templates |

## Specification Status Values

| Status | Meaning |
|--------|---------|
| 📝 Draft | Work in progress, not yet finalized |
| 🚧 In Progress | Implementation underway |
| ✅ Implemented | Feature complete and working |
| 🔄 Updated | Specification updated for new version |
| ⚠️ Deprecated | No longer in use, kept for reference |

## Version Numbering

Specifications follow the project version numbering:

- **Major.Minor.Patch** (e.g., 0.0.2)
- Version should match when feature was implemented
- Update version when significant changes occur

## Validation

Before committing specifications:

```bash
# Validate YAML/Markdown format
task validate-yaml
task validate-markdown

# Run all checks
task check
```

## Integration with Development

### During Planning

1. Create specification before implementation
2. Review with team
3. Get approval before coding

### During Implementation

1. Reference specification for requirements
2. Update specification if design changes
3. Check off acceptance criteria as completed

### During Code Review

1. Verify implementation matches specification
2. Check all requirements are met
3. Update specification if deviations are approved

### During Testing

1. Use test cases from specification
2. Verify acceptance criteria
3. Document any issues found

## Linking Specifications

### In Code Comments

```csharp
// See specification: docs/specs/dungeon-generation-system.md
// Implements: REQ-003 (L-shaped corridors)
public void CreateCorridor(Point start, Point end)
{
    // ...
}
```

### In Documentation

```markdown
Refer to [Dungeon Generation Specification](..\specs\dungeon-generation-system.md)
for detailed algorithm description.
```

### In Commit Messages

```
feat: implement FOV calculation

Implements REQ-005 from dungeon-generation-system.md
Radius set to 20 tiles as specified
```

## Templates Available

Located in `templates/docs/`:

- `spec-template.tmpl` - Standard specification template

## Best Practices

### Writing Specifications

1. **Be Specific**: Use concrete requirements, not vague descriptions
2. **Include Examples**: Show expected behavior with examples
3. **Version Everything**: Track changes with version history
4. **Link References**: Reference related specs and external resources
5. **Test Cases**: Include specific test scenarios

### Maintaining Specifications

1. **Keep Updated**: Specifications should reflect current implementation
2. **Track Changes**: Use version history table
3. **Archive Old**: Don't delete, mark as deprecated
4. **Review Regularly**: Check specs during sprint planning

### Format Consistency

- Use Markdown for all specifications
- Include YAML frontmatter for metadata
- Follow the template structure
- Use consistent heading levels
- Include code examples in fenced blocks

## Related Documentation

- [SPEC-KIT-UTILIZATION.md](../SPEC-KIT-UTILIZATION.md) - Spec-kit usage guide
- [HANDOVER.md](../../HANDOVER.md) - Development handover document
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System architecture
- [DEVELOPMENT.md](../DEVELOPMENT.md) - Development guide

## Contributing

When adding new specifications:

1. Follow the template format
2. Use clear, concise language
3. Include diagrams where helpful
4. Add test cases and acceptance criteria
5. Link to related specifications
6. Update this README with new entry

## Questions?

- Check existing specifications for examples
- Review the spec template
- Consult [SPEC-KIT-UTILIZATION.md](../SPEC-KIT-UTILIZATION.md)
- Ask the team during planning sessions

---

**Last Updated**: 2025-10-20
**Maintained By**: Development Team
