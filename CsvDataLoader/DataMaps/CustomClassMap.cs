using System.Reflection;
using CsvHelper.Configuration;

namespace CsvDataLoader.DataMaps;

internal sealed class CustomClassMap<T> : ClassMap<T> where T: class
{
    public CustomClassMap()
    {
        foreach (var propertyInfo in typeof(T).GetProperties())
        {
            var indexAttribute = propertyInfo.GetCustomAttribute<CsvIndexAttribute>();
            
            if(indexAttribute == null)
                continue;

            Map(typeof(T), propertyInfo).Index(indexAttribute.Index);
        }
    }
}