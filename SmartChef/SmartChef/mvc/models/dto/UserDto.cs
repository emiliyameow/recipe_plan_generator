using System.Text.Json.Serialization;

namespace SmartChef.mvc.models.dto;

public class UserDto
{
    [JsonPropertyName("id")]
    public long UserId { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("role")]
    public string Role { get; set; }
}