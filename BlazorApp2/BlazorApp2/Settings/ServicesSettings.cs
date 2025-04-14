namespace BlazorApp2.Settings;

public class ServicesSettings
{
    public required string AccountApiBaseUrl { get; set; }
    public required string AccountApiRegisterUrl { get; set; }
    public required string AccountApiLogoutUrl { get; set; }
    
    public required string AccountApiLoginUrl { get; set; }
    
    public required string AccountApiRefreshTokenUrl { get; set; }
    
    public required string KeyCloakBaseUrl { get; set; }
    public required string KeyCloakRealm { get; set; }
    public required string CompanyApiServiceUrl { get; set; }
    
    public required string AccountApiServiceUrl { get; set; }
    
    public required string ServiceApiKey { get; set; }
    
    public required string VacancyApiServiceUrl { get; set; }
    
    public required string VacancyApiBaseUrl { get; set; }
}