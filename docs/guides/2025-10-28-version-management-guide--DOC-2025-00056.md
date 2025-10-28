---
doc_id: DOC-2025-00056
title: Version Management Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-28
tags: [versioning, gitversion, build, release, semantic-versioning, git, nuke]
summary: >
  Comprehensive guide to version management in the Lablab-Bean project using GitVersion
  for automatic semantic versioning. Covers current setup, release workflow, and best practices.
source:
  author: agent
  agent: claude
  model: sonnet-4.5-20250929
---

# Version Management Guide

## Executive Summary

The Lablab-Bean project uses **GitVersion 6.0.0** for automatic semantic versioning based on git tags and commit history. This document provides a complete guide to understanding, using, and maintaining the version management system.

**Current Version**: v0.0.3
**Next Version**: v0.0.4 (when next release is tagged)
**Versioning Strategy**: GitVersion with Semantic Versioning 2.0

---

## 1. Current Setup

### Version Sources

| Source | Value | Purpose |
|--------|-------|---------|
| Git Tags | v0.0.1, v0.0.2, v0.0.3 | Source of truth for releases |
| Directory.Build.props | 0.0.3 | Fallback version for all .NET projects |
| website/package.json | 0.0.3 | Website/frontend version |
| GitVersion | Calculated | Dynamic build versioning |

### Tools & Integration

**Primary Tool**: GitVersion 6.0.0

- **Package**: `GitVersion.MsBuild` (defined in `dotnet/Directory.Packages.props`)
- **Configuration**: `GitVersion.yml` (root directory)
- **Build Integration**: NUKE build system (`build/nuke/Build.cs`)

**Build System**: NUKE

- Uses `[GitVersion]` attribute for automatic version injection
- Fallback version: `0.1.0-dev` (used when GitVersion unavailable)
- Creates versioned artifact directories: `build/_artifacts/{version}/`

---

## 2. How GitVersion Works

### Version Calculation

GitVersion calculates versions automatically based on:

