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




    [Fact]
    public void IsDescendant_WhenDescendant1_ReturnsTrue()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join(directory, "file.txt");
        Assert.True(PathHelpers.IsDescendant(file, directory));
    }


    [Fact]
    public void IsDescendant_WhenDescendant2_ReturnsTrue()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join(directory, "innerFolder", "file.txt");
        Assert.True(PathHelpers.IsDescendant(file, directory));
    }


    [Fact]
    public void IsDescendant_WhenDescendantAndEndingSeparator_ReturnsTrue()
    {
        string directory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string file = Path.Join(directory, "file.txt");
        Assert.True(PathHelpers.IsDescendant(file, directory));
    }


    [Fact]
    public void IsDescendant_WhenNotDescendant1_ReturnsFalse()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join("C:", "differentFolder", "file.txt");
        Assert.False(PathHelpers.IsDescendant(file, directory));
    }


    [Fact]
    public void IsDescendant_WhenNotDescendant2_ReturnsFalse()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join("C:", "folder1", "file.txt");
        Assert.False(PathHelpers.IsDescendant(file, directory));
    }


    [Fact]
    public void IsDescendant_WhenNotDescendantAndEndingSeparator_ReturnsFalse()
    {
        string directory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string file = Path.Join("C:", "differentFolder", "file.txt");
        Assert.False(PathHelpers.IsDescendant(file, directory));
    }
}
