namespace Jobs.Core.Contracts;

public interface ITypedClientConfig
{
    int Timeout { get; set; }
    string UserAgent { get; set; }
}