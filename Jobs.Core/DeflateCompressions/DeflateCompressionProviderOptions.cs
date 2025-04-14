using System.IO.Compression;
using Microsoft.Extensions.Options;

namespace Jobs.Core.DeflateCompressions;

public class DeflateCompressionProviderOptions : IOptions<DeflateCompressionProviderOptions>
{
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;

    DeflateCompressionProviderOptions IOptions<DeflateCompressionProviderOptions>.Value => this;
}