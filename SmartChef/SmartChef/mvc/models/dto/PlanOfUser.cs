namespace SmartChef.mvc.models.dto;

public class PlanOfUser
{
    public RecipeDtoFromApi[] Recipes {get; set; }
    
    public double CaloriesPlan { get; set; }
    public double ProteinsPlan { get; set; }
    public double FatsPlan { get; set; }
    public double CarbsPlan { get; set; }
    
}