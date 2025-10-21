---
doc_id: DOC-2025-00030
title: Spec-Kit Quick Start Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [spec-kit, quickstart, templates, documentation]
summary: >
  5-minute guide to using Spec-Kit in Lablab Bean for specification
  templates and code generation.
---

# Spec-Kit Quick Start Guide

**5-Minute Guide to Using Spec-Kit in Lablab Bean**

## What is Spec-Kit?

Spec-kit provides:
- 📝 **Specification templates** - Standard format for documenting features
- 🏭 **Code generators** - Templates for common code patterns
- 🎯 **Consistency** - Uniform structure across the project

## Quick Usage

### 1. Create a New Specification (2 minutes)

```bash
# Copy the example
copy docs\specs\dungeon-generation-system.md docs\specs\my-feature.md

# Edit your feature details
code docs\specs\my-feature.md
```

**What to change**:
- Title and metadata (version, status, author)
- Overview and requirements
- Components and implementation details
- Test cases and acceptance criteria

### 2. Generate a New Monster (3 minutes)

```bash
# Copy the template
copy templates\entity\monster.tmpl dotnet\framework\LablabBean.Game.Core\Monsters\MyMonster.cs

# Edit MyMonster.cs and replace:
{{.Name}} → MyMonster
{{.DisplayName}} → "My Cool Monster"
{{.Glyph}} → 'M'
{{.MaxHealth}} → 100
{{.Attack}} → 20
{{.Defense}} → 10
{{.Speed}} → 5
{{.AiType}} → Aggressive
{{.AggroRange}} → 8
{{.ExperienceValue}} → 250
{{.GoldDropMin}} → 25
{{.GoldDropMax}} → 75
{{.Description}} → "A cool new monster"
{{.Author}} → "Your Name"
{{.Timestamp}} → "2025-10-20"
```

**Find & Replace** in your editor makes this quick!

### 3. Read Examples (1 minute)

- **Specification**: `docs\specs\dungeon-generation-system.md`
- **Template Usage**: `docs\specs\monster-template-example.md`
- **Full Guide**: `docs\SPEC-KIT-UTILIZATION.md`

## Common Tasks

### Document an Existing Feature

1. Open `docs\specs\dungeon-generation-system.md` as reference
2. Copy to new file: `docs\specs\your-feature.md`
3. Fill in sections based on your implementation
4. Commit with feature code

### Add a New Monster Type

1. Check `docs\specs\monster-template-example.md` for stats reference
2. Copy `templates\entity\monster.tmpl`
3. Replace variables with your values
4. Add custom behavior in TODO sections
5. Test in game

### Create a Service Class

1. Use monster template as reference
2. Adapt structure for services:
   - Remove monster-specific properties
   - Add service-specific methods
   - Keep logging and DI patterns

## Template Variables Reference

### Monster Template
```
{{.Name}}              // Class name (e.g., Dragon)
{{.DisplayName}}       // Display text (e.g., "Red Dragon")
{{.Glyph}}             // Character (e.g., 'D')
{{.Description}}       // Description text
{{.MaxHealth}}         // Number (e.g., 150)
{{.Attack}}            // Number (e.g., 25)
{{.Defense}}           // Number (e.g., 15)
{{.Speed}}             // Number (e.g., 5)
{{.AiType}}            // Aggressive|Passive|Patrol
{{.AggroRange}}        // Number (e.g., 10)
{{.ExperienceValue}}   // Number (e.g., 500)
{{.GoldDropMin}}       // Number (e.g., 50)
{{.GoldDropMax}}       // Number (e.g., 150)
{{.Author}}            // Your name
{{.Timestamp}}         // Current date/time
```

## File Locations

```
lablab-bean/
├── docs/
│   ├── specs/                    # Specifications go here
│   │   ├── README.md             # Specs guide
│   │   └── *.md                  # Feature specs
│   └── SPEC-KIT-UTILIZATION.md   # Full strategy guide
└── templates/
    ├── entity/
    │   └── monster.tmpl          # Monster code template
    └── docs/
        └── spec-template.tmpl    # Spec document template
```

