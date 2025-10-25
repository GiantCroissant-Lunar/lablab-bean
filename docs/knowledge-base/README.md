# Knowledge Base CLI Commands

The Knowledge Base CLI (`kb`) allows you to manage game lore, quest information, and world knowledge that NPCs can access through RAG (Retrieval Augmented Generation).

## Commands

### 📚 Ingest Documents

Load documents into the knowledge base from markdown files.

```bash
# Ingest a single file
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb ingest --file docs/knowledge-base/world-history.md --category lore

# Ingest an entire directory
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb ingest --directory docs/knowledge-base --category lore
```

**Options:**

- `--file`, `-f`: Path to a single markdown file
- `--directory`, `-d`: Path to a directory containing markdown files
- `--category`, `-c`: Category (lore, quest, location, item)

### 🔍 Query Documents

Search the knowledge base with semantic search.

```bash
# Basic query
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb query --text "Tell me about dragons"

# Query with category filter
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb query --text "ancient ruins" --category location --limit 5
```

**Options:**

- `--text`, `-t`: Query text (required)
- `--limit`, `-l`: Maximum number of results (default: 3)
- `--category`, `-c`: Filter by category

### 📋 List Documents

List all documents in the knowledge base.

```bash
# List all documents
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb list

# List documents in a category
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb list --category lore
```

**Options:**

- `--category`, `-c`: Filter by category

### 🗑️ Delete Documents

Remove documents from the knowledge base.

```bash
# Delete by document ID
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb delete --id <document-id>
```

**Options:**

- `--id`, `-i`: Document ID to delete

## Document Format

Knowledge base documents use Markdown with YAML front-matter:

```markdown
---
title: "World History"
category: "lore"
tags:
  - history
  - ancient
  - dragons
---

# The Age of Dragons

Long ago, in the time before recorded history...
```

### Front Matter Fields

- `title`: Document title (string)
- `category`: Document category - lore, quest, location, item (string)
- `tags`: List of tags for filtering (array of strings)

### Categories

- **lore**: Game world history, mythology, factions
- **quest**: Quest information, objectives, rewards
- **location**: Cities, dungeons, points of interest
- **item**: Artifacts, consumables, equipment descriptions

## Examples

### Example 1: Ingest Sample Lore

```bash
cd D:\lunar-snake\personal-work\yokan-projects\lablab-bean

# Ingest all knowledge base documents
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb ingest --directory docs/knowledge-base --category lore
```

### Example 2: Query for Dragon Information

```bash
# Ask about dragons
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb query --text "Who is Draconus the Eternal?" --limit 3
```

### Example 3: List All Lore Documents

```bash
# See what lore is in the KB
dotnet run --project dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj -- kb list --category lore
```

## How NPCs Use the Knowledge Base

When NPCs detect a question from the player (e.g., "Tell me about..."), they:

1. Extract the query from the player's message
2. Determine the category (lore/quest/location/item)
3. Search the knowledge base using semantic search
4. Retrieve top 3-5 most relevant document chunks
5. Augment their response prompt with retrieved context
6. Generate a factually grounded answer with citations

Example NPC conversation:

```
Player: "Who is Draconus the Eternal?"

NPC: "Ah, Draconus the Eternal! He was the most powerful Ancient Dragon
      who ruled these lands for millennia. His reign ended 1000 years ago
      when he was defeated by Aethon the Bold in an epic battle at
      Dragonspire Peak. Many believe his treasure hoard still lies hidden
      somewhere in the mountains. [Source: world-history.md]"
```

## Architecture

```
User Input → Question Detection → Category Detection
     ↓
Knowledge Base Search (Semantic)
     ↓
Top-K Document Chunks Retrieved
     ↓
Prompt Augmentation with Context
     ↓
LLM Generation with Citations
```

## Requirements

- Qdrant vector database running (see appsettings.json)
- OpenAI API key configured
- Documents in Markdown format with YAML front-matter

## Troubleshooting

**Error: Cannot connect to Qdrant**

- Ensure Qdrant is running: `docker run -p 6333:6333 qdrant/qdrant`
- Check connection string in appsettings.json

**Error: Document not found**

- Verify file path is correct
- Ensure file has .md extension

**Error: No results found**

- Check if documents are ingested: `kb list`
- Try a different query or category filter

---

**Version**: 1.0.0
**Last Updated**: 2025-10-25
**Phase**: 5.11 - CLI Commands for KB Management
