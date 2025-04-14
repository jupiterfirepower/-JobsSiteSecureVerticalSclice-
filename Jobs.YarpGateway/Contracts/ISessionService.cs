namespace Jobs.YarpGateway.Contracts;

public interface ISessionService
{
    void AddSession(string appId, string correlationId);
    void UpdateSession(string correlationId, string apiKey);

    string GetApiKeyByAppId(string appId);
    void Trace();
}