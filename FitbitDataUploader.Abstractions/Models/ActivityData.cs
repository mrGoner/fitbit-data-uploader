namespace FitbitDataUploader.Abstractions.Models;

public class ActivityData
{
    public DateTime ActivityStart { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal Distance { get; set; }
    public DistanceUnit DistanceUnit { get; set; }
    public int ActivityCode { get; set; }
    public int? BurnedCalories { get; set; }
    
}