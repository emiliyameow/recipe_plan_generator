namespace SmartChef.mvc.models.dto.request;

public class PlanRequestForCalories
{
    public string? TypeFlag { get; set; }
    public UserBodyInformation? UserBodyInformation { get; set; }
    
    public double? Calories { get; set; }
    public Goal? Goal { get; set; }
    
    public double? Proteins { get; set; }
    public double? Fats { get; set; }
    public double? Carbs { get; set; }
    
}