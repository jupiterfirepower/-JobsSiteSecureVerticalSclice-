using System.Collections.Concurrent;
using Jobs.YarpGateway.Contracts;
using Jobs.YarpGateway.Helpers;

namespace Jobs.YarpGateway.Services;

public sealed class SessionService: ISessionService, IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentList<ApiKeyData>> _sessions = new (); //
    private readonly ConcurrentDictionary<string, string> _correlationAppId = new (); //
    private readonly CancellationTokenSource _src = new ();
    private readonly Task _task;
    
    public SessionService()
    {
        CancellationToken ct = _src.Token;
        
        _task = Task.Run(async () =>
        {
            while(true) {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(new TimeSpan(0,3,0), ct);
                await Maintain();
            }
        }, ct);
    }
    
    // Simple maintainer loop
    private Task Maintain()
    {
        var now = DateTime.UtcNow;
        
        foreach (var current in _sessions)
        {
            var dataForRemove = _sessions[current.Key].ToList().Where(x => x.Expires <= now).ToList();
            dataForRemove.ForEach(d => _sessions[current.Key].Remove(d));
        }

        return Task.CompletedTask;
    }
    
    public void Trace()
    {
        foreach (var currentItem in _correlationAppId) 
            Console.WriteLine("CorrelationAppId - " +currentItem.Key + ": " + currentItem.Value);
        foreach (var currentItem in _sessions)
        {
            Console.WriteLine($"AppId - {currentItem.Key} ");
            foreach (var currentAppKey in currentItem.Value) 
                Console.WriteLine($"------ApiKey - { currentAppKey.ApiKey}");
        }
    }

    public void AddSession(string appId, string correlationId)
    {
        _correlationAppId.TryAdd(correlationId, appId);
    }
    
    public string GetApiKeyByAppId(string appId)
    {
        if (_sessions.TryGetValue(appId, out var listApiKeys))
        {
            var apiKey = listApiKeys.FirstOrDefault();
            listApiKeys.Remove(apiKey!);
            return apiKey?.ApiKey ?? string.Empty;
        }

        return string.Empty;
    }
    
    private void RemoveCorrelationIdAppId(string correlationId)
    {
        if (_correlationAppId.ContainsKey(correlationId))
        {
            _correlationAppId.TryRemove(correlationId, out var _);
        }
    }
    
    public void UpdateSession(string correlationId, string apiKey)
    {
        if (_correlationAppId.TryGetValue(correlationId, out var appId))
        {
            var apiKeyData = new ApiKeyData(apiKey, DateTime.UtcNow.AddMinutes(15));
            
            if (_sessions.TryGetValue(appId, out var listApiKeys))
            {
                listApiKeys.Add(apiKeyData);
            }
            else
            {
                var newList = new ConcurrentList<ApiKeyData>([apiKeyData]);
                _sessions[appId] = newList;
            }
        }
        
        RemoveCorrelationIdAppId(correlationId);
    }
    
    private class ApiKeyData
    {
        internal ApiKeyData(string apiKey, DateTime exp)
        {
            ApiKey = apiKey;
            Expires = exp;
        }

        public string ApiKey { get; }
        public DateTime Expires { get; }
    }

    // Field used to detect redundant calls like double dispose
    private bool _disposed;

    // Protected method ensures correct handling of unmanaged resources
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return; 

        if (disposing) {

            // Cancel Task
            _src.Cancel();

            // Wait for your task to finish
            try
            {
                _task.Wait();
            }
            catch (AggregateException e)
            {
                // Handle the exception
            }
            
            _src.Dispose();
            
            _sessions.Clear();
            _correlationAppId.Clear();
        }

        // Free unmanaged resources here.
        _disposed = true;
    }  

    // Dispose() method that executes Dispose(true) and instructs the GC 
    // to skip the finalizer. 
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // The class destructor (aka Finalizer) for cleaning unmanaged resources 
    // if the programmer forgets to call Dispose()
    ~SessionService()
    {
        Dispose();
    }
}