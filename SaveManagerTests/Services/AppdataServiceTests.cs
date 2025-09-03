using SaveManager.DTOs;
using SaveManager.Services;
using System.Reflection;
using System.Xml.Linq;

namespace SaveManagerTests.Services;

public class AppdataServiceTests
{
    public static GameDTO TestGameDTO => new() { Name = "test", ProfilesDirectory = "testDir", SavefileLocation = "testLocation" };




    #region Tree Modification Tests (USES CONVERSION METHODS)

    [Fact]
    public void ReplaceGamesInDocument_WithEmpty_LeavesDocumentWithNoGames()
    {
        XDocument document = AppdataService.DefaultDocument;
        XElement gamesElement = document.Root!.Element(AppdataService.GamesName)!;
        gamesElement.Add(new XElement(AppdataService.ConvertToXElement(TestGameDTO)));

        AppdataService.ReplaceGamesInDocument(document, []);
        Assert.Empty(gamesElement.Elements());
    }


    [Fact]
    public void ReplaceGamesInDocument_WithNewGames_ReplacesGames()
    {
        XDocument document = AppdataService.DefaultDocument;
        XElement gamesElement = document.Root!.Element(AppdataService.GamesName)!;
        gamesElement.Add(new XElement(AppdataService.ConvertToXElement(TestGameDTO)));
        List<GameDTO> newGames = [
            new() { Name = "first" },
            new() { Name = "second" },
            new() { Name = "third" }];

        AppdataService.ReplaceGamesInDocument(document, newGames);
        
        Assert.Equal(newGames.Count, gamesElement.Elements().Count());
        
        // check new games each have a corresponding Name element.
        IEnumerable<XElement> descendants = gamesElement.Descendants();
        foreach (GameDTO game in newGames)
            Assert.NotNull(descendants.FirstOrDefault(x => x.Name.LocalName == nameof(GameDTO.Name) && x.Value == game.Name));
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
    public void ConvertToXElement_ConvertsGameAppdataDTO()
    {
        GameDTO gameAppdataDTO = TestGameDTO;

        XElement gameElement = AppdataService.ConvertToXElement(TestGameDTO);

        foreach (XElement element in gameElement.Elements())
        {
            string propertyName = element.Name.LocalName;
            PropertyInfo? prop = typeof(GameDTO).GetProperty(propertyName);
            Assert.NotNull(prop);
            Assert.Equal(prop.GetValue(gameAppdataDTO), element.Value);
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
    public void ConvertFromXElement_WithGameAppdataDTOElement_ReturnsGameAppDTO()
    {
        GameDTO expectedGameDTO = TestGameDTO;
        var children = typeof(GameDTO).GetProperties().Select(prop => new XElement(prop.Name, prop.GetValue(expectedGameDTO)));

        XElement gameElement = new("Game", children);
        GameDTO result = AppdataService.ConvertFromXElement<GameDTO>(gameElement);

        Assert.Equivalent(expectedGameDTO, result, true);
    }

    #endregion
}
