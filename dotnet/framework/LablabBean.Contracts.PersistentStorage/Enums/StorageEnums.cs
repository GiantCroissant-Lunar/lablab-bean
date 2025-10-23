namespace LablabBean.Contracts.PersistentStorage;

public enum StorageProviderType
{
    InMemory,
    JsonFile,
    BinaryFile,
    SQLite,
    LiteDB,
    CloudStorage,
    Custom
}

public enum StorageOperationType
{
    Save,
    Load,
    Delete,
    Exists,
    GetKeys,
    Clear,
    GetSize,
    Backup,
    Restore,
    Sync
}

public enum StoragePriority
{
    Low,
    Normal,
    High,
    Critical
}
