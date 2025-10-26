# Infrastructure Setup

This directory contains Docker Compose configurations for running supporting services required by the Lablab Bean project.

## Qdrant Vector Database

Qdrant is used for persistent vector storage of NPC memories, enabling semantic search and cross-session memory persistence.

### Quick Start

```bash
# Start Qdrant
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f qdrant

# Stop Qdrant
docker-compose down

# Stop and remove data (full reset)
docker-compose down -v
```

### Accessing Qdrant

- **REST API**: <http://localhost:6333>
- **Web UI/Dashboard**: <http://localhost:6333/dashboard>
- **Health Check**: <http://localhost:6333/health>
- **gRPC Port**: 6334

### Data Persistence

Qdrant data is persisted in a Docker volume named `qdrant_storage`. This ensures memories are retained across container restarts.

### Configuration

The application connects to Qdrant using settings in `appsettings.Development.json`:

```json
{
  "KernelMemory": {
    "Storage": {
      "Provider": "Qdrant",
      "ConnectionString": "http://localhost:6333",
      "CollectionName": "game_memories"
    }
  }
}
```

### Health Check

Qdrant includes a built-in health check that runs every 30 seconds. You can manually check health:

```bash
curl http://localhost:6333/health
```

### Troubleshooting

**Port Already in Use:**

```bash
# Check what's using port 6333
netstat -ano | findstr :6333

# Stop existing Qdrant containers
docker ps | findstr qdrant
docker stop <container-id>
```

**Data Corruption:**

```bash
# Reset all data
docker-compose down -v
docker-compose up -d
```

**Connection Errors:**

- Verify Qdrant is running: `docker-compose ps`
- Check logs for errors: `docker-compose logs qdrant`
- Ensure port 6333 is not blocked by firewall
- Verify application can reach localhost:6333

### Production Considerations

For production deployments:

1. **Use Qdrant Cloud** or self-hosted cluster for high availability
2. **Enable authentication** with API keys
3. **Configure SSL/TLS** for secure connections
4. **Set up backups** of the Qdrant data volume
5. **Monitor performance** and scale as needed

### Resources

- **Qdrant Documentation**: <https://qdrant.tech/documentation/>
- **Docker Image**: <https://hub.docker.com/r/qdrant/qdrant>
- **API Reference**: <https://qdrant.tech/documentation/api-reference/>

## Adding More Services

To add additional services (Redis, PostgreSQL, etc.), add them to `docker-compose.yml`:

```yaml
services:
  qdrant:
    # ... existing config ...

  redis:
    image: redis:latest
    ports:
      - "6379:6379"
```

Then restart:

```bash
docker-compose up -d
```
