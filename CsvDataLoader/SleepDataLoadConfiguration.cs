namespace CsvDataLoader;

public sealed record SleepDataLoadConfiguration(bool HasHeader, string Delimiter, int SleepStartColumnOrdinal, int DurationColumnOrdinal) : LoadConfiguration(HasHeader, Delimiter);