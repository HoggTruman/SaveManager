using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;
using SaveManagerTests.TestHelpers;
using System.Reflection;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
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
        FilesystemItemFactory.SetDependencies(new FilesystemService());
    }


    private void Setup(string testCaseDirectory)
    {        
        if (Directory.Exists(testCaseDirectory))
            Directory.Delete(testCaseDirectory, true);

        Directory.CreateDirectory(testCaseDirectory);
        TestFolder.Create(testCaseDirectory);
    }




    #region Create Tests

    [Fact]
    public void CreateChildFolder_UpdatesChildren()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder newFolder = parent.CreateChildFolder("New Folder");
        Assert.Contains(newFolder, parent.Children);
    }


    [Fact]
    public void Create_WhenParentDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Directory.Delete(parent.Location, true);
        Assert.Throws<FilesystemMismatchException>(() => parent.CreateChildFolder("New Folder"));
    }


    [Fact]
    public void Create_WhenAlreadyExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        string newFolderName = "New Folder";
        string newFolderLocation = Path.Join(parent.Location, newFolderName);
        Directory.CreateDirectory(newFolderLocation);

        Assert.Throws<FilesystemMismatchException>(() => parent.CreateChildFolder(newFolderName));
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
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder renameFolder = parent.Children.OfType<Folder>().First();
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
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder renameFolder = parent.Children.OfType<Folder>().First();      
        renameFolder.Rename("Renamed Folder");

        IEnumerable<IFilesystemItem> children = renameFolder.Children;
        while (children.Any())
        {
            foreach (IFilesystemItem child in children)
            {
                Assert.Equal(Path.Join(child.Parent!.Location, child.Name), child.Location);
            }
            
            children = children.OfType<Folder>().SelectMany(x => x.Children);
        }        
    }


    [Fact]
    public void Rename_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder renameFolder = parent.Children.OfType<Folder>().First();      
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
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder renameFolder = parent.Children.OfType<Folder>().First();
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
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder deleteFolder = parent.Children.OfType<Folder>().First();
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
        Folder parent = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        parent.LoadChildren();
        Folder folderToDelete = parent.Children.OfType<Folder>().First();      
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
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().Last();
        Folder destination = baseFolder.Children.OfType<Folder>().First();
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

            movedDescendants = [..movedDescendants.OfType<Folder>().SelectMany(x => x.Children)];
        }
    }


    [Fact]
    public void Move_ToItself_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First();
        Assert.Throws<ArgumentException>(() => movingFolder.Move(movingFolder));
    }


    [Fact]
    public void Move_ToChild_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First(x => x.Children.Count > 0);
        Folder childFolder = movingFolder.Children.OfType<Folder>().First();
        Assert.Throws<ArgumentException>(() => movingFolder.Move(childFolder));
    }


    [Fact]
    public void Move_ToDescendant_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First(x => x.Children.Count > 0);
        Folder childFolder = movingFolder.Children.OfType<Folder>().First();
        Folder descendantFolder = childFolder.Children.OfType<Folder>().First();
        Assert.Throws<ArgumentException>(() => movingFolder.Move(descendantFolder));
    }


    [Fact]
    public void Move_ToItsParent_ThrowsArgumentException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First();
        Assert.Throws<ArgumentException>(() => movingFolder.Move(movingFolder.Parent!));
    }


    [Fact]
    public void Move_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        // setup
        string testCaseDirectory = Path.Join(_filesystemFixture.TestDirectory, MethodBase.GetCurrentMethod()!.Name);
        Setup(testCaseDirectory);

        // test
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First();
        Folder destination = baseFolder.Children.OfType<Folder>().Last();
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
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First();
        Folder destination = baseFolder.Children.OfType<Folder>().Last();
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
        Folder baseFolder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        baseFolder.LoadChildren();
        Folder movingFolder = baseFolder.Children.OfType<Folder>().First();
        Folder destination = baseFolder.Children.OfType<Folder>().Last();
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
        Folder folder = FilesystemItemFactory.NewFolder(Path.Join(testCaseDirectory, TestFolder.Name), null);
        folder.LoadChildren();
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
            
            children = children.OfType<Folder>().SelectMany(x => x.Children);
        }  
    }

    #endregion
}
