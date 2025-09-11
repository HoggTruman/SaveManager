namespace SaveManagerTests.Models;

public class FileTests
{
    [Fact]
    public void UpdateLocation_UpdatesLocation()
    {
        SaveManager.Models.File file = new(Path.Join(Directory.GetCurrentDirectory(), "file.file"), null);
        string newLocation = Path.Join(Directory.GetCurrentDirectory(), "newlocation.file");
        file.UpdateLocation(newLocation);
        Assert.Equal(newLocation, file.Location);
    }
}
