using Jobs.Common.Contracts;

namespace Jobs.Common.Settings;

public class RetrySettings : IRetrySettings
{
    public int RetryCount { get; set; }
}