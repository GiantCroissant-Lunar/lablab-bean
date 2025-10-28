# Task 4: End-to-End Testing & Verification

**Status**: 🔄 IN PROGRESS
**Date**: 2025-10-25
**Branch**: `019-intelligent-avatar-system`

---

## 🎯 Objective

Verify that the IntelligentAISystem works end-to-end:

- Entities spawn with IntelligentAI components
- Actors are created for each entity
- Player proximity detection works
- AI decisions are made via Semantic Kernel
- Actor state persists and recovers
- Cleanup works when entities are destroyed

---

## 📋 Test Plan

### Test 1: Build & Launch ✅

- [x] Solution builds without errors
- [ ] Application starts without crashes
- [ ] Game window renders correctly

### Test 2: Entity Spawning

- [ ] 5 entities spawn on game start
- [ ] Entities have correct components (IntelligentAI, Position, Health, Name, Renderable)
- [ ] Entities visible on screen at correct positions
- [ ] Debug log shows spawn messages

### Test 3: Actor Creation

- [ ] BossActor created for "The Micromanager"
- [ ] BossActor created for "VP of Deadlines"
- [ ] EmployeeActor created for "Chatty Colleague"
- [ ] EmployeeActor created for "Coffee Expert"
- [ ] EmployeeActor created for "Bug Hunter"
- [ ] Akka.NET logs show actor spawning
- [ ] AkkaActorRef component added to entities

### Test 4: Player Proximity Detection

- [ ] Move player near entity (within 10 tiles)
- [ ] PlayerNearbyMessage sent to actor
- [ ] Actor logs receipt of message
- [ ] Move player away (>10 tiles)
- [ ] No messages sent when player far away

### Test 5: AI Decision Making

- [ ] Actor receives PlayerNearbyMessage
- [ ] Semantic Kernel agent invoked
- [ ] AI decision logged
- [ ] Decision event published to ECS

### Test 6: Event Publishing

- [ ] Actor publishes AIThoughtEvent
- [ ] Event reaches ECS via EventBusAdapter
- [ ] Event logged in debug console
- [ ] Other systems can react to events

### Test 7: Persistence & Recovery

- [ ] Play game for 1 minute
- [ ] Actors make decisions and accumulate state
- [ ] Exit game gracefully
- [ ] Check SQLite database for actor snapshots
- [ ] Restart game
- [ ] Verify actor state recovered

### Test 8: Entity Destruction Cleanup

- [ ] Destroy an intelligent entity (simulated death)
- [ ] Actor receives stop message
- [ ] Actor state saved to database
- [ ] ActorStoppedEvent published
- [ ] AkkaActorRef component removed

---

## 🔧 Test Environment

**Prerequisites**:

- .NET 8 SDK installed
- Semantic Kernel configured (OpenAI/Azure OpenAI API key)
- SQLite database writeable
- Terminal.Gui dependencies available

**Configuration**:

- Check `appsettings.json` for Semantic Kernel settings
- Verify Akka.Persistence SQLite connection string
- Ensure log level set to Debug for visibility

---

## 🚀 Running Tests

### Step 1: Check Configuration

```powershell
# Verify appsettings.json exists
Get-Content dotnet/console-app/LablabBean.Console/appsettings.json | Select-String "SemanticKernel|Akka"
```

### Step 2: Launch Game

```powershell
cd dotnet/console-app/LablabBean.Console
dotnet run
```

### Step 3: Observe Logs

- Watch console output for:
  - "IntelligentAISystem initialized"
  - "Spawned X bosses and Y employees"
  - "Spawned BossActor for entity X"
  - "Spawned EmployeeActor for entity Y"
- Watch in-game debug log panel (bottom of screen)

### Step 4: Manual Testing

1. Start new game
2. Observe entity spawning
3. Move player near entities
4. Watch for AI messages/events
5. Exit and restart to test persistence

---

## 📊 Results

### Test Execution Log

*(To be filled during testing)*

**Test 1: Build & Launch**

- Build: ✅ Success
- Launch: ⏳ Pending
- Render: ⏳ Pending

**Test 2: Entity Spawning**

- Spawn count: ⏳ Pending
- Components: ⏳ Pending
- Visibility: ⏳ Pending

**Test 3: Actor Creation**

- Boss actors: ⏳ Pending
- Employee actors: ⏳ Pending
- Logs: ⏳ Pending

**Test 4: Proximity Detection**

- Near detection: ⏳ Pending
- Far detection: ⏳ Pending

**Test 5: AI Decisions**

- SK invocation: ⏳ Pending
- Decision logging: ⏳ Pending

**Test 6: Event Publishing**

- Event send: ⏳ Pending
- Event receive: ⏳ Pending

**Test 7: Persistence**

- State save: ⏳ Pending
- State restore: ⏳ Pending

**Test 8: Cleanup**

- Actor stop: ⏳ Pending
- Component removal: ⏳ Pending

---

## 🐛 Issues Found

*(To be documented during testing)*

### Issue Template

```
**Issue #X**: [Title]
**Severity**: Critical / High / Medium / Low
**Description**: What went wrong
**Steps to Reproduce**: How to trigger the issue
**Expected**: What should happen
**Actual**: What actually happens
**Fix**: How it was resolved
```

---

## 📝 Notes

*(Testing observations and insights)*

---

## ✅ Success Criteria

- [ ] All 8 test scenarios pass
- [ ] No critical bugs found
- [ ] Performance acceptable (no lag)
- [ ] Logs show correct behavior
- [ ] Persistence works reliably
- [ ] Cleanup is graceful

---

**Next Steps After Testing**:

- Document any issues found
- Fix critical bugs
- Polish and optimize
- Create final demo video
- Write completion report
