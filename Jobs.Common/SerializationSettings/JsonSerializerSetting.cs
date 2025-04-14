using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jobs.Common.SerializationSettings;

public static class JsonSerializerSetting
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new ()
    {
        WriteIndented = true,
        IncludeFields = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip // 0 - Skip, 1 - Disallow
    };
}