## Pre-made Monster Examples

Copy these YAML configs and convert to code:

### Powerful Boss
```yaml
Name: Dragon
DisplayName: "Ancient Dragon"
Glyph: 'D'
MaxHealth: 200
Attack: 30
Defense: 20
Speed: 6
AiType: Aggressive
AggroRange: 15
ExperienceValue: 1000
GoldDropMin: 100
GoldDropMax: 300
```

### Fast Assassin
```yaml
Name: Wraith
DisplayName: "Shadow Wraith"
Glyph: 'W'
MaxHealth: 50
Attack: 22
Defense: 8
Speed: 12
AiType: Patrol
AggroRange: 10
ExperienceValue: 300
GoldDropMin: 30
GoldDropMax: 80
```

### Weak Swarm
```yaml
Name: Rat
DisplayName: "Giant Rat"
Glyph: 'r'
MaxHealth: 15
Attack: 5
Defense: 2
Speed: 8
AiType: Passive
AggroRange: 3
ExperienceValue: 20
GoldDropMin: 1
GoldDropMax: 5
```

## Tips & Tricks

### Find & Replace Workflow

1. **Open template** in your editor
2. **Copy to new file** with target name
3. **Use Find & Replace** (Ctrl+H in most editors):
   - Find: `{{.Name}}`
   - Replace: `YourMonsterName`
   - Replace All
4. **Repeat** for each variable
5. **Customize** TODO sections

### Batch Creation

Create a PowerShell script:

```powershell
# create-monster.ps1
param($Name, $DisplayName, $Glyph, $Health)

$template = Get-Content "templates\entity\monster.tmpl" -Raw
$output = $template `
    -replace '{{.Name}}', $Name `
    -replace '{{.DisplayName}}', $DisplayName `
    -replace '{{.Glyph}}', $Glyph `
    -replace '{{.MaxHealth}}', $Health
    # ... add more replacements

$output | Out-File "dotnet\framework\LablabBean.Game.Core\Monsters\$Name.cs"
Write-Output "Created $Name.cs"
```

Usage:
```bash
.\create-monster.ps1 -Name Dragon -DisplayName "Red Dragon" -Glyph D -Health 150
```

## Validation Checklist

Before committing:

- [ ] All `{{.Variables}}` replaced
- [ ] No syntax errors in generated code
- [ ] TODO sections customized or removed
- [ ] Logging statements updated
- [ ] Test in game (`task dotnet-run-console`)
- [ ] Specification created/updated
- [ ] README updated if needed

## Getting Help

### Documentation
1. `docs\SPEC-KIT-UTILIZATION.md` - Complete guide
2. `docs\specs\README.md` - Specs overview
3. `docs\specs\monster-template-example.md` - Template examples

### Examples
1. `docs\specs\dungeon-generation-system.md` - Real specification
2. `templates\entity\monster.tmpl` - Code template
3. `templates\docs\spec-template.tmpl` - Doc template

### Project Context
1. `HANDOVER.md` - What's implemented
2. `README.md` - Project overview
3. `Taskfile.yml` - Available commands

## Future: When Automated

Once template engine is integrated:

```bash
# Simple generation
task gen-monster NAME=Dragon HEALTH=150

# From YAML file
task gen-monster-from-yaml FILE=monsters/dragon.yaml

# Generate specification
task spec-new NAME=my-feature

# Validate everything
task spec-validate
```

## Summary

**To document a feature**: Copy spec example → Fill in details → Commit
**To generate code**: Copy template → Replace variables → Customize → Test
**To learn more**: Read the guides in `docs/`

---

**Time to productivity**: 5 minutes
**Time saved per feature**: 15-30 minutes
**Quality improvement**: Consistent patterns and documentation
