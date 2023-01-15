using CsvDataLoader.DataMaps;

public class CustomActivityData
{
    [CsvIndex(0)]
    public DateTime Date { get; set; }
    
    [CsvIndex(1)]
    public TimeSpan Start { get; set; }
    
    [CsvIndex(2)]
    public TimeSpan Stop { get; set; }
    
    [CsvIndex(4)]
    public int Calories { get; set; }
    
    [CsvIndex(5)]
    public int Steps { get; set; }
}