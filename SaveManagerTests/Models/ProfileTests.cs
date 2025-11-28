using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
public class ProfileTests
{
    private readonly string _root = Path.Join(Directory.GetCurrentDirectory(), "Test");

    #region Rename Tests

    [Fact]
    public void Rename_WithValidName_RenamesProfile()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profilePath = Path.Join(profilesDirectory, "Profile");
        string newName = "Renamed";
        string renamedPath = Path.Join(profilesDirectory, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilePath) == true &&
            x.DirectoryExists(renamedPath) == false));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Folder profilesFolder = FilesystemItemFactory.NewFolder(profilesDirectory, null);
        Folder profileFolder = FilesystemItemFactory.NewFolder(profilePath, profilesFolder);
        Profile profile = new(profileFolder, game);
        game.Profiles = [profile];
        game.ActiveProfile = profile;

        profile.Rename(newName);

        Assert.Equal(newName, profile.Name);
        Assert.Contains(game.Profiles, x => x.Name == newName);
    }


    [Fact]
    public void Rename_WithOwnName_ThrowsValidationException()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profilePath = Path.Join(profilesDirectory, "Profile");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilePath) == true));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Folder profilesFolder = FilesystemItemFactory.NewFolder(profilesDirectory, null);
        Folder profileFolder = FilesystemItemFactory.NewFolder(profilePath, profilesFolder);
        Profile profile = new(profileFolder, game);
        game.Profiles = [profile];
        game.ActiveProfile = profile;

        Assert.Throws<ValidationException>(() => profile.Rename(profile.Name));
    }


    [Fact]
    public void Rename_WithAnotherProfilesName_ThrowsValidationException()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profilePath = Path.Join(profilesDirectory, "Profile");
        string otherProfilePath = Path.Join(profilesDirectory, "OtherProfile");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilePath) == true &&
            x.DirectoryExists(otherProfilePath) == true));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Folder profilesFolder = FilesystemItemFactory.NewFolder(profilesDirectory, null);
        Folder profileFolder = FilesystemItemFactory.NewFolder(profilePath, profilesFolder);
        Profile profile = new(profileFolder, game);
        Folder otherProfileFolder = FilesystemItemFactory.NewFolder(otherProfilePath, profilesFolder);
        Profile otherProfile = new(otherProfileFolder, game);
        game.Profiles = [profile, otherProfile];
        game.ActiveProfile = profile;

        Assert.Throws<ValidationException>(() => profile.Rename(otherProfile.Name));
    }

    #endregion



    #region Delete Tests

    [Fact]
    public void Delete_UpdatesGameProfiles()
    {
        string profilesDirectory = Path.Join(_root, "ProfilesDirectory");
        string profileName = "Profile";
        string profilePath = Path.Join(profilesDirectory, profileName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(profilePath) == true));

        Game game = new("Game") { ProfilesDirectory = profilesDirectory };
        Folder profilesFolder = FilesystemItemFactory.NewFolder(profilesDirectory, null);
        Folder profileFolder = FilesystemItemFactory.NewFolder(profilePath, profilesFolder);
        Profile profile = new(profileFolder, game);
        game.Profiles = [profile];
        game.ActiveProfile = profile;

        profile.Delete();

        Assert.DoesNotContain(game.Profiles, x => x.Name == profileName);
        Assert.NotEqual(game.ActiveProfile, profile);
    }

    #endregion



    #region UpdateSaveListEntries Tests


    [Fact]
    public void UpdateSaveListEntries_WhenFolderClosed_DoesNotAddChildren()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Folder profileFolder = FilesystemItemFactory.NewFolder("Folder", null);
        Folder childFolder = FilesystemItemFactory.NewFolder("ChildFolder", profileFolder);
        childFolder.Children = [Mock.Of<IFilesystemItem>()];
        childFolder.IsOpen = false;
        profileFolder.Children = [childFolder];
        Profile profile = new(profileFolder, game);

        profile.UpdateSaveListEntries();

        Assert.Single(profile.SaveListEntries);
    }


    [Fact]
    public void UpdateSaveListEntries_WhenFolderOpen_AddsChildren()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Folder profileFolder = FilesystemItemFactory.NewFolder("Folder", null);
        Folder childFolder = FilesystemItemFactory.NewFolder("ChildFolder", profileFolder);
        childFolder.Children = [Mock.Of<IFilesystemItem>()];
        childFolder.IsOpen = true;
        profileFolder.Children = [childFolder];
        Profile profile = new(profileFolder, game);

        profile.UpdateSaveListEntries();

        Assert.Equal(2, profile.SaveListEntries.Count);
    }


    [Fact]
    public void UpdateSaveListEntries_SortsChildFoldersBeforeChildFiles()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Game game = new("Game");
        Folder profileFolder = FilesystemItemFactory.NewFolder("Folder", null);
        profileFolder.Children = [
            FilesystemItemFactory.NewSavefile("ChildFile1", profileFolder),
            FilesystemItemFactory.NewFolder("ChildFolder1", profileFolder),
            FilesystemItemFactory.NewSavefile("ChildFile2", profileFolder),
            FilesystemItemFactory.NewFolder("ChildFolder2", profileFolder)];        
        Profile profile = new(profileFolder, game);

        profile.UpdateSaveListEntries();

        // Checks every element up to the last Folder is a Folder (All Savefiles come after)
        int lastFolderIndex = profile.SaveListEntries.FindLastIndex(x => x is Folder);
        for (int i = 0; i < lastFolderIndex; ++i)
        {
            Assert.True(profile.SaveListEntries[i] is Folder);
        }        
    }

    #endregion
}
