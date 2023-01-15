namespace CsvDataLoader;

public sealed record ActivityDataLoadConfiguration(bool HasHeader, string Delimiter, int ActivityStartColumnOrdinal,
    int DurationColumnOrdinal, int DistanceColumnOrdinal, int? BurnedCaloriesColumnOrdinal) : LoadConfiguration(
    HasHeader, Delimiter);