namespace SaveManager.Models;

public class Game
{
    public required string Name { get; set; }
    public string SavefileLocation { get; set; } = "";
    public string ProfilesDirectory { get; set; } = "";
}
