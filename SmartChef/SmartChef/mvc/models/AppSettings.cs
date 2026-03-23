namespace SmartChef.mvc.models;

public class AppSettings
{
    public string? ApiKey { get; set; }
    public string? ConnectionString { get; set; }
    public int? MaxConcurrentRequests { get; set; }
}