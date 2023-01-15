using System.Globalization;
using System.Runtime.CompilerServices;
using CsvDataLoader.DataMaps;
using CsvHelper;
using CsvHelper.Configuration;
using FitbitDataUploader.Abstractions.Models;

namespace CsvDataLoader;

public sealed class CsvLoader
{
    public async IAsyncEnumerable<SleepData> LoadSleepData(string filePath, SleepDataLoadConfiguration configuration, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            throw new ArgumentException($"File with sleep data not exists in path {filePath}");
        
        var reader = new StreamReader(filePath);
        
        var csv = new CsvReader(reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
                {HasHeaderRecord = configuration.HasHeader, Delimiter = configuration.Delimiter});

        csv.Context.RegisterClassMap(new SleepDataMap(configuration.SleepStartColumnOrdinal,
            configuration.DurationColumnOrdinal));

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return csv.GetRecord<SleepData>() ?? throw new Exception($"Failed to get sleep data");
        }
    }
    
    public async IAsyncEnumerable<ActivityData> LoadActivityData(string filePath, ActivityDataLoadConfiguration configuration, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            throw new ArgumentException($"File with activity data not exists in path {filePath}");
        
        var reader = new StreamReader(filePath);
        
        var csv = new CsvReader(reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
                {HasHeaderRecord = configuration.HasHeader, Delimiter = configuration.Delimiter});

        csv.Context.RegisterClassMap(new ActivityDataMap(configuration.ActivityStartColumnOrdinal,
            configuration.DurationColumnOrdinal, configuration.DistanceColumnOrdinal,
            configuration.BurnedCaloriesColumnOrdinal));

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return csv.GetRecord<ActivityData>() ?? throw new Exception($"Failed to get activity data");
        }
    }

    public async IAsyncEnumerable<T> LoadCustomData<T>(string filePath, LoadConfiguration configuration, [EnumeratorCancellation] CancellationToken cancellationToken) where T: class
    {
        if (!File.Exists(filePath))
            throw new ArgumentException($"File with custom data not exists in path {filePath}");
        
        var reader = new StreamReader(filePath);

        var csv = new CsvReader(reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
                {HasHeaderRecord = configuration.HasHeader, Delimiter = configuration.Delimiter});

        csv.Context.RegisterClassMap<CustomClassMap<T>>();

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            yield return csv.GetRecord<T>() ?? throw new Exception($"Failed to get record of type {typeof(T)}");
        }
    }
}