using BlazorApp2.Contracts;
using BlazorApp2.Contracts.Clients;
using Jobs.Common.Responses;
using Jobs.Entities.DataModel;
using Jobs.Entities.Responses;

namespace BlazorApp2.Services;

public class AccountService(IAccountClientService clientService) : IAccountService
{
    private bool _isLoggedIn;
    private string  _username = string.Empty;
    private KeycloakTokenResponse?  _lastResponse;
    public bool IsLoggedIn => _isLoggedIn;
    public string Username => _username;
    private KeycloakTokenResponse?  Response => _lastResponse;
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));
        
        Console.WriteLine("AccountService LoginAsync.");

        _lastResponse = await  clientService.LoginAsync(username, password);

        _isLoggedIn = true;
        _username = username;
        
        return true;
    }

    public async Task<RegisterUserResponse> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        var user = new User
        {
            Email = email, 
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };
        
        return await clientService.RegisterAsync(user);
    }

    public async Task<bool> LogoutAsync()
    {
        if (string.IsNullOrEmpty(_username))
            throw new ArgumentNullException(nameof(_username));
        
        var user = new LogoutUser { Username = _username };
        
        var result = await clientService.LogoutAsync(user);

        if (result)
        {
            _isLoggedIn = false;
            _username = string.Empty;
        }

        return true;
    }
    
    public async Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));
        
        var result = await clientService.RefreshTokenAsync(refreshToken);

        return result;
    }
}