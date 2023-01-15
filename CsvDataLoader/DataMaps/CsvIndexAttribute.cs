namespace CsvDataLoader.DataMaps;

public class CsvIndexAttribute : Attribute
{
    public int Index { get; }

    public CsvIndexAttribute(int index)
    {
        Index = index;
    }
}