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
        Savefile file = new(Path.Join(Directory.GetCurrentDirectory(), "file.file"), null);
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
        Savefile fileToCopy = folder.Children.OfType<Savefile>().First();
        Folder targetFolder = folder.Children.OfType<Folder>().First();
        int beforeChildrenCount = targetFolder.Children.Count;

        Savefile copyResult = fileToCopy.CopyTo(targetFolder);
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
        Savefile fileToCopy = folder.Children.OfType<Savefile>().First();
        int beforeChildrenCount = folder.Children.Count;

        Savefile copyResult = fileToCopy.CopyTo(folder);
        Assert.Equal(beforeChildrenCount + 1, folder.Children.Count);
        Assert.Equal(folder, copyResult.Parent);
    }


    [Fact]
    public void CopyTo_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile fileToCopy = new(Path.Join(folder.Location, "aFileThatDoesNotExist.file"), null);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(folder));
    }


    [Fact]
    public void CopyTo_WhenParentDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile fileToCopy = folder.Children.OfType<Savefile>().First();
        Directory.Delete(folder.Location, true);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(folder));
    }


    [Fact]
    public void CopyTo_WhenFileExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder destination = folder.Children.OfType<Folder>().First();
        Savefile fileToCopy = folder.Children.OfType<Savefile>().First();
        string copyLocation = Path.Join(destination.Location, fileToCopy.Name);
        using FileStream stream = File.Create(copyLocation);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(destination));
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
        Savefile testFile = folder.Children.OfType<Savefile>().First();
        string oldName = testFile.Name;
        string newName = "renamed" + testFile.Name;
        testFile.Rename(newName);

        Assert.Equal(newName, testFile.Name);
        Assert.DoesNotContain(folder.Children, x => x.Name == oldName);
    }


    [Fact]
    public void Rename_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemMismatchException>(() => testFile.Rename("newName"));
    }


    [Fact]
    public void Rename_WhenFileExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile testFile = folder.Children.OfType<Savefile>().First();
        string newName = "newName.file";
        string renamedLocation = Path.Join(folder.Location, newName);
        using FileStream stream = File.Create(renamedLocation);

        Assert.Throws<FilesystemMismatchException>(() => testFile.Rename(newName));
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
        Savefile testFile = folder.Children.OfType<Savefile>().First();
        int oldChildCount = folder.Children.Count;
        testFile.Delete();

        Assert.DoesNotContain(testFile, folder.Children);
        Assert.Equal(oldChildCount - 1, folder.Children.Count);
    }


    [Fact]
    public void Delete_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemMismatchException>(testFile.Delete);
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
        Savefile movingFile = folder.Children.OfType<Savefile>().First();
        Folder destination = folder.Children.OfType<Folder>().First();
        movingFile.Move(destination);

        Assert.Equal(destination, movingFile.Parent);
        Assert.Contains(movingFile, destination.Children);
        Assert.DoesNotContain(movingFile, folder.Children);
        Assert.Equal(destination.Location, Path.GetDirectoryName(movingFile.Location));
    }


    [Fact]
    public void Move_WhenMovingFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile movingFile = folder.Children.OfType<Savefile>().First();
        Folder destination = folder.Children.OfType<Folder>().First();
        File.Delete(movingFile.Location);

        Assert.Throws<FilesystemMismatchException>(() => movingFile.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile movingFile = folder.Children.OfType<Savefile>().First();
        Folder destination = folder.Children.OfType<Folder>().First();
        Directory.Delete(destination.Location);

        Assert.Throws<FilesystemMismatchException>(() => movingFile.Move(destination));
    }


    [Fact]
    public void Move_WhenNewLocationExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile movingFile = folder.Children.OfType<Savefile>().First();
        Folder destination = folder.Children.OfType<Folder>().First();
        string newFileLocation = Path.Join(destination.Location, movingFile.Name);
        using FileStream stream = File.Create(newFileLocation);

        Assert.Throws<FilesystemMismatchException>(() => movingFile.Move(destination));
    }

    #endregion




    #region OverwriteContents

    [Fact]
    public void OverwriteContents_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile testFile = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Savefile fileToCopy = folder.Children.OfType<Savefile>().First();
        Assert.Throws<FilesystemMismatchException>(() => testFile.OverwriteContents(fileToCopy));
    }


    [Fact]
    public void OverwriteContents_WhenFileToCopyDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Savefile testFile = folder.Children.OfType<Savefile>().First();
        Savefile fileToCopy = new(Path.Join(testCaseDirectory, "fileThatDoesNotExist.file"), folder);
        Assert.Throws<FilesystemMismatchException>(() => testFile.OverwriteContents(fileToCopy));
    }

    #endregion




    #region GenerateFilename

    [Fact]
    public void GenerateFilename_WithNoSiblings_GivesSameName()
    {
        IEnumerable<IFilesystemItem> siblings = [];
        string name = "file.file";
        string result = Savefile.GenerateFileName(name, siblings);
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
        string result = Savefile.GenerateFileName(name, siblings);
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
        string result = Savefile.GenerateFileName(name, siblings);
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
        string result = Savefile.GenerateFileName(name, siblings);
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
        string result = Savefile.GenerateFileName(name, siblings);
        Assert.Equal(expected, result);
    }

    #endregion
}
