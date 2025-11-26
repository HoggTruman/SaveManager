using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

public class MockedFolderTests
{
    private readonly string _root = Path.Join(Directory.GetCurrentDirectory(), "Test");


    #region CreateChildFolder Tests

    [Fact]
    public void CreateChildFolder_UpdatesChildren()
    {
        string parentPath = Path.Join(_root, "Parent");
        string childName = "Child";
        string newChildPath = Path.Join(parentPath, childName);            
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(newChildPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder newChild = parent.CreateChildFolder(childName);

        Assert.Contains(newChild, parent.Children);
        Assert.Equal(parent, newChild.Parent);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(@"\\\\")]
    public void CreateChildFolder_WithInvalidName_ThrowsValidationException(string name)
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());

        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(_root, "Parent"), null);

        Assert.Throws<ValidationException>(() => parent.CreateChildFolder(name));
    }


    [Fact]
    public void CreateChildFolder_WhenAChildAlreadyHasName_ThrowsValidationException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string childName = "Child";
        string childPath = Path.Join(parentPath, childName);    
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder child = FilesystemItemFactory.NewFolder(childPath, parent);
        parent.Children = [child];

        Assert.Throws<ValidationException>(() => parent.CreateChildFolder(childName));
    }


    [Fact]
    public void CreateChildFolder_WhenParentDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");       
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);

        Assert.Throws<FilesystemMismatchException>(() => parent.CreateChildFolder("New Folder"));
    }


    [Fact]
    public void CreateChildFolder_WhenChildExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string childName = "Child";
        string newChildPath = Path.Join(parentPath, childName);            
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(newChildPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);

        Assert.Throws<FilesystemMismatchException>(() => parent.CreateChildFolder(childName));
    }

    #endregion

}
