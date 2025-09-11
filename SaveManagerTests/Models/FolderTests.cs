using SaveManager.Models;
using SaveManagerTests.TestHelpers;
using System.Reflection;

namespace SaveManagerTests.Models;

public class FolderTests : IClassFixture<FilesystemFixture>
{
    private readonly FilesystemFixture _filesystemFixture;    

    public readonly TestFolder TestFolder = new()
    {
        Name = "RootFolder",
        Folders = 
        [
            new() { Name = "Folder1" },
            new() { Name = "Folder2", Folders = [new() { Name = "Folder3" }], Files = ["file3.file"] }
        ],
        Files = ["file1.file", "file2.file"]
    };


    public FolderTests(FilesystemFixture filesystemFixture)
    {
        _filesystemFixture = filesystemFixture;
    }


    private void Setup(string testCaseDirectory)
    {        
        if (Directory.Exists(testCaseDirectory))
            Directory.Delete(testCaseDirectory, true);

        Directory.CreateDirectory(testCaseDirectory);
        TestFolder.Create(testCaseDirectory);
    }



    [Fact]
    public void FolderLoadsChildrenOnConstruction_WithString()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Assert.True(TestFolder.IsEquivalentTo(folder));
    }


    [Fact]
    public void FolderLoadsChildrenOnConstruction_WithDirectoryInfo()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        DirectoryInfo directoryInfo = new(Path.Join(testCaseDirectory, TestFolder.Name));
        Folder folder = new(directoryInfo, null);
        Assert.True(TestFolder.IsEquivalentTo(folder));
    }


    [Fact]
    public void Create_UpdatesParentsChildren()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder newFolder = Folder.Create("New Folder", parent);
        Assert.Contains(newFolder, parent.Children);
    }


    [Fact]
    public void Rename_UpdatesParentsChildren()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder renameFolder = (Folder)parent.Children.First(x => x is Folder);
        string oldName = renameFolder.Name;
        string newName = "Renamed Folder";        
        renameFolder.Rename(newName);
        Assert.Contains(parent.Children, x => x.Name == newName);
        Assert.DoesNotContain(parent.Children, x => x.Name == oldName);
    }


    [Fact]
    public void Rename_UpdatesDescendantLocations()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder renameFolder = (Folder)parent.Children.First(x => x is Folder);      
        renameFolder.Rename("New Folder");

        IEnumerable<IFilesystemItem> children = renameFolder.Children;
        while (children.Any())
        {
            foreach (IFilesystemItem child in children)
            {
                Assert.Equal(Path.Join(child.Parent!.Location, child.Name), child.Location);
            }
            
            children = children.SelectMany(x => x is Folder folder ? folder.Children: []);
        }        
    }


    [Fact]
    public void Delete_UpdatesParentsChildren()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder deleteFolder = (Folder)parent.Children.First(x => x is Folder);
        deleteFolder.Delete();

        Assert.DoesNotContain(deleteFolder, parent.Children);
        Assert.DoesNotContain(parent.Children, x => x.Name == deleteFolder.Name);
    }


    [Fact]
    public void UpdateLocation_UpdatesDescendantLocations()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        string newLocation = Path.Join(folder.Location, "new");
        folder.UpdateLocation(newLocation);
        Assert.Equal(newLocation, folder.Location);

        IEnumerable<IFilesystemItem> children = folder.Children;
        while (children.Any())
        {
            foreach (IFilesystemItem child in children)
            {
                Assert.Equal(Path.Join(child.Parent!.Location, child.Name), child.Location);
            }
            
            children = children.SelectMany(x => x is Folder folder ? folder.Children: []);
        }  
    }
}
