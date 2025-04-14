using System.Text.Json;

namespace Jobs.ReferenceApi.Helpers;

public static class DataSerializerHelper
{
    public static void Serialize<T>(string fileName, List<T> data)
    {
        string jsonResult = JsonSerializer.Serialize(data);
        File.WriteAllText($"{fileName}", jsonResult);
    }
    
    public static List<T> Deserialize<T>(string data) => JsonSerializer.Deserialize<List<T>>(data)!;
}