# Next Steps - Intelligent Avatar Integration Guide

**Priority**: 🔴 HIGH
**Estimated Time**: 4-8 hours
**Goal**: Wire intelligent avatar system into console app and achieve MVP functionality

---

## 🎯 Objective

Complete the integration of the intelligent avatar system (Akka.NET + Semantic Kernel) with the console application to enable User Story 1: **Boss with Personality and Memory**.

---

## 📋 Prerequisites (✅ Already Complete)

- ✅ All 3 framework libraries created (AI.Core, AI.Actors, AI.Agents)
- ✅ Akka.NET 1.5.35 integrated
- ✅ Semantic Kernel 1.25.0 integrated
- ✅ BossActor with Akka.Persistence implemented
- ✅ BossIntelligenceAgent with ChatCompletion working
- ✅ YAML personality system (personalities/boss-default.yaml)
- ✅ ECS bridge components (AkkaActorRef, SemanticAgent, IntelligentAI)

---

## 🔧 Critical Tasks (Priority Order)

### Task 1: Wire Akka.NET and Semantic Kernel in Console App (2-3 hours)

**File**: `dotnet/console-app/LablabBean.Console/Program.cs`

**Work Required**:

1. **Add Akka.NET Hosting**:

   ```csharp
   builder.Services.AddAkka("GameActorSystem", configBuilder =>
   {
       configBuilder
           .WithRemoting("localhost", 8081)
           .WithClustering(new ClusterOptions { SeedNodes = new[] { "akka.tcp://GameActorSystem@localhost:8081" } })
           .WithActors((system, registry, resolver) =>
           {
               var props = resolver.Props<BossActor>();
               registry.Register<BossActor>(system.ActorOf(props, "boss-supervisor"));
           })
           .AddHocon(ConfigurationFactory.ParseString(@"
               akka {
                   persistence {
                       journal.plugin = ""akka.persistence.journal.sql-server""
                       snapshot-store.plugin = ""akka.persistence.snapshot-store.sql-server""
                   }
               }
           "), HoconAddMode.Prepend);
   });
   ```

2. **Add Semantic Kernel Services**:

   ```csharp
   builder.Services.AddSemanticKernel();
   builder.Services.AddOpenAIChatCompletion(
       modelId: "gpt-4",
       apiKey: builder.Configuration["OpenAI:ApiKey"]!
   );
   ```

3. **Register AI Services**:

   ```csharp
   builder.Services.AddSingleton<IIntelligenceAgent, BossIntelligenceAgent>();
   builder.Services.AddSingleton<IIntelligenceAgent, EmployeeIntelligenceAgent>();
   ```

4. **Add Configuration**:
   - Update `appsettings.Development.json` with OpenAI API key
   - Add Akka.NET persistence connection string

### Task 2: Create IntelligentAISystem.cs (4-6 hours)

**File**: `dotnet/framework/LablabBean.AI.Core/Systems/IntelligentAISystem.cs`

**Purpose**: ECS system that spawns and manages intelligent avatar actors

**Required Implementation**:

```csharp
public class IntelligentAISystem : BaseSystem<World, float>
{
    private readonly IActorRefFactory _actorSystem;
    private readonly IIntelligenceAgent _bossAgent;
    private readonly QueryDescription _spawnQuery;
    private readonly QueryDescription _updateQuery;

    public IntelligentAISystem(World world, IActorRefFactory actorSystem, IIntelligenceAgent bossAgent) : base(world)
    {
        _actorSystem = actorSystem;
        _bossAgent = bossAgent;

        // Query for entities that need actors spawned
        _spawnQuery = new QueryDescription()
            .WithAll<IntelligentAI>()
            .WithNone<AkkaActorRef>();

        // Query for entities with active actors
        _updateQuery = new QueryDescription()
            .WithAll<IntelligentAI, AkkaActorRef>();
    }

    public override void Update(in float deltaTime)
    {
        // Spawn actors for new entities
        World.Query(in _spawnQuery, (Entity entity, ref IntelligentAI ai) =>
        {
            var actorRef = SpawnActor(entity, ai);
            entity.Add(new AkkaActorRef { Actor = actorRef, ActorPath = actorRef.Path });
            entity.Add(new SemanticAgent
            {
                AgentType = ai.Capabilities.HasFlag(AICapability.BossAI) ? AgentType.Boss : AgentType.Employee,
                PersonalityFile = ai.PersonalityFile ?? "boss-default.yaml"
            });
        });

        // Update existing actors
        World.Query(in _updateQuery, (Entity entity, ref IntelligentAI ai, ref AkkaActorRef actorRef) =>
        {
            // Send periodic updates to actors
            UpdateActor(entity, actorRef, deltaTime);
        });
    }

    private IActorRef SpawnActor(Entity entity, IntelligentAI ai)
    {
        // Implementation: Create and return actor based on AI type
        var props = Props.Create(() => new BossActor(entity.Id, _bossAgent));
        return _actorSystem.ActorOf(props, $"boss-{entity.Id}");
    }

    private void UpdateActor(Entity entity, AkkaActorRef actorRef, float deltaTime)
    {
        // Implementation: Send messages to actor based on game state
        // e.g., PlayerNearbyMessage, TakeDamageMessage
    }
}
```

