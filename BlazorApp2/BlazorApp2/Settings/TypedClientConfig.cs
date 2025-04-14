using BlazorApp2.Contracts;

namespace BlazorApp2.Settings;

public class TypedClientConfig : ITypedClientConfig
{
    public TypedClientConfig(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        configuration.Bind("TypedClient", this);
    }

    public int Timeout { get; set; }
}