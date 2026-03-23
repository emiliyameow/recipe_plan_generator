namespace SmartChef.mvc.models.dto.request;

//  данные пользователя названы так же как в json кладутся
public class UserBodyInformation
{
    public long UserId { get; set; }
    public int Height { get; set; } // рост в см
    public double Weight { get; set; } // вес в кг
    public int Age { get; set; } // возраст в годах
    public Gender Gender { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public Goal Goal { get; set; }
}

public enum Gender
{
    Male,
    Female
}

//для уровня активности
public enum ActivityLevel
{
    Sedentary = 1,      // Сидячий образ жизни
    Light = 2,          // Легкая активность
    Moderate = 3,       // Умеренная активность
    High = 4,           // Высокая активность
    Extreme = 5         // Экстремальная активность
}

// для целей
public enum Goal
{
    WeightLoss,        // Похудение
    Maintenance,       // Поддержание
    WeightGain,        // Набор массы
    Recomposition  // Рекомпозиция тела (поддержание мышц при снижении жира)
}