**Key Responsibilities**:

1. Spawn Akka actors for entities with `IntelligentAI` component
2. Bridge game events to actor messages
3. Handle actor lifecycle (spawn, update, destroy)
4. Publish actor decisions back to ECS via events

### Task 3: Implement AvatarSupervisor (2 hours)

**File**: `dotnet/framework/LablabBean.AI.Actors/Supervision/AvatarSupervisor.cs`

**Purpose**: Akka.NET supervisor for fault tolerance

**Required Implementation**:

```csharp
public class AvatarSupervisor : ReceiveActor
{
    private readonly SupervisorStrategy _strategy;

    public AvatarSupervisor()
    {
        _strategy = new OneForOneStrategy(
            maxNrOfRetries: 3,
            withinTimeRange: TimeSpan.FromMinutes(1),
            decider: Decider.From(ex =>
            {
                switch (ex)
                {
                    case ActorInitializationException _:
                        return Directive.Stop;
                    case ActorKilledException _:
                        return Directive.Stop;
                    case OpenAIException _:
                        return Directive.Restart; // Retry on AI failures
                    default:
                        return Directive.Escalate;
                }
            })
        );

        ReceiveAny(_ => { }); // Supervisor just manages children
    }

    protected override SupervisorStrategy SupervisorStrategy() => _strategy;

    protected override void PreStart()
    {
        Context.System.EventStream.Subscribe(Self, typeof(ActorStoppedEvent));
    }
}
```

### Task 4: Test User Story 1 (2 hours)

**Acceptance Criteria** (from spec):

1. ✅ Boss entity has `IntelligentAI` + `AkkaActorRef` + `SemanticAgent` components
2. ✅ Boss actor spawns when boss entity created
3. ✅ Boss makes AI decisions based on personality file
4. ✅ Boss remembers previous player interactions
5. ✅ Boss changes tactics based on health threshold
6. ✅ Boss state persists across game saves/loads

**Test Steps**:

```csharp
// 1. Create boss entity
var bossEntity = world.Create(
    new IntelligentAI
    {
        Capabilities = AICapability.BossAI | AICapability.Memory | AICapability.Learning,
        PersonalityFile = "personalities/boss-default.yaml"
    },
    new Health { Current = 1000, Maximum = 1000 },
    new Position { X = 10, Y = 10 }
);

// 2. Verify actor spawned
Assert.True(bossEntity.Has<AkkaActorRef>());
var actorRef = bossEntity.Get<AkkaActorRef>().Actor;

// 3. Trigger AI decision
actorRef.Tell(new PlayerNearbyMessage
{
    PlayerEntity = playerEntity,
    Distance = 5.0f
});

// 4. Verify AI decision published as event
// Should see AIThoughtEvent on event bus

// 5. Damage boss and verify tactical change
actorRef.Tell(new TakeDamageMessage { Amount = 700 });
// Should see AIBehaviorChangedEvent when health < 30%

// 6. Test persistence
actorRef.Tell(new SaveSnapshotCommand());
// Restart actor and verify state restored
```

---

## 🛠️ Implementation Checklist

### Setup (15 min)

