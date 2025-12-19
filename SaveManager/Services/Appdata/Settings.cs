using System.Text.Json.Serialization;

namespace SaveManager.Services.Appdata;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class Settings
{
    public bool GlobalHotkeys { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Hotkey.Hotkey? LoadSelectedSaveHotkey { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Hotkey.Hotkey? ImportSaveHotkey { get; set; }
}