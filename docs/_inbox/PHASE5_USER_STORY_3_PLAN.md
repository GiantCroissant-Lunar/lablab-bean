# Phase 5: User Story 3 - Knowledge Base RAG

**Start Date:** 2025-10-25
**Status:** 🚧 Planning

## Goal

Implement Retrieval Augmented Generation (RAG) for NPCs to access game lore, quest information, and world knowledge from a persistent knowledge base.

## Overview

Enable NPCs to answer questions about the game world, provide lore information, and give context-aware guidance by retrieving relevant information from a knowledge base before generating responses.

## Tasks Overview (T041-T055)

### Tests (Write First - TDD)

- [ ] T041: Integration test for knowledge retrieval
- [ ] T042: Unit test for document ingestion
- [ ] T043: Unit test for relevance filtering

### Knowledge Base Setup

- [ ] T044: Design knowledge base schema (lore, quests, items, locations)
- [ ] T045: Create sample knowledge documents (markdown/JSON format)
- [ ] T046: Add document ingestion service

### Implementation

- [ ] T047: Create `IKnowledgeBaseService` interface
- [ ] T048: Implement `KnowledgeBaseService` using Kernel Memory
- [ ] T049: Add document ingestion pipeline
- [ ] T050: Create RAG-enabled dialogue agent
- [ ] T051: Add knowledge retrieval to NPC decision making
- [ ] T052: Implement citation/source tracking
- [ ] T053: Add knowledge base management UI (optional)

### Integration & Testing

- [ ] T054: Test NPC answers lore questions correctly
- [ ] T055: Test knowledge relevance filtering

## Estimated Timeline

~1.5 days (15 tasks)

## Success Criteria

1. ✅ NPCs can answer lore questions from knowledge base
2. ✅ Knowledge retrieval completes within 3 seconds
3. ✅ Responses include source citations
4. ✅ Knowledge base can be updated without code changes
5. ✅ All tests pass

## Dependencies

- Requires Phase 4 completion (Qdrant persistence) ✅
- Requires Kernel Memory with Qdrant backend ✅
- Requires document parsing capabilities

## Design

### Knowledge Base Structure

```
knowledge-base/
├── lore/
│   ├── world-history.md
│   ├── factions.md
│   └── mythology.md
├── quests/
│   ├── main-quest-line.md
│   └── side-quests.md
├── locations/
│   ├── cities.md
│   └── dungeons.md
└── items/
    ├── artifacts.md
    └── consumables.md
```

### RAG Flow

1. **Player asks NPC**: "Tell me about the Ancient Dragon"
2. **Knowledge Retrieval**: Query knowledge base for relevant documents
3. **Context Building**: Top 3-5 most relevant passages extracted
4. **Generation**: LLM generates response using retrieved context
5. **Response**: NPC responds with accurate lore + citations

## Notes

- Knowledge base documents stored in Qdrant alongside memories
- Separate collection: `game_knowledge` vs `game_memories`
- Markdown format for easy editing
- YAML front-matter for metadata (tags, categories, version)

Would you like me to proceed with implementing User Story 3 (Knowledge Base RAG)?
