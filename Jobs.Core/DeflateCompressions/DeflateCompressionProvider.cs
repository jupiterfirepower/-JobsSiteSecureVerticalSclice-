using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace Jobs.Core.DeflateCompressions;

public class DeflateCompressionProvider : ICompressionProvider
{
    public DeflateCompressionProvider(IOptions<DeflateCompressionProviderOptions> options)
    {
        ArgumentNullException.ThrowIfNull(nameof(options));

        Options = options.Value;
    }

    private DeflateCompressionProviderOptions Options { get; }

    public string EncodingName => "deflate";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
        => new DeflateStream(outputStream, Options.Level, leaveOpen: true);
}