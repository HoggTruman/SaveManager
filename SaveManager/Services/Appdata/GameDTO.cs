using System.Text.Json.Serialization;

namespace SaveManager.Services.Appdata;


public class GameDTO
{
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    [JsonPropertyName("SavefileLocation")]
    public string? SavefileLocation { get; set; } = null;

    [JsonPropertyName("ProfilesDirectory")]
    public string? ProfilesDirectory { get; set; } = null;
}
