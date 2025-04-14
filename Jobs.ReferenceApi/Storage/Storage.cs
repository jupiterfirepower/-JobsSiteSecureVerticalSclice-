namespace Jobs.ReferenceApi.Storage;

public abstract class Storage
{
    public StorageSource Type { get; init; }
    
    public enum StorageSource
    {
        File,
        Db,
        Stream
    }
}