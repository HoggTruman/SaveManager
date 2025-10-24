using SaveManager.Helpers;

namespace SaveManagerTests.Helpers;

public class PathHelpersTests
{
    [Fact]
    public void IsDescendantOf_WhenFileIsDescendant_ReturnsTrue_1()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join(directory, "file.txt");
        Assert.True(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenFileIsDescendant_ReturnsTrue_2()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join(directory, "innerFolder", "file.txt");
        Assert.True(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenFileIsDescendantAndDirectoryHasEndingSeparator_ReturnsTrue()
    {
        string directory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string file = Path.Join(directory, "file.txt");
        Assert.True(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenFileIsNotDescendant_ReturnsFalse_1()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join("C:", "differentFolder", "file.txt");
        Assert.False(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenFileIsNotDescendant_ReturnsFalse_2()
    {
        string directory = Path.Join("C:", "folder");
        string file = Path.Join("C:", "folder1", "file.txt");
        Assert.False(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenFileIsNotDescendantAndDirectoryHasEndingSeparator_ReturnsFalse()
    {
        string directory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string file = Path.Join("C:", "differentFolder", "file.txt");
        Assert.False(file.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsDescendant_ReturnsTrue_1()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join(rightDirectory, "innerFolder");
        Assert.True(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsDescendant_ReturnsTrue_2()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join(rightDirectory, "innerFolder", "innerInnerFolder");
        Assert.True(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsDescendantAndRightDirectoryHasEndingSeparator_ReturnsTrue()
    {
        string rightDirectory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string leftDirectory = Path.Join(rightDirectory, "innerFolder");
        Assert.True(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsDescendantAndHasEndingSeparator_ReturnsTrue()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join(rightDirectory, "innerFolder") + Path.DirectorySeparatorChar;
        Assert.True(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsDescendantAndBothHaveEndingSeparator_ReturnsTrue()
    {
        string rightDirectory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string leftDirectory = Path.Join(rightDirectory, "innerFolder") + Path.DirectorySeparatorChar;
        Assert.True(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsNotDescendant_ReturnsFalse_1()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join("C:", "differentFolder", "differentInnerFolder");
        Assert.False(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsNotDescendant_ReturnsFalse_2()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join("C:", "folder1", "folder2");
        Assert.False(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsNotDescendantAndRightDirectoryHasEndingSeparator_ReturnsFalse()
    {
        string rightDirectory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string leftDirectory = Path.Join("C:", "differentFolder", "differentInnerFolder");
        Assert.False(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsNotDescendantAndHasEndingSeparator_ReturnsFalse()
    {
        string rightDirectory = Path.Join("C:", "folder");
        string leftDirectory = Path.Join("C:", "differentFolder", "differentInnerFolder") + Path.DirectorySeparatorChar;
        Assert.False(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenLeftDirectoryIsNotDescendantAndBothHaveEndingSeparator_ReturnsFalse()
    {
        string rightDirectory = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string leftDirectory = Path.Join("C:", "differentFolder", "differentInnerFolder") + Path.DirectorySeparatorChar;
        Assert.False(leftDirectory.IsDescendantOf(rightDirectory));
    }


    [Fact]
    public void IsDescendantOf_WhenEqual_ReturnsFalse()
    {
        string directory = Path.Join("C:", "folder");
        Assert.False(directory.IsDescendantOf(directory));
    }


    [Fact]
    public void IsDescendantOf_WhenOnlyArgumentHasEndingSeparator_ReturnsFalse()
    {
        string withoutSeparator = Path.Join("C:", "folder");
        string withSeparator = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        Assert.False(withoutSeparator.IsDescendantOf(withSeparator));
    }


    [Fact]
    public void IsDescendantOf_WhenOnlyCallerHasEndingSeparator_ReturnsFalse()
    {
        string withSeparator = Path.Join("C:", "folder") + Path.DirectorySeparatorChar;
        string withoutSeparator = Path.Join("C:", "folder");        
        Assert.False(withSeparator.IsDescendantOf(withoutSeparator));
    }
}
