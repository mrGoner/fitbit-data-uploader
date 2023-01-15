using CsvHelper.Configuration;
using FitbitDataUploader.Abstractions.Models;

namespace CsvDataLoader.DataMaps;

internal sealed class ActivityDataMap : ClassMap<ActivityData>
{
    public ActivityDataMap(int activityStartIndex, int durationIndex, int distanceIndex, int? burnedCaloriesIndex)
    {
        Map(m => m.ActivityStart).Index(activityStartIndex);
        Map(m => m.Duration).Index(durationIndex);
        Map(m => m.Distance).Index(distanceIndex);


        if (burnedCaloriesIndex.HasValue)
            Map(m => m.BurnedCalories).Index(burnedCaloriesIndex.Value);
    }
}