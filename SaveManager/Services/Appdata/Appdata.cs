using System.Text.Json.Serialization;

namespace SaveManager.Services.Appdata;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class Appdata
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GameDTO> Games { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StartupPreferences StartupPreferences { get; set; } = new();
}
