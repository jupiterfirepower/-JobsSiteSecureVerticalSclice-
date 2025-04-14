namespace Jobs.DTO.In;

public record CompanyInDto( int CompanyId,
    string CompanyName,
    string CompanyDescription,
    string CompanyLogoPath,
    string CompanyLink,
    bool IsVisible = true,
    bool IsActive = true );
