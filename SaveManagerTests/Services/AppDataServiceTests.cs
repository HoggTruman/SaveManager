using SaveManager.Models;
using SaveManager.Services;
using System.Reflection;
using System.Xml.Linq;

namespace SaveManagerTests.Services;

public class AppdataServiceTests
{
    #region Tree Modification Tests (USES CONVERSION METHODS)

    [Fact]
    public void AddGameToTree_AddsGameElement()
    {
        XElement tree = AppdataService.EmptyTree;
        Game game = new() { Name = "test", ProfilesDirectory = "profiles", SavefileLocation = "savefile" };
        AppdataService.AddGameToTree(tree, game);
        var gameElements = tree.Elements().First(x => x.Name.LocalName == AppdataService.GamesName).Elements();        

        Assert.Equal(2, tree.Elements().Count());
        Assert.Single(gameElements);
    }

    [Fact]
    public void RenameGameInTree_RenamesGameElement()
    {
        XElement tree = AppdataService.EmptyTree;
        Game game = new() { Name = "test", ProfilesDirectory = "profiles", SavefileLocation = "savefile" };
        var gameElements = tree.Elements().First(x => x.Name.LocalName == AppdataService.GamesName);
        gameElements.Add(AppdataService.ConvertToXElement(game));
        string newName = "new name";

        AppdataService.RenameGameInTree(tree, game, newName);

        XElement gameElement = gameElements.Elements().First();
        XElement nameElement = gameElement.Elements().First(x => x.Name.LocalName == nameof(Game.Name));
        Assert.Equal(newName, nameElement.Value);
    }

    [Fact]
    public void DeleteGameInTree_DeletesGameElement()
    {
        XElement tree = AppdataService.EmptyTree;
        Game game = new() { Name = "test", ProfilesDirectory = "profiles", SavefileLocation = "savefile" };
        var gameElements = tree.Elements().First(x => x.Name.LocalName == AppdataService.GamesName);
        gameElements.Add(AppdataService.ConvertToXElement(game));

        AppdataService.DeleteGameInTree(tree, game);

        Assert.Empty(gameElements.Elements());
    }

    #endregion


    #region ConvertToXElement Tests

    [Fact]
    public void ConvertToXElement_WithNull_ThrowsArgumentNullException()
    {
        string nullObject = null!;
        Assert.Throws<ArgumentNullException>(() => AppdataService.ConvertToXElement(nullObject));
    }

    [Fact]
    public void ConvertToXElement_ConvertsGame()
    {
        Game game = new() { Name = "test game", ProfilesDirectory = "test directory", SavefileLocation = "test savefilelocation" };

        XElement gameElement = AppdataService.ConvertToXElement(game);

        Assert.Equal(game.GetType().Name, gameElement.Name);

        foreach (XElement element in gameElement.Elements())
        {
            string propertyName = element.Name.LocalName;
            PropertyInfo? prop = typeof(Game).GetProperty(propertyName);
            Assert.NotNull(prop);
            Assert.Equal(prop.GetValue(game), element.Value);
        }
    }

    #endregion


    #region ConvertFromXElement Tests

    [Fact]
    public void ConvertFromXElement_WithNull_ThrowsArgumentNullException()
    {
        XElement nullObject = null!;
        Assert.Throws<ArgumentNullException>(() => AppdataService.ConvertFromXElement<string>(nullObject));
    }

    [Fact]
    public void ConvertFromXElement_WithGameElement_ReturnsGame()
    {
        Game expectedGame = new() { Name = "test game", ProfilesDirectory = "test directory", SavefileLocation = "test savefilelocation" };
        var children = typeof(Game).GetProperties().Select(prop => new XElement(prop.Name, prop.GetValue(expectedGame)));

        XElement gameElement = new(expectedGame.GetType().Name, children);
        Game result = AppdataService.ConvertFromXElement<Game>(gameElement);

        Assert.Equivalent(expectedGame, result, true);
    }

    #endregion
}
