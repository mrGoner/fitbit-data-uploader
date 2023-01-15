using CsvDataLoader.DataMaps;

public class CustomSleepData
{
    [CsvIndex(4)]
    public DateTime Start { get; set; }
    
    [CsvIndex(5)]
    public DateTime Stop { get; set; }
}