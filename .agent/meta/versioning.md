# Versioning and Sync Protocol

This document describes how adapters stay synchronized with base rules.

## Version Declaration

Every adapter MUST declare its expected base version:

```markdown
**Base-Version-Expected**: 1.0.0
```

This appears at the top of each adapter file (e.g., `.agent/adapters/claude.md`).

## Sync Check

When an agent starts working:

1. **Read** `.agent/base/00-index.md` to get current base version
2. **Compare** with adapter's `Base-Version-Expected`
3. **If mismatch**: Agent should ask human to review changes before proceeding

## Version in Base

The canonical version is declared in `.agent/base/00-index.md`:

```markdown
Version: 1.0.0
```

## Update Process

When base rules change:

### For Rule Authors (Humans)

1. **Update rule** in `.agent/base/20-rules.md`
2. **Increment version** in `.agent/base/00-index.md` (following semver)
3. **Document change** in `.agent/meta/changelog.md`
4. **Review adapters** - adapters will be out of sync until updated
5. **Update adapters** as needed with new `Base-Version-Expected`

### For Adapter Maintainers

1. **Review changelog** at `.agent/meta/changelog.md`
2. **Read new/changed rules** in base files
3. **Update adapter** to reference new rule IDs or incorporate changes
4. **Update** `Base-Version-Expected` to match new base version
5. **Test** that adapter works correctly

## Fail-Safe Behavior

If an agent detects a version mismatch:

```
WARNING: Adapter version mismatch
  Adapter expects: 1.0.0
  Base version: 1.1.0

Please review changelog at .agent/meta/changelog.md and update adapter.
```

Agent should:

- **Ask human** if it's safe to proceed
- **Not automatically update** to newer version without review
- **Provide link** to changelog for review

## Version Compatibility

### Major Version Changes (X.0.0)

- **Incompatible**: Adapter must be updated
- **Breaking changes** to rule semantics
- **Removed or renamed** rules

### Minor Version Changes (0.X.0)

- **Compatible**: Adapter can work but should be updated soon
- **New rules** added (adapter won't enforce them yet)
- **Clarifications** to existing rules

### Patch Version Changes (0.0.X)

- **Compatible**: No adapter update needed
- **Typo fixes** and formatting
- **Documentation** improvements

## Example Workflow

### Scenario: Adding a New Rule

1. Human adds R-DOC-005 to `20-rules.md`
2. Human updates version in `00-index.md`: `1.0.0` → `1.1.0`
3. Human adds entry to `changelog.md`
4. Adapters now have mismatched versions (expecting 1.0.0, base is 1.1.0)
5. Next time agent runs, it warns about mismatch
6. Human reviews changelog, updates adapter
7. Human updates `Base-Version-Expected: 1.1.0` in adapter
8. System is back in sync

### Scenario: Breaking Change

1. Human renames R-CODE-001 to R-SEC-003 (breaking change)
2. Human updates version: `1.1.0` → `2.0.0` (major bump)
3. Human updates changelog with migration notes
4. Adapters now critically out of sync
5. Agent detects major version mismatch, refuses to proceed
6. Human must review and update all adapters
7. System resumes once all adapters updated to 2.0.0

## Best Practices

### For Humans

- Always update version in `00-index.md` when changing rules
- Always document in `changelog.md`
- Use semver strictly (major for breaking, minor for additive, patch for fixes)
- Test adapters after base updates

### For Agents

- Check version sync at start of session
- Warn user if mismatch detected
- Do not silently proceed with mismatched versions
- Provide helpful links to changelog

### For Rule IDs

- **Never** delete rule IDs
- **Never** reuse rule IDs
- Mark deprecated rules as "DEPRECATED" but keep them
- Assign new IDs sequentially

## References

- **Semver Specification**: <https://semver.org>
- **Base Index**: `.agent/base/00-index.md`
- **Changelog**: `.agent/meta/changelog.md`
- **Current Base Version**: 1.0.0
