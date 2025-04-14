using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RocksDb.Extensions;
using RocksDb.Extensions.Protobuf;
using RocksDb.Extensions.System.Text.Json;

namespace Jobs.Core.Extentions;

public static class RockDbExtentions
{
    public static IServiceCollection AddRocksDbService(this IServiceCollection services)
    {
        services.AddRocksDb(options =>
        {

            // Specify the path for the RocksDb database
            options.Path = "./apikey-rocks-db";

            // Clear pre-configured SerializerFactories so we can have full control over the order in which serializers will be resolved.
            options.SerializerFactories.Clear();

            // Add the serializer factories for the key and value types
            options.SerializerFactories.Add(new PrimitiveTypesSerializerFactory());
            //options.SerializerFactories.Add(new SystemTextJsonSerializerFactory());
            options.SerializerFactories.Add(new ProtobufSerializerFactory());
        });

        return services;
    }
}