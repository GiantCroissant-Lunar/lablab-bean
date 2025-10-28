# Configuration Directory

This directory contains configuration files for the LablabBean project.

## Files

- **`appsettings.Development.json`** - Development environment configuration
  - Database connections (SQLite for Akka persistence)
  - AI/ML settings (OpenAI, Qdrant vector store)
  - Media player settings
  - Plugin configuration

## Usage

Configuration files are automatically loaded by the .NET applications based on the environment.

For development, ensure you have:

1. OpenAI API key configured
2. Qdrant vector database running (if using AI features)
3. Appropriate file permissions for SQLite database creation
