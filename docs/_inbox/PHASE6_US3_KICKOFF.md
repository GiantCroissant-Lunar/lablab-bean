# Phase 6 - User Story 3: Knowledge-Grounded NPC Behavior - KICKOFF

**Date**: 2025-10-25
**Status**: 🚀 STARTING
**Tasks**: T041-T055 (15 tasks)
**Dependencies**: ✅ US1 (Semantic Retrieval), ✅ US2 (Persistence)

---

## 🎯 Goal

Enable NPCs to query knowledge bases (personality documents, policies, lore) to ground their responses in consistent documented information using RAG (Retrieval-Augmented Generation).

---

## 📋 User Story

**As a** game designer
**I want** NPCs to reference knowledge base documents (handbooks, policies, lore)
**So that** NPC behavior is consistent, grounded, and aligned with documented world rules

---

## ✅ Acceptance Criteria

1. **Knowledge Base Indexing**:
   - Documents can be indexed with role-based tagging
   - Text is chunked appropriately for semantic search
   - Embeddings are generated for each chunk

2. **RAG Query**:
   - NPCs can query knowledge bases with natural language
   - System retrieves relevant document chunks
   - Responses include citations to source documents

3. **NPC Integration**:
   - Employee NPCs query "Customer Service Guidelines"
   - Boss NPCs query "Management Policies"
   - Queries happen automatically during decision-making

4. **Sample Documents**:
   - Employee handbook with service procedures
   - Boss policies with management guidance
   - Documents indexed on startup

---

## 🧪 Independent Test

**Scenario**: Create an "Employee Handbook" with customer service procedures, trigger an NPC customer interaction, verify NPC behavior aligns with handbook guidance and includes citations.

**Expected**: NPC response follows documented procedures and cites specific handbook sections.

---

## 📦 Deliverables

### Tests (T041-T043)

- [ ] T041 - Unit test for IndexDocumentAsync
- [ ] T042 - Unit test for QueryKnowledgeBaseAsync
- [ ] T043 - Integration test for RAG workflow

### DTOs (T044-T046)

- [ ] T044 - KnowledgeBaseDocument DTO
- [ ] T045 - KnowledgeBaseAnswer DTO
- [ ] T046 - Citation DTO

### Service (T047-T051)

- [ ] T047 - IKnowledgeBaseService interface
- [ ] T048 - KnowledgeBaseService class
- [ ] T049 - Implement IndexDocumentAsync
- [ ] T050 - Implement QueryKnowledgeBaseAsync
- [ ] T051 - Register in DI container

### Integration (T052-T055)

- [ ] T052 - Update EmployeeIntelligenceAgent
- [ ] T053 - Update BossIntelligenceAgent
- [ ] T054 - Create sample KB documents
- [ ] T055 - Index KB on startup

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│     Intelligence Agents                 │
│  (EmployeeAgent, BossAgent)             │
└─────────────┬───────────────────────────┘
              │ IKnowledgeBaseService
              ▼
┌─────────────────────────────────────────┐
│    KnowledgeBaseService                 │
│  - IndexDocumentAsync()                 │
│  - QueryKnowledgeBaseAsync()            │
└─────────────┬───────────────────────────┘
              │ IKernelMemory
              ▼
┌─────────────────────────────────────────┐
│       Kernel Memory                     │
│  - ImportDocumentAsync()                │
│  - SearchAsync()                        │
└─────────────┴───────────────────────────┘
```

---

## 🔑 Key Concepts

### RAG (Retrieval-Augmented Generation)

1. **Index Phase**: Documents → Chunks → Embeddings → Vector Store
2. **Query Phase**: Question → Embedding → Semantic Search → Retrieved Chunks
3. **Generation Phase**: Chunks + Question → LLM → Grounded Answer + Citations

### Knowledge Base Structure

- **Document**: Full text file (handbook, policy, lore)
- **Chunk**: Semantic paragraph/section (for retrieval)
- **Citation**: Reference to source document + chunk

### Tagging Strategy

- **Role**: "employee", "boss", "all"
- **Category**: "policy", "handbook", "lore"
- **Type**: "knowledge"

---

## 📝 Implementation Plan

### Phase 1: DTOs & Contracts (T044-T047)

**Time**: 30 mins
**Goal**: Define data structures and interfaces

1. Create DTOs in `DTOs.cs`
2. Create `IKnowledgeBaseService.cs`

### Phase 2: Tests (T041-T043)

**Time**: 1 hour
**Goal**: Write failing tests (TDD)

1. Unit test for IndexDocumentAsync
2. Unit test for QueryKnowledgeBaseAsync
3. Integration test for full RAG workflow

### Phase 3: Service Implementation (T048-T050)

**Time**: 2 hours
**Goal**: Implement RAG functionality

1. Create KnowledgeBaseService
2. Implement document indexing with chunking
3. Implement semantic querying with citations

### Phase 4: DI & Integration (T051-T053)

**Time**: 1 hour
**Goal**: Wire up service and update agents

1. Register in DI container
2. Update EmployeeIntelligenceAgent
3. Update BossIntelligenceAgent

### Phase 5: Sample Data & Startup (T054-T055)

**Time**: 30 mins
**Goal**: Create documents and auto-index

1. Create employee_handbook.txt
2. Create boss_policies.txt
3. Add indexing to Program.cs

---

## 🎯 Success Metrics

- ✅ Documents indexed with semantic searchability
- ✅ RAG queries return relevant chunks with citations
- ✅ NPC responses grounded in knowledge base
- ✅ All 3 tests passing
- ✅ Sample documents indexed on startup

---

## 🚀 Let's Begin

**Starting with**: T041-T043 (Tests - TDD approach)
**Next**: T044-T047 (DTOs & Interfaces)
**Then**: T048-T051 (Implementation)

---

**Version**: 1.0.0
**Phase**: 6 (Kernel Memory Integration)
**Progress**: 40/80 tasks → Target: 55/80 tasks (69%)
