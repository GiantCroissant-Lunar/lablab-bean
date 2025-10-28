# Scripts Directory

This directory contains utility scripts for the LablabBean project.

## Structure

- **`test/`** - Testing and verification scripts
  - `test-apps-verification.ps1` - Comprehensive app verification test
  - `test-terminal-ui.ps1` - Terminal UI integration test
- **`run-media-player.ps1`** - Quick launcher for the media player
- **`security-scan-local.ps1`** - Local security scanning with TruffleHog

## Usage

### Running Tests

From the project root:

```powershell
# Run app verification tests
.\scripts\test\test-apps-verification.ps1

# Run terminal UI tests
.\scripts\test\test-terminal-ui.ps1
```

### Running Media Player

From the project root:

```powershell
# Launch the media player
.\scripts\run-media-player.ps1

# Run security scan locally (requires Docker)
.\scripts\security-scan-local.ps1
```

## Test Reports

Test reports are automatically saved to `test-reports/` in the project root with timestamps.
