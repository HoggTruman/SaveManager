using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.Appdata;
using SaveManager.ViewModels;

namespace SaveManagerTests.ViewModels;

public class GameEditViewModelTests
{
    private readonly GameEditViewModel _gameEditViewModel;
    
    public GameEditViewModelTests()
    {
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
        // write once figured out filesystem method coupling
        Assert.Fail();
    }


    [Fact]
    public void SetSavefileLocation_WithLocationWithinProfilesDirectoryOfAnotherGame_ThrowsValidationException()
    {
        // write once figured out filesystem method coupling
        Assert.Fail();
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
        // write once figured out filesystem method coupling
        Assert.Fail();
        Game game = new("test")
        {
            ProfilesDirectory = "ProfilesDirectory" // will load files / directories from filesystem currently
        };
        _gameEditViewModel.Games = [game];
        _gameEditViewModel.ActiveGame = game;

        Exception exception = Record.Exception(() => _gameEditViewModel.SetProfilesDirectory(game.ProfilesDirectory));
        Assert.Null(exception);
    }

    #endregion
}
