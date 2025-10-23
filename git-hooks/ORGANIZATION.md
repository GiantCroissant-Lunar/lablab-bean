# Git Hooks Organization Plan

## Proposed Directory Structure

```text
git-hooks/
├── README.md                    # Main documentation
├── ORGANIZATION.md             # This file
├── hooks/                      # Actual git hook scripts
│   ├── pre-commit              # Main pre-commit hook
│   ├── commit-msg              # Commit message validation
│   └── pre-push                # Pre-push validation
├── checks/                     # Individual check scripts
│   ├── general/                # Language-agnostic checks
│   │   ├── gitleaks-check
│   │   ├── prevent-nul-file
│   │   └── validate-agent-pointers
│   ├── dotnet/                 # .NET specific checks
│   │   ├── dotnet-format-check
│   │   ├── one-type-per-file-check
│   │   └── OneTypePerFile/     # Tool implementation
│   └── python/                 # Python specific checks
│       └── python-check
├── utils/                      # Shared utilities
│   ├── common.sh              # Common functions
│   └── colors.sh              # Color output functions
└── examples/                   # Example configurations
    ├── pre-commit-config.yaml
    └── git-hooks-setup.sh
```

## Benefits of This Organization

1. Clear separation: hooks vs checks vs utilities
2. Language grouping: language-specific checks are grouped together
3. Scalability: easy to add new languages or check types
4. Maintainability: related files are co-located
5. Discoverability: clear naming conventions

## Migration Steps

1. Create new directory structure
2. Move files to appropriate locations
3. Update script paths in documentation
4. Update any cross-references between scripts
5. Test all hooks still work correctly

## Implementation Notes

- All scripts should maintain their executable permissions
- Update README.md with new paths
- Consider creating symlinks for backward compatibility during transition
- Update .pre-commit-config.yaml examples with new paths

Note: YAML and Markdown checks are provided via third-party pre-commit hooks
(`markdownlint-cli` and `pretty-format-yaml`) rather than custom scripts.
