using Moq;
using SaveManager.Exceptions;
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




    #region Location Tests

    [Fact]
    public void LocationSetter_UpdatesLocation()
    {
        SaveManager.Models.File file = new(Path.Join(Directory.GetCurrentDirectory(), "file.file"), null);
        string newLocation = Path.Join(Directory.GetCurrentDirectory(), "newlocation.file");
        file.Location = newLocation;
        Assert.Equal(newLocation, file.Location);
    }

    #endregion




    #region CopyTo Tests

    [Fact]
    public void CopyTo_FolderWithoutMatchingFilename_CopiesFile()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File fileToCopy = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Folder targetFolder = (Folder)folder.Children.First(x => x is Folder);
        int beforeChildrenCount = targetFolder.Children.Count;

        SaveManager.Models.File copyResult = fileToCopy.CopyTo(targetFolder);
        Assert.Equal(beforeChildrenCount + 1, targetFolder.Children.Count);
        Assert.Contains(targetFolder.Children, x => x.Name == fileToCopy.Name);
        Assert.Equal(targetFolder, copyResult.Parent);
    }


    [Fact]
    public void CopyTo_FolderWithMatchingFilename_CopiesFile()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File fileToCopy = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        int beforeChildrenCount = folder.Children.Count;

        SaveManager.Models.File copyResult = fileToCopy.CopyTo(folder);
        Assert.Equal(beforeChildrenCount + 1, folder.Children.Count);
        Assert.Equal(folder, copyResult.Parent);
    }


    [Fact]
    public void CopyTo_WhenFileDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File fileToCopy = new(Path.Join(folder.Location, "aFileThatDoesNotExist.file"), null);

        Assert.Throws<FilesystemItemNotFoundException>(() => fileToCopy.CopyTo(folder));
    }


    [Fact]
    public void CopyTo_WhenParentDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File fileToCopy = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Directory.Delete(folder.Location, true);

        Assert.Throws<FilesystemItemNotFoundException>(() => fileToCopy.CopyTo(folder));
    }


    [Fact]
    public void CopyTo_WhenFileExistsInFilesystemButNotInternally_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder destination = (Folder)folder.Children.First(x => x is Folder);
        SaveManager.Models.File fileToCopy = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        string copyLocation = Path.Join(destination.Location, fileToCopy.Name);
        using FileStream stream = System.IO.File.Create(copyLocation);

        Assert.Throws<FilesystemItemNotFoundException>(() => fileToCopy.CopyTo(destination));
    }

    #endregion




    #region Rename Tests

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
    public void Rename_WhenFileDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemItemNotFoundException>(() => testFile.Rename("newName"));
    }


    [Fact]
    public void Rename_WhenFileExistsInFilesystemButNotInternally_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        string newName = "newName.file";
        string renamedLocation = Path.Join(folder.Location, newName);
        using FileStream stream = System.IO.File.Create(renamedLocation);

        Assert.Throws<FilesystemItemNotFoundException>(() => testFile.Rename(newName));
    }

    #endregion




    #region Delete Tests

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


    [Fact]
    public void Delete_WhenFileDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemItemNotFoundException>(testFile.Delete);
    }

    #endregion




    #region Move Tests

    [Fact]
    public void Move_MovesFile()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File movingFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Folder destination = (Folder)folder.Children.First(x => x is Folder);
        movingFile.Move(destination);

        Assert.Equal(destination, movingFile.Parent);
        Assert.Contains(movingFile, destination.Children);
        Assert.DoesNotContain(movingFile, folder.Children);
        Assert.Equal(destination.Location, Path.GetDirectoryName(movingFile.Location));
    }


    [Fact]
    public void Move_WhenMovingFileDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File movingFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Folder destination = (Folder)folder.Children.First(x => x is Folder);
        System.IO.File.Delete(movingFile.Location);

        Assert.Throws<FilesystemItemNotFoundException>(() => movingFile.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File movingFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Folder destination = (Folder)folder.Children.First(x => x is Folder);
        Directory.Delete(destination.Location);

        Assert.Throws<FilesystemItemNotFoundException>(() => movingFile.Move(destination));
    }


    [Fact]
    public void Move_WhenNewLocationExistsInFilesystemButNotInternally_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File movingFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Folder destination = (Folder)folder.Children.First(x => x is Folder);
        string newFileLocation = Path.Join(destination.Location, movingFile.Name);
        using FileStream stream = System.IO.File.Create(newFileLocation);

        Assert.Throws<FilesystemItemNotFoundException>(() => movingFile.Move(destination));
    }

    #endregion




    #region OverwriteContents

    [Fact]
    public void OverwriteContents_WhenFileDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        SaveManager.Models.File fileToCopy = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        Assert.Throws<FilesystemItemNotFoundException>(() => testFile.OverwriteContents(fileToCopy));
    }


    [Fact]
    public void OverwriteContents_WhenFileToCopyDoesNotExist_ThrowsFilesystemItemNotFoundException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        SaveManager.Models.File testFile = (SaveManager.Models.File)folder.Children.First(x => x is SaveManager.Models.File);
        SaveManager.Models.File fileToCopy = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemItemNotFoundException>(() => testFile.OverwriteContents(fileToCopy));
    }

    #endregion




    #region GenerateFilename

    [Fact]
    public void GenerateFilename_WithNoSiblings_GivesSameName()
    {
        IEnumerable<IFilesystemItem> siblings = [];
        string name = "file.file";
        string result = SaveManager.Models.File.GenerateFileName(name, siblings);
        Assert.Equal(name, result);
    }


    [Fact]
    public void GenerateFilename_WithNoSiblingsSharingName_GivesSameName()
    {
        List<IFilesystemItem> siblings =
        [
            Mock.Of<IFilesystemItem>(x => x.Name == "sibling"),
            Mock.Of<IFilesystemItem>(x => x.Name == "sibling.file"),
        ];

        string name = "file.file";
        string result = SaveManager.Models.File.GenerateFileName(name, siblings);
        Assert.Equal(name, result);
    }


    [Fact]
    public void GenerateFilename_WithSiblingSharingName_GivesSuffixedName()
    {
        string name = "file.file";
        List<IFilesystemItem> siblings =
        [
            Mock.Of<IFilesystemItem>(x => x.Name == name),
        ];

        string expected = name + "_1";
        string result = SaveManager.Models.File.GenerateFileName(name, siblings);
        Assert.Equal(expected, result);
    }


    [Fact]
    public void GenerateFilename_WithSiblingSharingNameAndSuffixedName_GivesHigherSuffixedName()
    {
        string name = "file.file";
        List<IFilesystemItem> siblings =
        [
            Mock.Of<IFilesystemItem>(x => x.Name == name),
            Mock.Of<IFilesystemItem>(x => x.Name == name + "_1"),
            Mock.Of<IFilesystemItem>(x => x.Name == name + "_2"),
        ];

        string expected = name + "_3";
        string result = SaveManager.Models.File.GenerateFileName(name, siblings);
        Assert.Equal(expected, result);
    }


    [Fact]
    public void GenerateFilename_WithSiblingSharingNameAndSuffixedNameWithGap_GivesGapSuffixedName()
    {
        string name = "file.file";
        List<IFilesystemItem> siblings =
        [
            Mock.Of<IFilesystemItem>(x => x.Name == name),
            Mock.Of<IFilesystemItem>(x => x.Name == name + "_1"),
            Mock.Of<IFilesystemItem>(x => x.Name == name + "_2"),
            Mock.Of<IFilesystemItem>(x => x.Name == name + "_4"),
        ];

        string expected = name + "_3";
        string result = SaveManager.Models.File.GenerateFileName(name, siblings);
        Assert.Equal(expected, result);
    }

    #endregion
}
