using RocksDb.Extensions;

namespace Jobs.Core.DataModel;

public class ApiKeyRockDbStore(IRocksDbAccessor<string, ApiKey> rocksDbAccessor)
    : RocksDbStore<string, ApiKey>(rocksDbAccessor);