using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;
using SaveManagerTests.TestHelpers;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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



    
    #region Rename Tests

    [Fact]
    public void Rename_UpdatesParentsChildren()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalName = "Child";
        string originalPath = Path.Join(parentPath, originalName);   
        string newName = "Renamed";
        string renamedPath = Path.Join(parentPath, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(renamedPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];
    
        renameFolder.Rename(newName);

        Assert.Equal(newName, renameFolder.Name);
        Assert.Contains(parent.Children, x => x.Name == newName);
        Assert.DoesNotContain(parent.Children, x => x.Name == originalName);
    }


    [Fact]
    public void Rename_UpdatesDescendantLocations()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalName = "Child";
        string originalPath = Path.Join(parentPath, originalName);   
        string newName = "Renamed";
        string renamedPath = Path.Join(parentPath, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(renamedPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];
        Savefile childFile = FilesystemItemFactory.NewSavefile(Path.Join(renameFolder.Location, "Child.file"), renameFolder);
        Folder childFolder = FilesystemItemFactory.NewFolder(Path.Join(renameFolder.Location, "ChildFolder"), renameFolder);
        renameFolder.Children = [childFile, childFolder];
        Savefile gChildFile = FilesystemItemFactory.NewSavefile(Path.Join(childFolder.Location, "GChild.file"), childFolder);
        Folder gChildFolder = FilesystemItemFactory.NewFolder(Path.Join(childFolder.Location, "GChildFolder"), childFolder);
        childFolder.Children = [gChildFile, gChildFolder];
        
        renameFolder.Rename(newName);

        Assert.Equal(renamedPath, renameFolder.Location);
        Assert.Equal(Path.Join(childFile.Parent!.Location, childFile.Name), childFile.Location);
        Assert.Equal(Path.Join(childFolder.Parent!.Location, childFolder.Name), childFolder.Location);
        Assert.Equal(Path.Join(gChildFile.Parent!.Location, gChildFile.Name), gChildFile.Location);
        Assert.Equal(Path.Join(gChildFolder.Parent!.Location, gChildFolder.Name), gChildFolder.Location);      
    }


    [Fact]
    public void Rename_WithoutParent_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Folder folder = FilesystemItemFactory.NewFolder(Path.Join(_root, "Folder"), null);
        Assert.Throws<InvalidOperationException>(() => folder.Rename("Renamed"));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(@"\\\\")]
    public void Rename_WithInvalidName_ThrowsValidationException(string newName)
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Child");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];

        Assert.Throws<ValidationException>(() => renameFolder.Rename(newName));
    }


    [Fact]
    public void Rename_WithOwnName_ThrowsValidationException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Child");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];

        Assert.Throws<ValidationException>(() => renameFolder.Rename(renameFolder.Name));
    }


    [Fact]
    public void Rename_WithSiblingsName_ThrowsValidationException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Child");
        string siblingPath = Path.Join(parentPath, "Sibling");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        Folder sibling = FilesystemItemFactory.NewFolder(siblingPath, parent);
        parent.Children = [renameFolder, sibling];

        Assert.Throws<ValidationException>(() => renameFolder.Rename(sibling.Name));
    }


    [Fact]
    public void Rename_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalName = "Child";
        string originalPath = Path.Join(parentPath, originalName);   
        string newName = "Renamed";
        string renamedPath = Path.Join(parentPath, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == false &&
            x.DirectoryExists(renamedPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];

        Assert.Throws<FilesystemMismatchException>(() => renameFolder.Rename(newName));
    }


    [Fact]
    public void Rename_WhenRenamePathExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalName = "Child";
        string originalPath = Path.Join(parentPath, originalName);   
        string newName = "Renamed";
        string renamedPath = Path.Join(parentPath, newName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(renamedPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder renameFolder = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [renameFolder];

        Assert.Throws<FilesystemMismatchException>(() => renameFolder.Rename(newName));
    }

    #endregion



    
    #region Delete Tests

    [Fact]
    public void Delete_UpdatesParentsChildren()
    {
        string parentPath = Path.Join(_root, "Parent");
        string deletePath = Path.Join(parentPath, "ToDelete");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(deletePath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder deleteFolder = FilesystemItemFactory.NewFolder(deletePath, parent);
        parent.Children = [deleteFolder];

        deleteFolder.Delete();

        Assert.Empty(parent.Children);
        Assert.DoesNotContain(deleteFolder, parent.Children);
        Assert.DoesNotContain(parent.Children, x => x.Name == deleteFolder.Name);
    }


    [Fact]
    public void Delete_WithoutParent_ThrowsInvalidOperationException()
    {
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());
        Folder folder = FilesystemItemFactory.NewFolder(Path.Join(_root, "Folder"), null);
        Assert.Throws<InvalidOperationException>(folder.Delete);
    }


    [Fact]
    public void Delete_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string deletePath = Path.Join(parentPath, "ToDelete");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(deletePath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder deleteFolder = FilesystemItemFactory.NewFolder(deletePath, parent);
        parent.Children = [deleteFolder];

        Assert.Throws<FilesystemMismatchException>(deleteFolder.Delete);
    }

    #endregion



}
