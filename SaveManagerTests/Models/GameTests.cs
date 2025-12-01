using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
public class GameTests
{
    private readonly string _root = Path.Join(Directory.GetCurrentDirectory(), "Test");


    #region Profiles Setter

    [Fact]
    public void ProfilesSetter_WithNullActiveProfile_SetsAnActiveProfile()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Profile profile = new(FilesystemItemFactory.NewFolder("Profile", null), game);
        
        game.Profiles = [profile];

        Assert.NotNull(game.ActiveProfile);        
    }


    [Fact]
    public void ProfilesSetter_WhenNewProfilesDontContainActiveProfile_SetsNewActiveProfile()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Profile oldProfile = new(FilesystemItemFactory.NewFolder("Old Profile", null), game);
        game.Profiles = [oldProfile];
        game.ActiveProfile = oldProfile;
        Profile newProfile = new(FilesystemItemFactory.NewFolder("New Profile", null), game);

        game.Profiles = [newProfile];        

        Assert.Equal(newProfile, game.ActiveProfile);   
    }

    #endregion




    #region CreateProfile Tests

    [Fact]
    public void CreateProfile_WithValidName_CreatesProfile()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profileName = "NewProfile";
        string profilePath = Path.Join(profilesDirectory, profileName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilesDirectory) == true &&
            x.DirectoryExists(profilePath) == false));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Profile profile = game.CreateProfile(profileName);

        Assert.Contains(profile, game.Profiles);
        Assert.Equal(game, profile.Game);
    }


    [Fact]
    public void CreateProfile_WithoutSettingProfilesDirectory_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Assert.Throws<InvalidOperationException>(() => game.CreateProfile("NewProfile"));
    }


    [Fact]
    public void CreateProfile_WithTakenName_ThrowsValidationException()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profileName = "NewProfile";
        string profilePath = Path.Join(profilesDirectory, profileName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilesDirectory) == true &&
            x.DirectoryExists(profilePath) == false));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Profile existingProfile = game.CreateProfile(profileName);

        Assert.Throws<ValidationException>(() => game.CreateProfile(profileName));
    }

    #endregion
}
