using System.Text.Json.Serialization;

namespace SaveManager.Services.Appdata;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class StartupPreferences
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActiveGame { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActiveProfile { get; set; }

    public double WindowWidth { get; set; }

    public double WindowHeight { get; set; }

    public bool WindowMaximized { get; set; }
}
