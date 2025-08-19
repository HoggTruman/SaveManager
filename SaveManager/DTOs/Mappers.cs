using SaveManager.Models;

namespace SaveManager.DTOs;

public static class Mappers
{
    public static GameAppdataDTO ToGameAppdataDTO(this Game game)
    {
        return new()
        {
            Name = game.Name,
            ProfilesDirectory = game.ProfilesDirectory,
            SavefileLocation = game.SavefileLocation
        };
    }
}
