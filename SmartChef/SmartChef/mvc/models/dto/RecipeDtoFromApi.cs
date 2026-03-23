namespace SmartChef.mvc.models.dto;

public class RecipeDtoFromApi
{
    public long Id { get; set; }
    public string? Image { get; set; }
   
    public string? Title { get; set; }v
    public int? ReadyInMinutes { get; set; }
    public int? Servings { get; set; }
    public string? SourceUrl { get; set; }
    
    public double Calories { get; set; }
    public double Proteins { get; set; }
    public double Carbs { get; set; }
    public double Fats { get; set; }
    public List<string?> DishTypes {get; set;}
    
}