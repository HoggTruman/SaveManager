using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.Appdata;
using SaveManager.Services.FilesystemService;
using SaveManager.ViewModels;

namespace SaveManagerTests.ViewModels;

[Collection("Sequential")]
public class GameEditViewModelTests
{
    private readonly GameEditViewModel _gameEditViewModel;
    
    public GameEditViewModelTests()
    {
        // mock out filesystem interaction
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());

        IAppdataService mockAppdataService = Mock.Of<IAppdataService>();
        _gameEditViewModel = new(mockAppdataService);
    }


    #region AddGame Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("      ")]
    public void AddGame_WithInvalidName_ThrowsValidationException(string name)
    {
        Assert.Throws<ValidationException>(() => _gameEditViewModel.AddGame(name));
    }


    [Fact]
    public void AddGame_WhenNameAlreadyUsed_ThrowsValidationException()
    {
        string name = "test";
        _gameEditViewModel.AddGame(new(name));

        Assert.Throws<ValidationException>(() => _gameEditViewModel.AddGame(name));
    }


    [Fact]
    public void AddGame_WithValidInput_AddsGame()
    {
        string name = "test";
        _gameEditViewModel.AddGame(new(name));

        Assert.Contains(_gameEditViewModel.Games, x => x.Name == name);
        Assert.Equal(name, _gameEditViewModel.ActiveGame?.Name);
    }

    #endregion




    #region RenameGame Tests

    [Fact]
    public void RenameGame_WithoutActiveGame_DoesNotThrowException()
    {
        Exception? exception = Record.Exception(() => _gameEditViewModel.RenameGame("test"));
        Assert.Null(exception);        
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("      ")]
    public void RenameGame_WithInvalidName_ThrowsValidationException(string name)
    {
        Game game = new("test");
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.RenameGame(name));
    }


    [Fact]
    public void RenameGame_WhenAnotherGameUsesName_ThrowsValidationException()
    {
        Game game = new("test");
        Game gameToRename = new("gameToRename");
        _gameEditViewModel.Games = [game, gameToRename];
        _gameEditViewModel.ActiveGame = gameToRename;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.RenameGame(game.Name));
    }


    [Fact]
    public void RenameGame_WhenGamesOwnName_ThrowsValidationException()
    {
        Game game = new("test");
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.RenameGame(game.Name));
    }


    [Fact]
    public void RenameGame_WithValidInput_RenamesGame()
    {
        string newName = "newName";
        Game game = new("test");
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        _gameEditViewModel.RenameGame(newName);

        Assert.Contains(_gameEditViewModel.Games, x => x.Name == newName);
        Assert.Equal(newName, _gameEditViewModel.ActiveGame.Name);
    }

    #endregion




    #region RemoveGame Tests

    [Fact]
    public void RemoveGame_WithoutActiveGame_DoesNotThrow()
    {
        Exception? exception = Record.Exception(_gameEditViewModel.RemoveGame);
        Assert.Null(exception);  
    }


    [Fact]
    public void RemoveGame_WhenOnlyGame_RemovesGame()
    {
        Game game = new("test");
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        _gameEditViewModel.RemoveGame();

        Assert.Empty(_gameEditViewModel.Games);
        Assert.Null(_gameEditViewModel.ActiveGame);
    }


    [Fact]
    public void RemoveGame_WhenMultipleGames_SetsDifferentActiveGame()
    {
        Game gameToRemove = new("gameToRemove");
        Game remainingGame = new("remainingGame");
        _gameEditViewModel.Games = [gameToRemove, remainingGame];
        _gameEditViewModel.ActiveGame = gameToRemove;

        _gameEditViewModel.RemoveGame();

        Assert.DoesNotContain(gameToRemove, _gameEditViewModel.Games);
        Assert.Equal(remainingGame, _gameEditViewModel.ActiveGame);
    }

    #endregion




    #region SetSavefileLocation Tests

    [Fact]
    public void SetSavefileLocation_WithNullActiveGame_DoesNotThrow()
    {
        Exception exception = Record.Exception(() => _gameEditViewModel.SetSavefileLocation("test"));
        Assert.Null(exception);
    }


    [Fact]
    public void SetSavefileLocation_WithSameLocation_DoesNotThrow()
    {
        Game game = new("test")
        {
            SavefileLocation = "saveFileLocation"
        };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Exception exception = Record.Exception(() => _gameEditViewModel.SetSavefileLocation(game.SavefileLocation));
        Assert.Null(exception);
    }


    [Fact]
    public void SetSavefileLocation_WithLocationOfAnotherGame_ThrowsValidationException()
    {
        Game game = new("test")
        {
            SavefileLocation = "saveFileLocation"
        };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetSavefileLocation(game.SavefileLocation));
    }


    [Fact]
    public void SetSavefileLocation_WithLocationWithinItsProfilesDirectory_ThrowsValidationException()
    {
        string profilesDirectory = @"C:\Folder";
        string savefileLocation = Path.Join(profilesDirectory, "savefile.file");

        Game game = new("test") { ProfilesDirectory = profilesDirectory };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetSavefileLocation(savefileLocation));
    }


    [Fact]
    public void SetSavefileLocation_WithLocationWithinProfilesDirectoryOfAnotherGame_ThrowsValidationException()
    {
        string profilesDirectory = @"C:\Folder";
        string savefileLocation = Path.Join(profilesDirectory, "savefile.file");

        Game game = new("test") { ProfilesDirectory = profilesDirectory };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetSavefileLocation(savefileLocation));
    }

    #endregion




    #region SetProfilesDirectory Tests

    [Fact]
    public void SetProfilesDirectory_WithNullActiveGame_DoesNotThrow()
    {
        Exception exception = Record.Exception(() => _gameEditViewModel.SetProfilesDirectory("test"));
        Assert.Null(exception);
    }


    [Fact]
    public void SetProfilesDirectory_WithSameLocation_DoesNotThrow()
    {
        Game game = new("test") { ProfilesDirectory = "ProfilesDirectory" };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Exception exception = Record.Exception(() => _gameEditViewModel.SetProfilesDirectory(game.ProfilesDirectory));
        Assert.Null(exception);
    }


    [Fact]
    public void SetProfilesDirectory_WithProfilesDirectoryOfAnotherGame_ThrowsValidationException()
    {
        Game game = new("test") { ProfilesDirectory = "ProfilesDirectory" };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetProfilesDirectory(game.ProfilesDirectory));
    }


    [Fact]
    public void SetProfilesDirectory_WithParentOfOwnProfilesDirectory_DoesNotThrow()
    {
        string parentDirectory = @"C:\Folder\";
        string childDirectory = Path.Join(parentDirectory, "Subfolder");

        Game game = new("test") { ProfilesDirectory = childDirectory };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Exception exception = Record.Exception(() => _gameEditViewModel.SetProfilesDirectory(parentDirectory));
        Assert.Null(exception);
    }


    [Fact]
    public void SetProfilesDirectory_WithChildOfOwnProfilesDirectory_DoesNotThrow()
    {
        string parentDirectory = @"C:\Folder\";
        string childDirectory = Path.Join(parentDirectory, "Subfolder");

        Game game = new("test") { ProfilesDirectory = parentDirectory };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Exception exception = Record.Exception(() => _gameEditViewModel.SetProfilesDirectory(childDirectory));
        Assert.Null(exception);
    }


    [Fact]
    public void SetProfilesDirectory_WithParentOfAnotherProfilesDirectory_ThrowsValidationException()
    {
        string parentDirectory = @"C:\Folder\";
        string childDirectory = Path.Join(parentDirectory, "Subfolder");

        Game game = new("test") { ProfilesDirectory = childDirectory };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetProfilesDirectory(parentDirectory));
    }


    [Fact]
    public void SetProfilesDirectory_WithChildOfAnotherProfilesDirectory_ThrowsValidationException()
    {
        string parentDirectory = @"C:\Folder\";
        string childDirectory = Path.Join(parentDirectory, "Subfolder");

        Game game = new("test") { ProfilesDirectory = parentDirectory };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetProfilesDirectory(childDirectory));
    }


    [Fact]
    public void SetProfilesDirectory_WithParentOfOwnSavefileLocation_ThrowsValidationException()
    {
        string parentDirectory = @"C:\Folder\";
        string savefileLocation = Path.Join(parentDirectory, "savefile.file");

        Game game = new("test") { SavefileLocation = savefileLocation };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetProfilesDirectory(parentDirectory));
    }


    [Fact]
    public void SetProfilesDirectory_WithParentOfAnotherSavefileLocation_ThrowsValidationException()
    {
        string parentDirectory = @"C:\Folder\";
        string savefileLocation = Path.Join(parentDirectory, "savefile.file");

        Game game = new("test") { SavefileLocation = savefileLocation };
        Game gameBeingChanged = new("gameBeingChanged");
        _gameEditViewModel.Games = [game, gameBeingChanged];
        _gameEditViewModel.ActiveGame = gameBeingChanged;

        Assert.Throws<ValidationException>(() => _gameEditViewModel.SetProfilesDirectory(parentDirectory));
    }

    #endregion
}
