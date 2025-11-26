using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
public class MockedSavefileTests
{


    public MockedSavefileTests()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
    }




    #region Location Tests

    [Fact]
    public void LocationSetter_UpdatesLocation()
    {
        string location = Path.Join(@"C:\Root", "file.file");
        string newLocation = Path.Join(@"C:\Root", "newlocation.file");
        Savefile file = FilesystemItemFactory.NewSavefile(location, null);
        
        file.Location = newLocation;
        Assert.Equal(newLocation, file.Location);
    }

    #endregion




    #region CopyTo Tests

    [Fact]
    public void CopyTo_FolderWithoutMatchingFilename_CopiesFile()
    {
        string destinationFolderPath = @"C:\Root\Folder\";
        string fileLocation = @"C:\Root\file.file";
        string copyLocation = @"C:\Root\Folder\file.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(fileLocation) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(copyLocation) == false));

        Folder destinationFolder = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(fileLocation, null);

        Savefile copyResult = fileToCopy.CopyTo(destinationFolder);

        Assert.Equal(fileToCopy.Name, copyResult.Name);
        Assert.Single(destinationFolder.Children);
        Assert.Contains(destinationFolder.Children, x => x.Name == fileToCopy.Name);
        Assert.Equal(destinationFolder, copyResult.Parent);
    }


    [Fact]
    public void CopyTo_FolderWithMatchingFilename_CopiesFileButGivesDifferentName()
    {
        string destinationFolderPath = @"C:\Root\Folder\";
        string fileLocation = @"C:\Root\file.file";
        string intendedCopyLocation = @"C:\Root\Folder\file.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(fileLocation) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(intendedCopyLocation) == true));

        Folder destinationFolder = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Savefile fileAtCopyLocation = FilesystemItemFactory.NewSavefile(intendedCopyLocation, destinationFolder);
        destinationFolder.Children.Add(fileAtCopyLocation);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(fileLocation, destinationFolder);

        Savefile copyResult = fileToCopy.CopyTo(destinationFolder);

        Assert.NotEqual(fileAtCopyLocation.Name, copyResult.Name);
        Assert.Equal(2, destinationFolder.Children.Count);
        Assert.Equal(destinationFolder, copyResult.Parent);
    }


    [Fact]
    public void CopyTo_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string destinationFolderPath = @"C:\Root\Folder\";
        string fileLocation = @"C:\Root\file.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(fileLocation) == false));

        Folder destinationFolder = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(fileLocation, null);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(destinationFolder));
    }


    [Fact]
    public void CopyTo_WhenDestinationFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string destinationFolderPath = @"C:\Root\Folder\";
        string fileLocation = @"C:\Root\file.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(fileLocation) == true &&
            x.DirectoryExists(destinationFolderPath) == false));

        Folder folder = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(fileLocation, null);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(folder));
    }


    [Fact]
    public void CopyTo_WhenCopyExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string destinationFolderPath = @"C:\Root\Folder\";
        string fileLocation = @"C:\Root\file.file";
        string copyLocation = @"C:\Root\Folder\file.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(fileLocation) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(copyLocation) == true));

        Folder destinationFolder = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(fileLocation, null);

        Assert.Throws<FilesystemMismatchException>(() => fileToCopy.CopyTo(destinationFolder));
    }

    #endregion
}
