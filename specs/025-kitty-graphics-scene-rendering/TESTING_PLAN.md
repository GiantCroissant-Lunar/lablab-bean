# SPEC-025 Testing Plan

**Status**: In Progress
**Created**: 2025-10-28
**Last Updated**: 2025-10-28

## Overview

This document outlines the testing strategy for SPEC-025 (Kitty Graphics Scene Rendering). The implementation is complete, and we now need to validate functionality across different environments.

## Testing Categories

### 🟢 Category A: Local Tests (Can Run Now)
Tests that can be executed in the current Windows environment without special terminal emulators.

### 🟡 Category B: Terminal-Specific Tests (Requires Setup)
Tests that require specific terminal emulators (WezTerm, Kitty, xterm).

### 🟠 Category C: Environment-Specific Tests (Requires Infrastructure)
Tests that require SSH, remote sessions, or specific network configurations.

### 🔴 Category D: Performance Tests (Requires Profiling Tools)
Tests that require performance monitoring and benchmarking tools.

---

## Test Task Breakdown

### Category A: Local Tests (5 tasks) 🟢

| Task | Description | Status | Can Test Now |
|------|-------------|--------|--------------|
| T079 | Test with missing tileset | ⏳ | ✅ Yes |
| T080 | Test with corrupted tileset | ⏳ | ✅ Yes |
| T092 | Run acceptance scenarios | ⏳ | ✅ Partial |
| T094 | Verify fallback without errors | ⏳ | ✅ Yes |
| T096 | Verify capability detection logs | ⏳ | ✅ Yes |

**Action**: Start with these tests immediately.

### Category B: Terminal-Specific Tests (5 tasks) 🟡

| Task | Description | Status | Requirements |
|------|-------------|--------|--------------|
| T076 | Test in WezTerm | ⏳ | Install WezTerm |
| T077 | Test in xterm | ⏳ | Linux/WSL + xterm |
| T085 | Screenshot console rendering | ⏳ | WezTerm/Kitty |
| T086 | Screenshot SadConsole | ⏳ | Windows build |
| T093 | Verify pixel graphics rendering | ⏳ | WezTerm/Kitty |

**Action**: Set up WezTerm as next priority.

### Category C: Environment Tests (2 tasks) 🟠

| Task | Description | Status | Requirements |
|------|-------------|--------|--------------|
| T078 | Test via SSH | ⏳ | SSH server setup |
| T095 | Verify tileset loading both platforms | ⏳ | Console + Windows apps |

**Action**: Defer until infrastructure available.

### Category D: Performance Tests (5 tasks) 🔴

| Task | Description | Status | Requirements |
|------|-------------|--------|--------------|
| T081 | Measure 80x24 frame time | ⏳ | Profiling tools |
| T082 | Measure 160x48 frame time | ⏳ | Profiling tools |
| T083 | Profile Kitty encoding | ⏳ | Profiling tools |
| T084 | Video playback FPS | ⏳ | Media player + profiling |
| T097 | Verify frame time < 33ms | ⏳ | Performance baseline |

**Action**: Create performance test harness first.

### Category E: Visual Validation (2 tasks) 📸

| Task | Description | Status | Requirements |
|------|-------------|--------|--------------|
| T087 | Compare screenshots | ⏳ | Both terminal + Windows |
| T098 | Verify media player FPS | ⏳ | Video playback test |

**Action**: After terminal-specific tests.

---

## Testing Strategy

### Phase 1: Local Validation (Start Now) ✅

**Goal**: Validate error handling, fallback logic, and configuration.

**Tasks**: T079, T080, T092, T094, T096

**Steps**:
1. Build the console application
2. Test with missing tileset configuration
3. Test with corrupted tileset file
4. Verify capability detection logs
5. Verify graceful fallback to glyph mode
6. Document results

**Expected Duration**: 2-3 hours

---

### Phase 2: Terminal Emulator Setup 🟡

**Goal**: Install and configure WezTerm for Kitty graphics testing.

**Tasks**: T076, T085, T093

**Steps**:
1. Install WezTerm on Windows
2. Configure WezTerm for Kitty graphics protocol
3. Launch console app in WezTerm
4. Verify Kitty escape sequences
5. Take screenshots
6. Document findings

**Expected Duration**: 3-4 hours

---

### Phase 3: Cross-Platform Testing 🟠

**Goal**: Test on Linux/WSL with xterm and SSH sessions.

**Tasks**: T077, T078, T095

**Steps**:
1. Set up WSL environment
2. Install xterm
3. Test console app in xterm (verify fallback)
4. Set up SSH server
5. Test via SSH (verify remote detection)
6. Document findings

**Expected Duration**: 4-6 hours

---

### Phase 4: Performance Benchmarking 🔴

**Goal**: Measure rendering performance and validate targets.

**Tasks**: T081, T082, T083, T084, T097, T098

