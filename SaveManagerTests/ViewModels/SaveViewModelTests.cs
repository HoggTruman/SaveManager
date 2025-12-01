using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.Appdata;
using SaveManager.Services.FilesystemService;
using SaveManager.ViewModels;
using SaveManagerTests.TestHelpers;

namespace SaveManagerTests.ViewModels;

[Collection("Sequential")]
public class SaveViewModelTests : IClassFixture<FilesystemFixture>, IDisposable
{  
    private const string SavefilesFolderName = "Savefiles";
    private const string Game1ProfilesDirectoryName = "Game1ProfilesDirectory";
    private const string Game1SavefileName = "game1.savefile";

    private readonly string _testFolderPath;
    private readonly string _game1ProfilesDirectoryPath;
    private readonly string _game1SavefilePath;
    private readonly SaveViewModel _saveViewModel;

    public static readonly TestFolder TestFolder = new()
    {
        Name = "RootFolder",
        Folders = 
        [
            new() 
            { 
                Name = SavefilesFolderName,
                Files =
                [
                    Game1SavefileName,
                    "game2.savefile",
                ]
            },
            new() 
            { 
                Name = Game1ProfilesDirectoryName,
                Folders =
                [
                    new()
                    {
                        Name = "Game1Profile1",
                        Files =
                        [
                            "game1backup1.savefile",
                            "game1backup2.savefile"
                        ],
                        Folders =
                        [
                            new()
                            {
                                Name = "Folder1",
                                Files =
                                [
                                    "innergame1backup1.savefile",
                                    "innergame1backup2.savefile"
                                ],
                                Folders = [new() { Name = "InnerFolder" } ]
                            }, 
                            new()
                            {
                                Name = "Folder2",
                            }
                        ]
                    }
                ]
            },
        ]
    };


    public SaveViewModelTests(FilesystemFixture filesystemFixture)
    {
        _testFolderPath = Path.Join(filesystemFixture.TestDirectory, TestFolder.Name);
        _game1ProfilesDirectoryPath = Path.Join(_testFolderPath, Game1ProfilesDirectoryName);
        _game1SavefilePath = Path.Join(_testFolderPath, SavefilesFolderName, Game1SavefileName);
        _saveViewModel = new(Mock.Of<IAppdataService>());
        FilesystemItemFactory.SetDependencies(new FilesystemService());
        TestFolder.Create(filesystemFixture.TestDirectory);        
    }


    public void Dispose()
    {
        if (Directory.Exists(_testFolderPath))
        {
            Directory.Delete(_testFolderPath, true);
        }   
    }


    #region CreateProfile Tests

