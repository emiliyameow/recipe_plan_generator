using System.Text.Json.Serialization;

namespace SmartChef.mvc.models.dto;

public class UserLoginModel
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
}