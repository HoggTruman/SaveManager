using SaveManager.Helpers;

namespace SaveManagerTests.Helpers;

public class PathHelpersTests
{
    [Fact]
    public void IsEitherDirectoryDescendant_WhenParentAndChild_ReturnsTrue()
    {
        string parent = Path.Join("C:","Parent");
        string child = Path.Join(parent, "Child");
        Assert.True(PathHelpers.IsEitherDirectoryDescendant(parent, child));
        Assert.True(PathHelpers.IsEitherDirectoryDescendant(child, parent));
    }


    [Fact]
    public void IsEitherDirectoryDescendant_WhenParentAndChildWithEndingSeparator_ReturnsTrue()
    {
        string parent = Path.Join("C:","Parent") + Path.DirectorySeparatorChar;
        string child = Path.Join(parent, "Child") + Path.DirectorySeparatorChar;
        Assert.True(PathHelpers.IsEitherDirectoryDescendant(parent, child));
        Assert.True(PathHelpers.IsEitherDirectoryDescendant(child, parent));
    }


    [Fact]
    public void IsEitherDirectoryDescendant_WhenSiblings_ReturnsFalse()
    {
        string first = @"C:\somerandomdir";
        string second = @"C:\someotherdir";
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(first, second));
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(second, first));
    }


    [Fact]
    public void IsEitherDirectoryDescendant_WhenNotParentChild_ReturnsFalse()
    {
        string first = Path.Join("C:","a", "b");
        string second = Path.Join("C:","b", "f", "g");
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(first, second));
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(second, first));
    }


    [Fact]
    public void IsEitherDirectoryDescendant_WhenNotParentChildWithEndingSeparator_ReturnsFalse()
    {
        string first = Path.Join("C:","a", "b") + Path.DirectorySeparatorChar;
        string second = Path.Join("C:","b", "f", "g") + Path.DirectorySeparatorChar;
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(first, second));
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(second, first));
    }


    [Fact]
    public void IsEitherDirectoryDescendant_WhenGivenSamePath_ReturnsFalse()
    {
        string path = Path.Join("C:", "Folder");
        Assert.False(PathHelpers.IsEitherDirectoryDescendant(path, path));
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
