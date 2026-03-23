namespace SmartChef.mvc.models.dto.request;

public class PlanRequestForPlan
{
    public double Calories { get; set; }

    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbs { get; set; }
    
    public int BreakfastTime { get; set; }
    
    public int LunchDinnerTime { get; set; }
    
    public List<string> Cuisine { get; set; }
    
    public List<string> Intolerances { get; set; }
    public string Diet { get; set; }
    
    public bool RecipePlanExist { get; set; }
    
    public long BreakfastId { get; set; }
    
    public long LunchId { get; set; }
    
    public long DinnerId { get; set; }
   

    public override string ToString()
    {
        return $"Calories: {Calories}, Proteins: {Proteins}, Fats: {Fats}, Carbs:{Carbs}, Diet: {Diet},\n" +
               $" BreakfastTime: {BreakfastTime}, LunchDinnerTime: {LunchDinnerTime}, \n" +
               $"Cuisine: {string.Join(",", Cuisine)}, Intolerances: {string.Join(",", Intolerances)}";
    }
}