using SaveManager.Models;
using SaveManager.Services;
using System.Reflection;
using System.Xml.Linq;

namespace SaveManagerTests.Services;

public class AppdataServiceTests
{
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
