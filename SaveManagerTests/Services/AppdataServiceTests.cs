using SaveManager.DTOs;
using SaveManager.Models;
using SaveManager.Services;
using System.Reflection;
using System.Xml.Linq;

namespace SaveManagerTests.Services;

public class AppdataServiceTests
{
    #region Tree Modification Tests (USES CONVERSION METHODS)

    

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
        GameAppdataDTO gameAppdataDTO = new() { Name = "test game", ProfilesDirectory = "test directory", SavefileLocation = "test savefilelocation" };

        XElement gameElement = AppdataService.ConvertToXElement(gameAppdataDTO);

        foreach (XElement element in gameElement.Elements())
        {
            string propertyName = element.Name.LocalName;
            PropertyInfo? prop = typeof(GameAppdataDTO).GetProperty(propertyName);
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
        GameAppdataDTO expectedGameDTO = new() { Name = "test game", ProfilesDirectory = "test directory", SavefileLocation = "test savefilelocation" };
        var children = typeof(GameAppdataDTO).GetProperties().Select(prop => new XElement(prop.Name, prop.GetValue(expectedGameDTO)));

        XElement gameElement = new("Game", children);
        GameAppdataDTO result = AppdataService.ConvertFromXElement<GameAppdataDTO>(gameElement);

        Assert.Equivalent(expectedGameDTO, result, true);
    }

    #endregion
}
