using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
public class MockedSavefileTests
{
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




    #region Rename Tests

    [Fact]
    public void Rename_RenamesFile()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilename = "file.file";
        string originalFilepath = Path.Join(folderPath, originalFilename);
        string renamedFilename = "renamedFile.file";
        string renamedFilepath = Path.Join(folderPath, renamedFilename);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == true &&
            x.DirectoryExists(folderPath) == true && 
            x.FileExists(renamedFilepath) == false));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        folder.Children.Add(file);

        file.Rename(renamedFilename);

        Assert.Equal(renamedFilename, file.Name);
        Assert.DoesNotContain(folder.Children, x => x.Name == originalFilename);
    }


    [Fact]
    public void Rename_WithoutParent_ThrowsInvalidOperationException()
    {
        // A file without a parent can't check its siblings for naming collisions so this is disallowed.
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Savefile file = FilesystemItemFactory.NewSavefile(@"C:\Root\Folder\file.file", null);
        Assert.Throws<InvalidOperationException>(() => file.Rename("newName.file"));
    }


    [Fact]
    public void Rename_WithOwnName_ThrowsValidationException()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilepath = Path.Join(folderPath, "file.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == true &&
            x.DirectoryExists(folderPath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        folder.Children.Add(file);

        Assert.Throws<ValidationException>(() => file.Rename(file.Name));
    }


    [Fact]
    public void Rename_WithSiblingsName_ThrowsValidationException()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilepath = Path.Join(folderPath, "file.file");
        string siblingFilepath = Path.Join(folderPath, "sibling.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == true &&
            x.DirectoryExists(folderPath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        Savefile sibling = FilesystemItemFactory.NewSavefile(siblingFilepath, folder);
        folder.Children = [file, sibling];

        Assert.Throws<ValidationException>(() => file.Rename(sibling.Name));
    }


    [Fact]
    public void Rename_WithInvalidCharacterInFilename_ThrowsValidationException()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilepath = Path.Join(folderPath, "file.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == true &&
            x.DirectoryExists(folderPath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        folder.Children.Add(file);

        Assert.Throws<ValidationException>(() => file.Rename(@"\\\\"));
    }


    [Fact]
    public void Rename_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilepath = Path.Join(folderPath, "file.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == false &&
            x.DirectoryExists(folderPath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        folder.Children.Add(file);

        Assert.Throws<FilesystemMismatchException>(() => file.Rename("newName"));
    }


    [Fact]
    public void Rename_WhenRenamedFilepathExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        string folderPath = @"C:\Root\Folder\";
        string originalFilepath = Path.Join(folderPath, "file.file");
        string newName = "renamedFile.file";
        string renamedFilepath = Path.Join(folderPath, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(originalFilepath) == true &&
            x.DirectoryExists(folderPath) == true &&
            x.FileExists(renamedFilepath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, folder);
        folder.Children.Add(file);

        Assert.Throws<FilesystemMismatchException>(() => file.Rename(newName));
    }

    #endregion




    #region Delete Tests

    [Fact]
    public void Delete_RemovesFileFromParentsChildren()
    {
        string folderPath = @"C:\Root\Folder\";
        string filePath = Path.Join(folderPath, "file.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(filePath) == true &&
            x.DirectoryExists(folderPath) == true));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(filePath, folder);
        folder.Children = [file];

        file.Delete();

        Assert.DoesNotContain(file, folder.Children);
        Assert.Empty(folder.Children);
    }


    [Fact]
    public void Delete_WithoutParent_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Savefile file = FilesystemItemFactory.NewSavefile(@"C:\Root\file.file", null);
        Assert.Throws<InvalidOperationException>(file.Delete);

    }


    [Fact]
    public void Delete_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string folderPath = @"C:\Root\Folder\";
        string filePath = Path.Join(folderPath, "file.file");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(filePath) == false));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(filePath, folder);
        folder.Children = [file];

        Assert.Throws<FilesystemMismatchException>(file.Delete);
    }

    #endregion



    

    #region Move Tests

    [Fact]
    public void Move_MovesFile()
    {
        string parentFolderPath = @"C:\Root\Folder\";
        string destinationFolderPath = @"C:\Root\Target\";
        string filename = @"file.file";
        string originalFilepath = Path.Join(parentFolderPath, filename);
        string destinationFilepath = Path.Join(destinationFolderPath, filename);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentFolderPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(originalFilepath) == true &&
            x.FileExists(destinationFilepath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentFolderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, parent);
        parent.Children = [file];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        file.Move(destination);

        Assert.Equal(destination, file.Parent);
        Assert.Contains(file, destination.Children);
        Assert.DoesNotContain(file, parent.Children);
        Assert.Equal(destinationFilepath, file.Location);
    }


    [Fact]
    public void Move_WithoutParent_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Savefile file = FilesystemItemFactory.NewSavefile(@"C:\Root\file.file", null);
        Folder destination = FilesystemItemFactory.NewFolder(@"C:\Root\Target\", null);
        Assert.Throws<InvalidOperationException>(() => file.Move(destination));
    }


    [Fact]
    public void Move_ToItsOwnParent_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Folder parent = FilesystemItemFactory.NewFolder(@"C:\Root\Folder\", null);
        Savefile file = FilesystemItemFactory.NewSavefile(Path.Join(parent.Location, "file.file"), parent);
        parent.Children = [file];
        Assert.Throws<InvalidOperationException>(() => file.Move(parent));
    }


    [Fact]
    public void Move_WhenDestinationFolderContainsFileWithSameName_ThrowsValidationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Folder parent = FilesystemItemFactory.NewFolder(@"C:\Root\Folder\", null);
        Savefile file = FilesystemItemFactory.NewSavefile(Path.Join(parent.Location, "file.file"), parent);
        parent.Children = [file];
        Folder destination = FilesystemItemFactory.NewFolder(@"C:\Root\Target\", null);
        Savefile destinationFile = FilesystemItemFactory.NewSavefile(Path.Join(destination.Location, file.Name), destination);
        destination.Children = [destinationFile];
        
        Assert.Throws<ValidationException>(() => file.Move(destination));
    }


    [Fact]
    public void Move_WhenMovingFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentFolderPath = @"C:\Root\Folder\";
        string destinationFolderPath = @"C:\Root\Target\";
        string filename = @"file.file";
        string originalFilepath = Path.Join(parentFolderPath, filename);
        string destinationFilepath = Path.Join(destinationFolderPath, filename);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentFolderPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(originalFilepath) == false &&
            x.FileExists(destinationFilepath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentFolderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, parent);
        parent.Children = [file];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => file.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentFolderPath = @"C:\Root\Folder\";
        string destinationFolderPath = @"C:\Root\Target\";
        string filename = @"file.file";
        string originalFilepath = Path.Join(parentFolderPath, filename);
        string destinationFilepath = Path.Join(destinationFolderPath, filename);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentFolderPath) == true &&
            x.DirectoryExists(destinationFolderPath) == false &&
            x.FileExists(originalFilepath) == true &&
            x.FileExists(destinationFilepath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentFolderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, parent);
        parent.Children = [file];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => file.Move(destination));
    }


    [Fact]
    public void Move_WhenNewLocationExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        string parentFolderPath = @"C:\Root\Folder\";
        string destinationFolderPath = @"C:\Root\Target\";
        string filename = @"file.file";
        string originalFilepath = Path.Join(parentFolderPath, filename);
        string destinationFilepath = Path.Join(destinationFolderPath, filename);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentFolderPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.FileExists(originalFilepath) == true &&
            x.FileExists(destinationFilepath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentFolderPath, null);
        Savefile file = FilesystemItemFactory.NewSavefile(originalFilepath, parent);
        parent.Children = [file];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => file.Move(destination));
    }

    #endregion




    #region OverwriteContents

    [Fact]
    public void OverwriteContents_WhenBothFilesExist_DoesNotThrow()
    {
        string filepath = @"C:\Root\file.file";
        string filepathToCopy = @"C:\Root\fileToCopy.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(filepath) == true &&
            x.FileExists(filepathToCopy) == true));

        Savefile file = FilesystemItemFactory.NewSavefile(filepath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(filepathToCopy, null);

        Exception? exception = Record.Exception(() => file.OverwriteContents(fileToCopy));
        Assert.Null(exception);
    }


    [Fact]
    public void OverwriteContents_WithItself_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Savefile file = FilesystemItemFactory.NewSavefile(@"C:\Root\file.file", null);
        Assert.Throws<InvalidOperationException>(() => file.OverwriteContents(file));
    }


    [Fact]
    public void OverwriteContents_WhenFileDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string filepath = @"C:\Root\file.file";
        string filepathToCopy = @"C:\Root\fileToCopy.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(filepath) == false &&
            x.FileExists(filepathToCopy) == true));

        Savefile file = FilesystemItemFactory.NewSavefile(filepath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(filepathToCopy, null);

        Assert.Throws<FilesystemMismatchException>(() => file.OverwriteContents(fileToCopy));
    }


    [Fact]
    public void OverwriteContents_WhenFileToCopyDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string filepath = @"C:\Root\file.file";
        string filepathToCopy = @"C:\Root\fileToCopy.file";
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.FileExists(filepath) == true &&
            x.FileExists(filepathToCopy) == false));

        Savefile file = FilesystemItemFactory.NewSavefile(filepath, null);
        Savefile fileToCopy = FilesystemItemFactory.NewSavefile(filepathToCopy, null);

        Assert.Throws<FilesystemMismatchException>(() => file.OverwriteContents(fileToCopy));
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
