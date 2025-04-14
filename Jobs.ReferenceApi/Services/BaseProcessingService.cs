using AutoMapper;
using Jobs.ReferenceApi.Helpers;

namespace Jobs.ReferenceApi.Services;

public abstract class BaseProcessingService(IMapper mapper)
{
    protected List<TR> GetDataAsync<T, TR>(string fileName, Func<Task<List<T>>> fn)
    {
        if (File.Exists(fileName))
        {
            string jsonResult = File.ReadAllText(fileName);
            return DataSerializerHelper.Deserialize<TR>(jsonResult);
        }

        var result = GetDataFromDatabaseAsync<T, TR>(fn);
        SaveDataToFile(fileName, result);
        return result;
    }

    private List<TR> GetDataFromDatabaseAsync<T, TR>(Func<Task<List<T>>> fn)
    {
        var items = fn().Result;
        var result = mapper.Map<List<TR>>(items);
        return result;
    }

    private void SaveDataToFile<T>(string fileName, List<T> data) => DataSerializerHelper.Serialize(fileName, data);
}