1. **Git tags** (e.g., v0.0.3)
2. **Branch name** (main, develop, feature/*, release/*, hotfix/*)
3. **Commit history** since last tag
4. **Configuration** in `GitVersion.yml`

### Branch-Specific Behavior

```yaml
# main branch: Production releases
main:
  mode: ContinuousDelivery
  increment: Patch
  Result: 0.0.3 → 0.0.4 (after commits)

# develop branch: Alpha releases
develop:
  mode: ContinuousDeployment
  label: alpha
  increment: Minor
  Result: 0.0.3 → 0.1.0-alpha.1

# feature branches: Feature-named prerelease
feature:
  mode: ContinuousDeployment
  label: useBranchName
  increment: Inherit
  Result: 0.0.3 → 0.0.4-my-feature.1

# release branches: Beta releases
release:
  mode: ContinuousDelivery
  label: beta
  increment: None
  Result: 0.1.0-beta.1

# hotfix branches: Patch with beta label
hotfix:
  mode: ContinuousDeployment
  label: beta
  increment: Patch
  Result: 0.0.3 → 0.0.4-beta.1
```

### Version Format Examples

| Scenario | GitVersion Output | Explanation |
|----------|-------------------|-------------|
| On tag v0.0.3 | `0.0.3` | Exact tag match |
| After commits on main | `0.0.4-001` | Patch bumped, commit count |
| On develop branch | `0.1.0-alpha.5` | Minor bumped, alpha prerelease |
| On feature/auth branch | `0.0.4-auth.12` | Patch bumped, branch name label |
| On release/0.1.0 branch | `0.1.0-beta.3` | Beta prerelease |

---

## 3. Release Workflow

### Standard Release Process

#### Step 1: Development Work

```bash
# Work on main or feature branch
git checkout main
git commit -m "feat: add new feature"
git commit -m "fix: resolve bug"

# GitVersion calculates: 0.0.4-001, 0.0.4-002, etc.
```

#### Step 2: Decide Version Increment

Follow **Semantic Versioning 2.0**:

- **MAJOR** (1.0.0): Breaking changes, incompatible API changes
- **MINOR** (0.1.0): New features, backward-compatible
- **PATCH** (0.0.4): Bug fixes, backward-compatible

#### Step 3: Create Release Tag

```bash
# For patch release (0.0.3 → 0.0.4)
git tag v0.0.4
git push origin v0.0.4

# For minor release (0.0.3 → 0.1.0)
git tag v0.1.0
git push origin v0.1.0

# For major release (0.1.0 → 1.0.0)
git tag v1.0.0
git push origin v1.0.0
```

#### Step 4: Build & Verify

```bash
# Run build - GitVersion automatically uses tag
./build.ps1

# Verify version in artifacts
ls build/_artifacts/0.0.4/

# Check calculated version
./build.ps1 --target Version
```

#### Step 5: Update Static Versions (Manual)

After tagging, update fallback versions:

```bash
# Update dotnet/Directory.Build.props
<Version>0.0.4</Version>

# Update website/package.json
"version": "0.0.4"

# Commit version bump
git commit -am "chore: bump version to 0.0.4"
```

### Quick Release Checklist

- [ ] All changes committed and pushed
- [ ] Decided version number (MAJOR.MINOR.PATCH)
- [ ] Created and pushed git tag (e.g., `git tag v0.0.4`)
- [ ] Build succeeds with new version
- [ ] Updated Directory.Build.props version
- [ ] Updated website/package.json version
- [ ] Committed version bump
- [ ] Verified artifacts in `build/_artifacts/{version}/`

---

## 4. Common Scenarios

### Scenario A: First Release After Project Start

```bash
# Current: v0.0.3 (last tag)
# Goal: Release next version

git tag v0.0.4
git push origin v0.0.4
./build.ps1
# Produces: build/_artifacts/0.0.4/
```

### Scenario B: Feature Branch Preview

```bash
git checkout -b feature/new-ui
git commit -m "feat: new UI components"

# GitVersion calculates: 0.0.4-new-ui.1
./build.ps1
# Produces: build/_artifacts/0.0.4-new-ui.1/
```

### Scenario C: Hotfix Release

```bash
git checkout -b hotfix/critical-bug
git commit -m "fix: critical security bug"

# GitVersion calculates: 0.0.4-beta.1
./build.ps1

# When ready:
git checkout main
git merge hotfix/critical-bug
git tag v0.0.4
git push origin v0.0.4
```

### Scenario D: Major Version Release

```bash
# Ready for 1.0.0 release
git checkout main
git tag v1.0.0
git push origin v1.0.0

# Update Directory.Build.props
<Version>1.0.0</Version>

git commit -am "chore: bump version to 1.0.0"
git push
```

---

## 5. Version Sources Reference

### File Locations

```
lablab-bean/
├── GitVersion.yml                      # GitVersion configuration
├── dotnet/
│   ├── Directory.Build.props          # .NET fallback version (0.0.3)
│   └── Directory.Packages.props       # GitVersion.MsBuild package
├── build/
│   └── nuke/
│       └── Build.cs                   # NUKE build with GitVersion
└── website/
    └── package.json                   # Website version (0.0.3)
```

### Configuration: GitVersion.yml

```yaml
mode: ContinuousDeployment
branches:
  main:
    mode: ContinuousDelivery
    increment: Patch
    is-release-branch: true
  develop:
    mode: ContinuousDeployment
    label: alpha
    increment: Minor
  # ... more branch configurations
```

### Build Integration: Build.cs

```csharp
[GitVersion(Framework = "net8.0", NoFetch = true)]
readonly GitVersion GitVersion;

string Version => GitVersion?.SemVer ?? "0.1.0-dev";
```

---

## 6. Best Practices

### DO ✅

1. **Use git tags for releases**
   - Tag format: `v{MAJOR}.{MINOR}.{PATCH}` (e.g., v0.0.4, v1.0.0)
   - Always push tags: `git push origin v0.0.4`

2. **Follow Semantic Versioning 2.0**
   - MAJOR: Breaking changes
   - MINOR: New features
   - PATCH: Bug fixes

3. **Let GitVersion calculate versions automatically**
   - Trust GitVersion for build metadata
   - Use calculated versions in artifacts

4. **Keep static versions in sync**
   - Update Directory.Build.props after tagging
   - Update website/package.json after tagging
   - Commit version bumps separately

5. **Use conventional commits**
   - `feat:` for features (may trigger minor bump)
   - `fix:` for bug fixes (may trigger patch bump)
   - `BREAKING CHANGE:` for breaking changes (triggers major bump)

### DON'T ❌

1. **Don't manually edit version strings in build outputs**
   - Let GitVersion handle it

2. **Don't create tags without testing**
   - Always build and test before tagging

3. **Don't skip version bumps**
   - Never reuse version numbers
   - Always increment versions

4. **Don't mix versioning strategies**
   - Stick to GitVersion + git tags
   - Don't manually set versions in csproj files

5. **Don't forget to push tags**
   - Tags are local by default
   - Always `git push origin v0.0.4`

---

## 7. Troubleshooting

### Problem: GitVersion not calculating correctly

**Solution:**

```bash
# Ensure you have tags
git tag -l

# Fetch all tags from remote
git fetch --tags

# Check GitVersion calculation
./build.ps1 --target Version
```

### Problem: Build uses wrong version

**Solution:**

```bash
# Check current git state
git describe --tags

# Verify GitVersion.yml is valid
cat GitVersion.yml

# Rebuild with diagnostics
./build.ps1 --verbosity verbose
```

### Problem: Version mismatch between git and csproj

**Solution:**

```bash
# Current git version
git describe --tags  # e.g., v0.0.3

# Update Directory.Build.props to match
# Change: <Version>0.0.3</Version>

# Update website/package.json
# Change: "version": "0.0.3"

git commit -am "chore: align static versions with git tags"
```

### Problem: Forgot to tag a release

**Solution:**

```bash
# Tag the release commit retroactively
git log --oneline  # Find the commit hash

git tag v0.0.4 <commit-hash>
git push origin v0.0.4
```

---

## 8. CI/CD Integration

### GitHub Actions Example

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  release:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Required for GitVersion

      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v0.10.2
        with:
          versionSpec: '6.x'

      - name: Calculate Version
        uses: gittools/actions/gitversion/execute@v0.10.2

      - name: Build
        run: ./build.ps1

      - name: Create Release
        uses: actions/create-release@v1
        with:
          tag_name: ${{ github.ref }}
          release_name: Release ${{ env.GitVersion_SemVer }}
```

---

## 9. Quick Reference

### Commands

```bash
# View current tags
git tag -l

# Create and push new tag
git tag v0.0.4
git push origin v0.0.4

# Delete tag (if mistake)
git tag -d v0.0.4
git push origin --delete v0.0.4

# Build with version
./build.ps1

# Check calculated version
./build.ps1 --target Version

# View version in artifacts
ls build/_artifacts/
```

### Version Format

```
{MAJOR}.{MINOR}.{PATCH}-{PRERELEASE}+{BUILD}
   0   .   0   .   4   -  alpha.1   +  001

Examples:
- 0.0.3          (stable release)
- 0.0.4-001      (prerelease, 1 commit after 0.0.3)
- 0.1.0-alpha.5  (develop branch, 5 commits)
- 1.0.0-beta.2   (release branch, 2 commits)
```

### Key Files

| File | Purpose | Edit Frequency |
|------|---------|----------------|
| GitVersion.yml | GitVersion config | Rarely |
| Directory.Build.props | .NET fallback version | After each release |
| website/package.json | Website version | After each release |
| build/nuke/Build.cs | Build integration | Rarely |

---

## 10. Migration Notes

### Previous State (Before 2025-10-28)

- Static version in Directory.Build.props: **0.1.0**
- Git tags: v0.0.1, v0.0.2, v0.0.3
- **Mismatch**: Git tags (0.0.x) vs static version (0.1.0)

### Current State (After 2025-10-28)

- Static version aligned to git tags: **0.0.3**
- GitVersion as primary version source
- Directory.Build.props as fallback only
- **Consistency**: All versions aligned

### Rationale

Using GitVersion as the primary version source provides:

1. **Automation**: No manual version updates needed
2. **Consistency**: Single source of truth (git tags)
3. **Traceability**: Version tied to git commit history
4. **Flexibility**: Branch-specific versioning strategies

---

## Related Documentation

- **Semantic Versioning**: <https://semver.org/>
- **GitVersion Documentation**: <https://gitversion.net/docs/>
- **NUKE Build**: <https://nuke.build/>
- **Conventional Commits**: <https://www.conventionalcommits.org/>

---

**Version**: 1.0.0
**Last Updated**: 2025-10-28
**Maintained By**: Development Team