    [Fact]
    public void CreateProfile_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(() => _saveViewModel.CreateProfile("profile"));
        Assert.Null(exception);  
    }


    [Fact]
    public void CreateProfile_WithActiveGame_CreatesProfile()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;

        _saveViewModel.CreateProfile("profile");

        Assert.NotNull(_saveViewModel.ActiveGame.ActiveProfile);
        Assert.True(Directory.Exists(_saveViewModel.ActiveGame.ActiveProfile.Folder.Location));
    }

    #endregion




    #region RenameProfile Tests

    [Fact]
    public void RenameProfile_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(() => _saveViewModel.RenameProfile("profile"));
        Assert.Null(exception);  
    }


    [Fact]
    public void RenameProfile_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;

        Exception? exception = Record.Exception(() => _saveViewModel.RenameProfile("profile"));
        Assert.Null(exception);  
    }


    [Fact]
    public void RenameProfile_WithActiveProfile_RenamesProfile()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        string newName = "RenamedProfile";

        _saveViewModel.RenameProfile(newName);        

        Assert.NotNull(_saveViewModel.ActiveGame.ActiveProfile);
        Assert.Equal(newName, _saveViewModel.ActiveGame.ActiveProfile.Name);
        Assert.True(Directory.Exists(Path.Join(game.ProfilesDirectory, newName)));
    }

    #endregion




    #region DeleteProfile Tests

    [Fact]
    public void DeleteProfile_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.DeleteProfile);
        Assert.Null(exception);  
    }


    [Fact]
    public void DeleteProfile_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;

        Exception? exception = Record.Exception(_saveViewModel.DeleteProfile);
        Assert.Null(exception);  
    }


    [Fact]
    public void DeleteProfile_WithActiveProfile_DeletesProfile()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile profile = game.ActiveProfile!;
        

        _saveViewModel.DeleteProfile();

        Assert.Empty(_saveViewModel.ActiveGame.Profiles);
        Assert.Null(_saveViewModel.ActiveGame.ActiveProfile);
        Assert.False(Directory.Exists(profile.Folder.Location));
    }

    #endregion




    #region OpenCloseSelectedEntry Tests

    [Fact]
    public void OpenCloseSelectedEntry_WithNoSelectedEntry_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.OpenCloseSelectedEntry);
        Assert.Null(exception);  
    }


    [Fact]
    public void OpenCloseSelectedEntry_WithSavefileSelected_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        Profile profile = game.ActiveProfile!;
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = profile.SaveListEntries.OfType<Savefile>().First();

        Exception? exception = Record.Exception(_saveViewModel.OpenCloseSelectedEntry);
        Assert.Null(exception);  
    }


    [Fact]
    public void OpenCloseSelectedEntry_WithFolderSelected_AddsAndRemovesChildEntries()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder selected = activeProfile.SaveListEntries.OfType<Folder>().First();
        _saveViewModel.SelectedEntry = selected;

        // Folders are initialized as closed, so children are not visible
        Assert.True(selected.Children.All(x => !activeProfile.SaveListEntries.Contains(x)));

        // Open Folder and check children are visible
        _saveViewModel.OpenCloseSelectedEntry();
        Assert.True(selected.Children.All(activeProfile.SaveListEntries.Contains));

        // Close Folder and check children aren't visible
        _saveViewModel.OpenCloseSelectedEntry();
        Assert.True(selected.Children.All(x => !activeProfile.SaveListEntries.Contains(x)));
    }

    #endregion




    #region MoveEntry Tests

    [Fact]
    public void MoveEntry_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(() => 
            _saveViewModel.MoveEntry(Mock.Of<IFilesystemItem>(), Mock.Of<IFilesystemItem>()));
        Assert.Null(exception);  
    }


    [Fact]
    public void MoveEntry_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;

        Exception? exception = Record.Exception(() => 
            _saveViewModel.MoveEntry(Mock.Of<IFilesystemItem>(), Mock.Of<IFilesystemItem>()));
        Assert.Null(exception);  
    }


    [Fact]
    public void MoveEntry_ToNullWhenAlreadyInProfileFolder_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Savefile movingFile = activeProfile.Folder.Children.OfType<Savefile>().First();
        _saveViewModel.MoveEntry(movingFile, null);
    }


    [Fact]
    public void MoveEntry_FileToItself_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Savefile movingFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        _saveViewModel.MoveEntry(movingFile, movingFile);
    }


    [Fact]
    public void MoveEntry_FileToSiblingFile_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Savefile movingFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        Savefile targetFile = activeProfile.SaveListEntries.OfType<Savefile>().First(x => x != movingFile);
        _saveViewModel.MoveEntry(movingFile, targetFile);
    }


    [Fact]
    public void MoveEntry_FolderToSiblingFile_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        Savefile targetFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        _saveViewModel.MoveEntry(movingFolder, targetFile);
    }


    [Fact]
    public void MoveEntry_FolderToParentFolder_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        _saveViewModel.MoveEntry(movingFolder, movingFolder.Parent);
    }


    [Fact]
    public void MoveEntry_FolderToItself_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        _saveViewModel.MoveEntry(movingFolder, movingFolder);
    }


    [Fact]
    public void MoveEntry_FolderToChildFile_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        Savefile targetFile = movingFolder.Children.OfType<Savefile>().First();
        _saveViewModel.MoveEntry(movingFolder, targetFile);
    }


    [Fact]
    public void MoveEntry_FolderToChildFolder_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;        
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        Folder targetFolder = movingFolder.Children.OfType<Folder>().First();
        _saveViewModel.MoveEntry(movingFolder, targetFolder);
    }


    [Fact]
    public void MoveEntry_FileToValidFile_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile movingFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        Savefile targetFile = activeProfile.SaveListEntries.OfType<Folder>().First().Children.OfType<Savefile>().First();
        Folder originalParent = movingFile.Parent!;
        string originalLocation = movingFile.Location;

        _saveViewModel.MoveEntry(movingFile, targetFile);

        Assert.DoesNotContain(movingFile, originalParent.Children);
        Assert.Contains(movingFile, targetFile.Parent!.Children);
        Assert.Contains(movingFile, activeProfile.SaveListEntries);
        Assert.False(File.Exists(originalLocation));
        Assert.True(File.Exists(movingFile.Location));
    }


    [Fact]
    public void MoveEntry_FileToValidFolder_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile movingFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        Folder targetFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        Folder originalParent = movingFile.Parent!;
        string originalLocation = movingFile.Location;

        _saveViewModel.MoveEntry(movingFile, targetFolder);

        Assert.DoesNotContain(movingFile, originalParent.Children);
        Assert.Contains(movingFile, targetFolder.Children);
        Assert.Contains(movingFile, activeProfile.SaveListEntries);
        Assert.False(File.Exists(originalLocation));
        Assert.True(File.Exists(movingFile.Location));
    }


    [Fact]
    public void MoveEntry_FileToNullWhenNotInProfileFolder_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile movingFile = activeProfile.SaveListEntries.OfType<Folder>().First().Children.OfType<Savefile>().First();
        Folder originalParent = movingFile.Parent!;
        string originalLocation = movingFile.Location;

        _saveViewModel.MoveEntry(movingFile, null);

        Assert.DoesNotContain(movingFile, originalParent.Children);
        Assert.Contains(movingFile, activeProfile.Folder.Children);
        Assert.Contains(movingFile, activeProfile.SaveListEntries);
        Assert.False(File.Exists(originalLocation));
        Assert.True(File.Exists(movingFile.Location));
    }


    [Fact]
    public void MoveEntry_FolderToValidFile_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder originalParent = activeProfile.SaveListEntries.OfType<Folder>().First();
        Folder movingFolder = originalParent.Children.OfType<Folder>().First();
        Savefile targetFile = activeProfile.SaveListEntries.OfType<Savefile>().First();
        string originalLocation = movingFolder.Location;

        _saveViewModel.MoveEntry(movingFolder, targetFile);

        Assert.DoesNotContain(movingFolder, originalParent.Children);
        Assert.Contains(movingFolder, targetFile.Parent!.Children);
        Assert.Contains(movingFolder, activeProfile.SaveListEntries);
        Assert.False(Directory.Exists(originalLocation));
        Assert.True(Directory.Exists(movingFolder.Location));
    }


    [Fact]
    public void MoveEntry_FolderToValidFolder_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder movingFolder = activeProfile.SaveListEntries.OfType<Folder>().First();
        Folder targetFolder = activeProfile.SaveListEntries.OfType<Folder>().First(x => x != movingFolder);
        string originalLocation = movingFolder.Location;
        Folder originalParent = movingFolder.Parent!;

        _saveViewModel.MoveEntry(movingFolder, targetFolder);

        Assert.DoesNotContain(movingFolder, originalParent.Children);
        Assert.Contains(movingFolder, targetFolder.Children);
        Assert.Contains(movingFolder, activeProfile.SaveListEntries);
        Assert.False(Directory.Exists(originalLocation));
        Assert.True(Directory.Exists(movingFolder.Location));
    }


    [Fact]
    public void MoveEntry_FolderToNullWhenNotInProfileFolder_MovesEntry()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder originalParent = activeProfile.SaveListEntries.OfType<Folder>().First();
        Folder movingFolder = originalParent.Children.OfType<Folder>().First();
        string originalLocation = movingFolder.Location;

        _saveViewModel.MoveEntry(movingFolder, null);

        Assert.DoesNotContain(movingFolder, originalParent.Children);
        Assert.Contains(movingFolder, activeProfile.Folder.Children);
        Assert.Contains(movingFolder, activeProfile.SaveListEntries);
        Assert.False(Directory.Exists(originalLocation));
        Assert.True(Directory.Exists(movingFolder.Location));
    }

    #endregion




    #region AddFolder Tests

    [Fact]
    public void AddFolder_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(() => _saveViewModel.AddFolder("NewFolder"));
        Assert.Null(exception);
    }


    [Fact]
    public void AddFolder_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(() => _saveViewModel.AddFolder("NewFolder"));
        Assert.Null(exception);
    }


    [Fact]
    public void AddFolder_WithNullSelectedEntry_CreatesFolderInProfileDirectory()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        _saveViewModel.SelectedEntry = null;
        string newFolderName = "NewFolder";

        _saveViewModel.AddFolder(newFolderName);

        Assert.Equal(newFolderName, _saveViewModel.SelectedEntry?.Name);
        Assert.True(Directory.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(activeProfile.Folder.Children, x => x.Name == newFolderName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == newFolderName);
    }


    [Fact]
    public void AddFolder_WithFolderSelectedEntry_CreatesChildFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder selectedFolder = activeProfile.Folder.Children.OfType<Folder>().First();
        _saveViewModel.SelectedEntry = selectedFolder;
        string newFolderName = "NewFolder";

        _saveViewModel.AddFolder(newFolderName);

        Assert.Equal(newFolderName, _saveViewModel.SelectedEntry?.Name);
        Assert.True(Directory.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(selectedFolder.Children, x => x.Name == newFolderName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == newFolderName);
    }


    [Fact]
    public void AddFolder_WithSavefileSelectedEntry_CreatesSiblingFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile selectedFile = activeProfile.Folder.Children.OfType<Savefile>().First();
        _saveViewModel.SelectedEntry = selectedFile;
        string newFolderName = "NewFolder";

        _saveViewModel.AddFolder(newFolderName);

        Assert.Equal(newFolderName, _saveViewModel.SelectedEntry?.Name);
        Assert.True(Directory.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(selectedFile.Parent!.Children, x => x.Name == newFolderName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == newFolderName);
    }

    #endregion




    #region DeleteSelectedEntry Tests

    [Fact]
    public void DeleteSelectedEntry_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.DeleteSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void DeleteSelectedEntry_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.DeleteSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void DeleteSelectedEntry_WithNullSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = null;
        Exception? exception = Record.Exception(_saveViewModel.DeleteSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void DeleteSelectedEntry_WithFolderSelectedEntry_DeletesFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder toDelete = activeProfile.Folder.Children.OfType<Folder>().First();
        _saveViewModel.SelectedEntry = toDelete;
        
        _saveViewModel.DeleteSelectedEntry();

        Assert.Null(_saveViewModel.SelectedEntry);
        Assert.False(Directory.Exists(toDelete.Location));
        Assert.DoesNotContain(toDelete, activeProfile.Folder.Children);
        Assert.DoesNotContain(toDelete, activeProfile.SaveListEntries);
    }


    [Fact]
    public void DeleteSelectedEntry_WithSavefileSelectedEntry_DeletesFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile toDelete = activeProfile.Folder.Children.OfType<Savefile>().First();
        _saveViewModel.SelectedEntry = toDelete;
        
        _saveViewModel.DeleteSelectedEntry();

        Assert.Null(_saveViewModel.SelectedEntry);
        Assert.False(File.Exists(toDelete.Location));
        Assert.DoesNotContain(toDelete, activeProfile.Folder.Children);
        Assert.DoesNotContain(toDelete, activeProfile.SaveListEntries);
    }

    #endregion




    #region RenameSelectedEntry Tests

    [Fact]
    public void RenameSelectedEntry_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(() => _saveViewModel.RenameSelectedEntry("New Name"));
        Assert.Null(exception);
    }


    [Fact]
    public void RenameSelectedEntry_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(() => _saveViewModel.RenameSelectedEntry("New Name"));
        Assert.Null(exception);
    }


    [Fact]
    public void RenameSelectedEntry_WithNullSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = null;
        Exception? exception = Record.Exception(() => _saveViewModel.RenameSelectedEntry("New Name"));
        Assert.Null(exception);
    }


    [Fact]
    public void RenameSelectedEntry_WithFolderSelectedEntry_RenamesFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder toRename = activeProfile.Folder.Children.OfType<Folder>().First();
        _saveViewModel.SelectedEntry = toRename;
        string newName = "New Name";
        
        _saveViewModel.RenameSelectedEntry(newName);

        Assert.Equal(newName, _saveViewModel.SelectedEntry.Name);
        Assert.True(Directory.Exists(toRename.Location));
        Assert.Contains(activeProfile.SaveListEntries, x=> x.Name == newName);
    }


    [Fact]
    public void RenameSelectedEntry_WithSavefileSelectedEntry_RenamesSavefile()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile toRename = activeProfile.Folder.Children.OfType<Savefile>().First();
        _saveViewModel.SelectedEntry = toRename;
        string newName = "New Name";
        
        _saveViewModel.RenameSelectedEntry(newName);

        Assert.Equal(newName, _saveViewModel.SelectedEntry.Name);
        Assert.True(File.Exists(toRename.Location));
        Assert.Contains(activeProfile.SaveListEntries, x=> x.Name == newName);
    }

    #endregion




    #region RefreshProfiles Tests

    [Fact]
    public void RefreshProfiles_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.RefreshProfiles);
        Assert.Null(exception);
    }


    [Fact]
    public void RefreshProfiles_WithNewProfilesInFilesystem_LoadsNewProfiles()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        string profileToBeDiscoveredPath = Path.Join(_game1ProfilesDirectoryPath, "ProfileToBeDiscovered");
        Directory.CreateDirectory(profileToBeDiscoveredPath);

        _saveViewModel.RefreshProfiles();
        
        Assert.Contains(game.Profiles, x => x.Folder.Location == profileToBeDiscoveredPath);
    }

    #endregion




    #region ImportSavefile Tests

    [Fact]
    public void ImportSavefile_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.ImportSavefile);
        Assert.Null(exception);
    }


    [Fact]
    public void ImportSavefile_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.ImportSavefile);
        Assert.Null(exception);
    }


    [Fact]
    public void ImportSavefile_WithNullSavefileLocation_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.ImportSavefile);
        Assert.Null(exception);
    }


    [Fact]
    public void ImportSavefile_WithNullSelectedEntry_ImportsSavefileToProfileFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = null;
        Profile activeProfile = game.ActiveProfile!;

        _saveViewModel.ImportSavefile();

        Assert.Equal(Game1SavefileName, _saveViewModel.SelectedEntry?.Name);        
        Assert.True(File.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(activeProfile.Folder.Children, x => x.Name == Game1SavefileName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == Game1SavefileName);
    }


    [Fact]
    public void ImportSavefile_WithFolderSelectedEntry_ImportsSavefileToProfileFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Folder importTarget = activeProfile.Folder.Children.OfType<Folder>().First();
        _saveViewModel.SelectedEntry = importTarget;        

        _saveViewModel.ImportSavefile();

        Assert.Equal(Game1SavefileName, _saveViewModel.SelectedEntry?.Name);        
        Assert.True(File.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(importTarget.Children, x => x.Name == Game1SavefileName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == Game1SavefileName);
    }


    [Fact]
    public void ImportSavefile_WithSavefileSelectedEntry_ImportsSavefileToProfileFolder()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        Savefile importTarget = activeProfile.Folder.Children.OfType<Savefile>().First();
        _saveViewModel.SelectedEntry = importTarget;        

        _saveViewModel.ImportSavefile();

        Assert.Equal(Game1SavefileName, _saveViewModel.SelectedEntry?.Name);        
        Assert.True(File.Exists(_saveViewModel.SelectedEntry?.Location));
        Assert.Contains(importTarget.Parent!.Children, x => x.Name == Game1SavefileName);
        Assert.Contains(activeProfile.SaveListEntries, x => x.Name == Game1SavefileName);
    }


    [Fact]
    public void ImportSavefile_WhenSavefileNoLongerExists_ThrowsAndResetsGameSavefileLocation()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Profile activeProfile = game.ActiveProfile!;
        File.Delete(_game1SavefilePath);

        Assert.Throws<SavefileNotFoundException>(_saveViewModel.ImportSavefile);
        Assert.Null(game.SavefileLocation);
    }

    #endregion




    #region LoadSelectedEntry Tests

    [Fact]
    public void LoadSelectedEntry_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WithNullSavefileLocation_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WithNullSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = null;

        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WithFolderSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Folder>().First();

        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WithSavefileSelectedEntry_Executes()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Savefile>().First();

        Exception? exception = Record.Exception(_saveViewModel.LoadSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void LoadSelectedEntry_WhenSavefileNoLongerExists_ThrowsAndResetsGameSavefileLocation()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Savefile>().First();
        File.Delete(_game1SavefilePath);

        Assert.Throws<SavefileNotFoundException>(_saveViewModel.LoadSelectedEntry);
        Assert.Null(game.SavefileLocation);
    }

    #endregion




    #region ReplaceSelectedEntry Tests

    [Fact]
    public void ReplaceSelectedEntry_WithNullActiveGame_ReturnsWithoutThrowing()
    {
        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WithNullActiveProfile_ReturnsWithoutThrowing()
    {
        Game game = new("game");
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WithNullSavefileLocation_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WithNullSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = null;

        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WithFolderSelectedEntry_ReturnsWithoutThrowing()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Folder>().First();

        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WithSavefileSelectedEntry_Executes()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Savefile>().First();

        Exception? exception = Record.Exception(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(exception);
    }


    [Fact]
    public void ReplaceSelectedEntry_WhenSavefileNoLongerExists_ThrowsAndResetsGameSavefileLocation()
    {
        Game game = new("game") { ProfilesDirectory = _game1ProfilesDirectoryPath, SavefileLocation = _game1SavefilePath };
        _saveViewModel.Games = [game];
        _saveViewModel.ActiveGame = game;
        _saveViewModel.SelectedEntry = game.ActiveProfile!.Folder.Children.OfType<Savefile>().First();
        File.Delete(_game1SavefilePath);

        Assert.Throws<SavefileNotFoundException>(_saveViewModel.ReplaceSelectedEntry);
        Assert.Null(game.SavefileLocation);
    }

    #endregion
}
