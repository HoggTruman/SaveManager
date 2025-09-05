using SaveManager.Helpers;

namespace SaveManagerTests.Helpers;

public class PathHelpersTests
{
    [Fact]
    public void AreParentChildDirectories_ReturnsTrue_WhenParentAndChild()
    {
        string parent = Path.Join("C:","Parent");
        string child = Path.Join(parent, "Child");
        Assert.True(PathHelpers.AreParentChildDirectories(parent, child));
        Assert.True(PathHelpers.AreParentChildDirectories(child, parent));
    }


    [Fact]
    public void AreParentChildDirectories_ReturnsFalse_WhenSiblings()
    {
        string first = @"C:\somerandomdir";
        string second = @"C:\someotherdir";
        Assert.False(PathHelpers.AreParentChildDirectories(first, second));
        Assert.False(PathHelpers.AreParentChildDirectories(second, first));
    }


    [Fact]
    public void AreParentChildDirectories_ReturnsFalse_WhenNotParentChild()
    {
        string first = Path.Join("C:","a", "b");
        string second = Path.Join("C:","b", "f", "g");
        Assert.False(PathHelpers.AreParentChildDirectories(first, second));
        Assert.False(PathHelpers.AreParentChildDirectories(second, first));
    }


    [Fact]
    public void AreParentChildDirectories_ReturnsFalse_WhenGivenSamePath()
    {
        string path = Path.Join("C:", "Folder");
        Assert.False(PathHelpers.AreParentChildDirectories(path, path));
    }    
}
