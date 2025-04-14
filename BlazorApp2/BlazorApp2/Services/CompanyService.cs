using BlazorApp2.Contracts;
using BlazorApp2.Contracts.Clients;
using Jobs.DTO;
using Jobs.DTO.In;

namespace BlazorApp2.Services;

public class CompanyService(ICompanyClientService clientService): ICompanyService
{
    public async Task<CompanyDataInDto> AddCompanyAsync(string name, string note, string logoPath, string link)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentNullException(nameof(note));
        
        if (string.IsNullOrWhiteSpace(logoPath))
            throw new ArgumentNullException(nameof(logoPath));

        var company = new CompanyDataInDto(0, name,note,  logoPath, link, true, true);
        
        return await clientService.AddCompanyAsync(company);
    }
    
    public async Task<bool> UpdateCompanyAsync(CompanyDataInDto company) => await clientService.UpdateCompanyAsync(company);
    
    public async Task<CompanyDto> GetCompanyByIdAsync(int companyId) => await clientService.GetCompanyByIdAsync(companyId);
}