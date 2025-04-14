namespace Jobs.Core.Options;

public class VaultOptions
{
    public const string SectionName = "VaultSetting";
    
    public string VaultServerUrl { get; set; }
    public int VaultServerPort { get; set; }
    
    public string VaultHealthCheckUriPart { get; set; }
}