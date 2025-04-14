using Jobs.Core.Contracts;
using Microsoft.Extensions.Configuration;

namespace Jobs.Core.Settings;

public class TypedClientConfig : ITypedClientConfig
{
    public TypedClientConfig(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        configuration.Bind("TypedClient", this);
    }

    public int Timeout { get; set; }
    public required string UserAgent { get; set; }
}