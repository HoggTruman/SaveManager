using System.Text.Json.Serialization;

namespace SaveManager.Services.Appdata;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class Appdata
{
    [JsonPropertyName("Games")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GameDTO> Games { get; set; } = [];

    [JsonPropertyName("Startup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StartupPreferences Startup { get; set; } = new();
}
