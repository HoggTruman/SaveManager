using SaveManager.Models;
using SaveManagerTests.TestHelpers;
using System.Reflection;

namespace SaveManagerTests.Models;

public class FileTests : IClassFixture<FilesystemFixture>
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


    public FileTests(FilesystemFixture filesystemFixture)
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
    public void UpdateLocation_UpdatesLocation()
    {
        SaveManager.Models.File file = new(Path.Join(Directory.GetCurrentDirectory(), "file.file"), null);
        string newLocation = Path.Join(Directory.GetCurrentDirectory(), "newlocation.file");
        file.UpdateLocation(newLocation);
        Assert.Equal(newLocation, file.Location);
    }


    [Fact]
    public void Rename_RenamesFile()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        string oldName = testFile.Name;
        string newName = "renamed" + testFile.Name;
        testFile.Rename(newName);

        Assert.Equal(newName, testFile.Name);
        Assert.DoesNotContain(folder.Children, x => x.Name == oldName);
    }


    [Fact]
    public void Delete_DeletesFile()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        int oldChildCount = folder.Children.Count;
        testFile.Delete();

        Assert.DoesNotContain(testFile, folder.Children);
        Assert.Equal(oldChildCount - 1, folder.Children.Count);
    }
}
