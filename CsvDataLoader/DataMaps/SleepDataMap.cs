using CsvHelper.Configuration;
using FitbitDataUploader.Abstractions.Models;

namespace CsvDataLoader.DataMaps;

internal sealed class SleepDataMap : ClassMap<SleepData>
{
    public SleepDataMap(int sleepStartIndex, int durationIndex)
    {
        Map(m => m.SleepStart).Index(sleepStartIndex);
        Map(m => m.Duration).Index(durationIndex);
    }
}