namespace Jobs.Core.Options;

public class KeycloakOptions
{
    public const string SectionName = "KeycloakSetting";
    
    public string KeycloakServerUrl { get; set; }
    
    public int KeycloakServerPort { get; set; }
    
    public string KeycloakHealthCheckUriPart { get; set; }
}