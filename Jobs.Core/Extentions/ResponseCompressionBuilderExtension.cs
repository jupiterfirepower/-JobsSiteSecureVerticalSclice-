using System.IO.Compression;
using Jobs.Core.DeflateCompressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Core.Extentions;

public static class ResponseCompressionBuilderExtension
{
    public static IServiceCollection AddResponseCompressionService(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.MimeTypes =
                ResponseCompressionDefaults.MimeTypes.Concat(
                [
                    "application/json",
                    "text/json",
                    "text/plain"
                ]);
        
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<DeflateCompressionProvider>();
        });
        
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });
    
        services.Configure<DeflateCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });
        
        return services;
    }
}