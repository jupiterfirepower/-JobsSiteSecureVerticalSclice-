namespace BlazorApp2.Components.Shared.Data;

public class VacancyData
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    
    public int CategoryId { get; set; }
    public string Title { get; set; }
    public string Note { get; set; }
    
    public int? SalaryFrom { get; set; }
    public int? SalaryTo { get; set; }
}