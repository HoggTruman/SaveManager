using SaveManager.Services.Appdata;

namespace SaveManagerTests.Services;

public class AppdataServiceTests
{
    public static GameDTO TestGameDTO => new() { Name = "test", ProfilesDirectory = "testDir", SavefileLocation = "testLocation" };

}
