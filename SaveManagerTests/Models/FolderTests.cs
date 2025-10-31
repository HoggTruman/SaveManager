using SaveManager.Exceptions;
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
            new() { Name = "Folder2", Folders = [new() { Name = "Folder3", Folders = [new() { Name = "Folder4" }] }], Files = ["file3.file"] }
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




    #region Constructor Tests

    [Fact]
    public void Constructor_LoadsChildren()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Assert.True(TestFolder.IsEquivalentTo(folder));
    }


    [Fact]
    public void Constructor_WhenLocationDoesNotExist_ThrowsFilesystemException()
    {
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Assert.Throws<FilesystemException>(() => new Folder(Path.Join(testCaseDirectory, "FolderThatDoesNotExist"), null));
    }

    #endregion




    #region Create Tests

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
    public void Create_WhenParentDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Directory.Delete(parent.Location, true);
        Assert.Throws<FilesystemMismatchException>(() => Folder.Create("New Folder", parent));
    }


    [Fact]
    public void Create_WhenAlreadyExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        string newFolderName = "New Folder";
        string newFolderLocation = Path.Join(parent.Location, newFolderName);
        Directory.CreateDirectory(newFolderLocation);

        Assert.Throws<FilesystemMismatchException>(() => Folder.Create(newFolderName, parent));
    }

    #endregion




    #region Rename Tests

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
        renameFolder.Rename("Renamed Folder");

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
    public void Rename_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder renameFolder = (Folder)parent.Children.First(x => x is Folder);      
        Directory.Delete(renameFolder.Location, true);

        Assert.Throws<FilesystemMismatchException>(() => renameFolder.Rename("Renamed Folder"));
    }


    [Fact]
    public void Rename_WhenRenameLocationExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder renameFolder = (Folder)parent.Children.First(x => x is Folder);
        string newName = "Renamed Folder";
        string newLocation = Path.Join(parent.Location, newName);
        Directory.CreateDirectory(newLocation);

        Assert.Throws<FilesystemMismatchException>(() => renameFolder.Rename(newName));
    }

    #endregion




    #region Delete Tests

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
    public void Delete_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder folderToDelete = (Folder)parent.Children.First(x => x is Folder);      
        Directory.Delete(folderToDelete.Location, true);

        Assert.Throws<FilesystemMismatchException>(folderToDelete.Delete);
    }

    #endregion




    #region Move Tests

    [Fact]
    public void Move_MovesFolder()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.Last(x => x is Folder);
        Folder destination = (Folder)baseFolder.Children.First(x => x is Folder);
        movingFolder.Move(destination);

        Assert.DoesNotContain(movingFolder, baseFolder.Children);
        Assert.Contains(movingFolder, destination.Children);
        Assert.Equal(destination, movingFolder.Parent);
        Assert.Equal(movingFolder.Parent!.Location, Path.GetDirectoryName(movingFolder.Location));

        // check the descendants have their locations updated
        List<IFilesystemItem> movedDescendants = [..movingFolder.Children];

        while(movedDescendants.Count > 0)
        {
            foreach (IFilesystemItem item in movedDescendants)
            {
                Assert.Equal(item.Parent!.Location, Path.GetDirectoryName(item.Location));
            }

            movedDescendants = [..movedDescendants.SelectMany(x => x is Folder f? f.Children: [])];
        }
    }


    [Fact]
    public void Move_ToItself_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder);
        Assert.Throws<ArgumentException>(() => movingFolder.Move(movingFolder));
    }


    [Fact]
    public void Move_ToChild_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder folder && folder.Children.Count > 0);
        Folder childFolder = (Folder)movingFolder.Children.First(x => x is Folder);
        Assert.Throws<ArgumentException>(() => movingFolder.Move(childFolder));
    }


    [Fact]
    public void Move_ToDescendant_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder folder && folder.Children.Count > 0);
        Folder childFolder = (Folder)movingFolder.Children.First(x => x is Folder);
        Folder descendantFolder = (Folder)childFolder.Children.First(x => x is Folder);
        Assert.Throws<ArgumentException>(() => movingFolder.Move(descendantFolder));
    }


    [Fact]
    public void Move_ToItsParent_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder);
        Assert.Throws<ArgumentException>(() => movingFolder.Move(movingFolder.Parent!));
    }


    [Fact]
    public void Move_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder);
        Folder destination = (Folder)baseFolder.Children.Last(x => x is Folder);
        Directory.Delete(movingFolder.Location);
        Assert.Throws<FilesystemMismatchException>(() => movingFolder.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder);
        Folder destination = (Folder)baseFolder.Children.Last(x => x is Folder);
        Directory.Delete(destination.Location, true);
        Assert.Throws<FilesystemMismatchException>(() => movingFolder.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationFolderExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        Folder movingFolder = (Folder)baseFolder.Children.First(x => x is Folder);
        Folder destination = (Folder)baseFolder.Children.Last(x => x is Folder);
        string newLocation = Path.Join(destination.Location, movingFolder.Name);
        Directory.CreateDirectory(newLocation);

        Assert.Throws<FilesystemMismatchException>(() => movingFolder.Move(destination));
    }

    #endregion




    #region Location Tests

    [Fact]
    public void LocationSetter_UpdatesDescendantLocations()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder folder = new(Path.Join(testCaseDirectory, TestFolder.Name), null);
        string newLocation = Path.Join(folder.Location, "new");
        folder.Location = newLocation;
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

    #endregion
}