- [ ] Add OpenAI API key to `appsettings.Development.json`
- [ ] Add Akka persistence connection string to configuration
- [ ] Verify all NuGet packages are restored

### Console App Integration (2-3 hours)

- [ ] Add Akka.NET hosting in `Program.cs`
- [ ] Add Semantic Kernel services
- [ ] Register AI agents (Boss, Employee)
- [ ] Configure Akka persistence
- [ ] Test DI container resolves all dependencies

### ECS System (4-6 hours)

- [ ] Create `IntelligentAISystem.cs`
- [ ] Implement actor spawning logic
- [ ] Implement game-to-actor message bridging
- [ ] Implement actor-to-game event publishing
- [ ] Add system to ECS world
- [ ] Test entity with `IntelligentAI` spawns actor

### Supervision (2 hours)

- [ ] Create `AvatarSupervisor.cs`
- [ ] Implement restart strategy
- [ ] Wire supervisor into actor hierarchy
- [ ] Test actor restarts on failures

### Testing & Validation (2 hours)

- [ ] Create integration test for User Story 1
- [ ] Verify boss spawns with actor
- [ ] Verify AI makes decisions
- [ ] Verify memory persists
- [ ] Verify tactical changes work
- [ ] Document any issues

---

## 📖 Reference Documents

### Implementation Review

- **File**: `docs/_inbox/2025-10-25-intelligent-avatar-implementation-review.md`
- **Doc ID**: DOC-2025-00038
- **Key Sections**: Critical Gaps, Path to MVP

### Specification

- **File**: `specs/019-intelligent-avatar-system/spec.md`
- **User Stories**: US1-US4
- **Architecture**: Three-layer design (ECS + Akka.NET + Semantic Kernel)

### Tasks

- **File**: `specs/019-intelligent-avatar-system/tasks.md`
- **Phase 1**: Setup (complete)
- **Phase 2**: Foundational (complete)
- **Phase 3**: User Story 1 (in progress)

---

## 🚨 Common Pitfalls

1. **Missing API Key**: OpenAI API key not configured → agent fails to initialize
2. **Actor Path Issues**: Incorrect actor paths → messages don't reach actors
3. **DI Scope Issues**: Singleton vs Scoped services → lifetime conflicts
4. **Event Bus Not Wired**: Events not publishing → UI not updating
5. **Persistence Not Configured**: State not saving → memory loss on restart

---

## ✅ Success Criteria

**Minimum Viable Product (MVP)**:

- ✅ Boss entity spawns Akka actor automatically
- ✅ Boss makes AI decisions using Semantic Kernel
- ✅ Boss remembers player interactions (memory)
- ✅ Boss changes tactics at 30% health
- ✅ Boss state persists across saves

**Demo Scenario**:

```
1. Player encounters boss (distance < 10)
   → Boss actor receives PlayerNearbyMessage
   → BossIntelligenceAgent generates decision
   → Boss attacks with opening tactic

2. Player damages boss to 25% health
   → Boss actor receives TakeDamageMessage
   → BossIntelligenceAgent detects low health
   → AIBehaviorChangedEvent published
   → Boss switches to desperate/enrage tactics

3. Player saves and reloads game
   → Boss actor restores from snapshot
   → Boss remembers previous encounter
   → Boss references past fight in AI decisions
```

---

## 🎯 Next Session Plan

**Recommended Approach**:

1. **Session 1 (4 hours)**: Console integration + IntelligentAISystem
   - Wire Akka.NET and SK into Program.cs (1 hour)
   - Create IntelligentAISystem.cs skeleton (1 hour)
   - Implement actor spawning (2 hours)

2. **Session 2 (4 hours)**: Message bridging + testing
   - Implement game-to-actor messaging (2 hours)
   - Implement actor-to-game events (1 hour)
   - Test User Story 1 (1 hour)

3. **Session 3 (2 hours)**: Supervision + polish
   - Implement AvatarSupervisor (1 hour)
   - Fix any issues found in testing (1 hour)

---

**Good luck! The foundation is solid - just need to wire everything together.** 🚀

**Questions?** See implementation review at `docs/_inbox/2025-10-25-intelligent-avatar-implementation-review.md`