**Steps**:
1. Create performance test harness
2. Add instrumentation to rendering pipeline
3. Measure frame rendering times
4. Measure Kitty encoding overhead
5. Test media player video playback
6. Generate performance report
7. Compare against targets

**Expected Duration**: 6-8 hours

---

### Phase 5: Visual Validation 📸

**Goal**: Compare visual output between platforms.

**Tasks**: T086, T087

**Steps**:
1. Build Windows SadConsole app
2. Create identical test scene
3. Take screenshots on both platforms
4. Compare visual fidelity
5. Document differences
6. Generate comparison report

**Expected Duration**: 2-3 hours

---

## Test Environment Requirements

### Hardware
- Windows PC (primary development) ✅
- Linux machine or WSL (for xterm testing) ⏳
- SSH server (for remote session testing) ⏳

### Software
- .NET 8 SDK ✅
- WezTerm terminal emulator ⏳
- xterm (Linux/WSL) ⏳
- FFmpeg (for media player tests) ⏳
- Performance profiling tools ⏳

### Test Assets
- Sample tileset PNG (16x16 tiles) ⏳
- Corrupted PNG file ⏳
- Sample video file (for media player) ⏳
- Test scene data ⏳

---

## Acceptance Criteria Mapping

| Criterion | Tasks | Category | Status |
|-----------|-------|----------|--------|
| SC-001: Pixel graphics in WezTerm | T076, T085, T093 | B | ⏳ |
| SC-002: Fallback without errors | T077, T094 | A/B | ⏳ |
| SC-003: Unified tileset loading | T095 | C | ⏳ |
| SC-004: Capability detection logs | T096 | A | ⏳ |
| SC-005: Frame time < 33ms | T081, T097 | D | ⏳ |
| SC-006: Media player > 24 FPS | T084, T098 | D | ⏳ |

---

## Test Execution Plan

### Immediate Actions (Today)
1. ✅ Create this testing plan
2. ⏳ Execute Phase 1: Local Validation (T079, T080, T092, T094, T096)
3. ⏳ Document results in test report

### Short Term (This Week)
1. ⏳ Install WezTerm
2. ⏳ Execute Phase 2: Terminal Emulator Setup (T076, T085, T093)
3. ⏳ Build Windows SadConsole app
4. ⏳ Execute Phase 5: Visual Validation (T086, T087)

### Medium Term (Next Week)
1. ⏳ Set up WSL environment
2. ⏳ Execute Phase 3: Cross-Platform Testing (T077, T078, T095)

### Long Term (When Infrastructure Available)
1. ⏳ Execute Phase 4: Performance Benchmarking (T081-T084, T097-T098)

---

## Test Reporting

### Report Structure
Each test phase will produce a markdown report with:
- Test environment details
- Steps executed
- Results (pass/fail)
- Screenshots (where applicable)
- Logs and console output
- Issues found
- Recommendations

### Report Locations
- `specs/025-kitty-graphics-scene-rendering/test-reports/phase1-local-validation.md`
- `specs/025-kitty-graphics-scene-rendering/test-reports/phase2-terminal-setup.md`
- `specs/025-kitty-graphics-scene-rendering/test-reports/phase3-cross-platform.md`
- `specs/025-kitty-graphics-scene-rendering/test-reports/phase4-performance.md`
- `specs/025-kitty-graphics-scene-rendering/test-reports/phase5-visual-validation.md`

---

## Success Metrics

### Must Pass (Blocking)
- ✅ T094: Graceful fallback without errors
- ✅ T096: Capability detection logs shown
- ⏳ T079: Missing tileset handled gracefully
- ⏳ T080: Corrupted tileset handled gracefully

### Should Pass (High Priority)
- ⏳ T076: WezTerm renders Kitty graphics
- ⏳ T077: xterm falls back to glyph mode
- ⏳ T093: Pixel graphics verified

### Nice to Have (Medium Priority)
- ⏳ T081-T083: Performance benchmarks
- ⏳ T085-T087: Visual comparison
- ⏳ T078: SSH session detection

---

## Next Steps

1. **Create test assets directory**: `specs/025-kitty-graphics-scene-rendering/test-assets/`
2. **Create test reports directory**: `specs/025-kitty-graphics-scene-rendering/test-reports/`
3. **Build console application**: Ensure latest code is compiled
4. **Execute Phase 1**: Start with local validation tests
5. **Document results**: Create phase 1 test report

---

## Notes

- Documentation tasks (T088-T091) are already complete ✅
- Core implementation is complete and builds successfully ✅
- All testing is validation of existing functionality, not new development
- Test failures will be tracked as issues for bug fixes, not blockers for completion

---

**Status**: Ready to begin Phase 1 testing
**Responsible**: QA Team / Developer
**Estimated Total Time**: 17-24 hours across all phases
