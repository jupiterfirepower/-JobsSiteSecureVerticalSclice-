using System.Text.Json.Serialization;

namespace Jobs.Entities.DataModel;

public class User
{  
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; } = null;
    public string LastName { get; set; } = null;
}