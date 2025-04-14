namespace BlazorApp2.Settings;

public static class ServicesBaseUris
{
    public static string AccountApiBaseUrl = "http://localhost:5029";
    public static string AccountApiRegisterUrl = $"{AccountApiBaseUrl}/api/v1/register";
    public static string AccountApiLogoutUrl = $"{AccountApiBaseUrl}/api/v1/logout";
    public static string KeyCloakBaseUrl = "hhttp://localhost:9001";
    public static string KeyCloakRealm = "mjobs";
    
}