namespace Jobs.Common.Settings;

public class KestrelSettings
{
    public int MaxConcurrentConnections { get; set; } = 100;
    public int MaxConcurrentUpgradedConnections { get; set; } = 100;
    public int MaxRequestBodySize { get; set; } = 10 * 1024;
    
    public int Port { get; set; } = 7159;
}
