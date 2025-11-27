using Moq;
using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services.FilesystemService;

namespace SaveManagerTests.Models;

[Collection("Sequential")]
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



    
    #region Move Tests

    [Fact]
    public void Move_UpdatesParentAndDestination()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string destinationFolderPath = Path.Join(_root, "Destination");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        moving.Move(destination);

        Assert.Empty(parent.Children);
        Assert.Contains(moving, destination.Children);
        Assert.Equal(destination, moving.Parent);     
    }


    [Fact]
    public void Move_UpdatesDescendantLocations()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string destinationFolderPath = Path.Join(_root, "Destination");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Savefile childFile = FilesystemItemFactory.NewSavefile(Path.Join(moving.Location, "Child.file"), moving);
        Folder childFolder = FilesystemItemFactory.NewFolder(Path.Join(moving.Location, "ChildFolder"), moving);
        moving.Children = [childFile, childFolder];
        Savefile gChildFile = FilesystemItemFactory.NewSavefile(Path.Join(childFolder.Location, "GChild.file"), childFolder);
        Folder gChildFolder = FilesystemItemFactory.NewFolder(Path.Join(childFolder.Location, "GChildFolder"), childFolder);
        childFolder.Children = [gChildFile, gChildFolder];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        moving.Move(destination);

        Assert.Equal(Path.Join(destination.Location, moving.Name), moving.Location);
        Assert.Equal(Path.Join(moving.Location, childFile.Name), childFile.Location);
        Assert.Equal(Path.Join(moving.Location, childFolder.Name), childFolder.Location);
        Assert.Equal(Path.Join(childFolder.Location, gChildFile.Name), gChildFile.Location);
        Assert.Equal(Path.Join(childFolder.Location, gChildFolder.Name), gChildFolder.Location);
    }


    [Fact]
    public void Move_WithNullParent_ThrowsInvalidOperationException()
    {
        string originalPath = Path.Join(_root, "Moving");
        string destinationFolderPath = Path.Join(_root, "Destination");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true));

        Folder moving = FilesystemItemFactory.NewFolder(originalPath, null);
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<InvalidOperationException>(() => moving.Move(destination));
    }


    [Fact]
    public void Move_ToItself_ThrowsArgumentException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];

        Assert.Throws<ArgumentException>(() => moving.Move(moving));
    }


    [Fact]
    public void Move_ToChild_ThrowsArgumentException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string childPath = Path.Join(originalPath, "Child");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(childPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder child = FilesystemItemFactory.NewFolder(childPath, moving);
        moving.Children = [child];

        Assert.Throws<ArgumentException>(() => moving.Move(child));
    }


    [Fact]
    public void Move_ToDescendant_ThrowsArgumentException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string childPath = Path.Join(originalPath, "Child");
        string gChildPath = Path.Join(originalPath, "Grandchild");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(childPath) == true &&
            x.DirectoryExists(gChildPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder child = FilesystemItemFactory.NewFolder(childPath, moving);
        moving.Children = [child];
        Folder gChild = FilesystemItemFactory.NewFolder(gChildPath, child);

        Assert.Throws<ArgumentException>(() => moving.Move(gChild));
    }


    [Fact]
    public void Move_ToItsCurrentParent_ThrowsArgumentException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];

        Assert.Throws<ArgumentException>(() => moving.Move(parent));
    }


    [Fact]
    public void Move_WhenDestinationHasFolderWithSameName_ThrowsValidationException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string movingName = "Moving";
        string originalPath = Path.Join(parentPath, movingName);
        string destinationFolderPath = Path.Join(_root, "Destination");
        string targetPath = Path.Join(destinationFolderPath, movingName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.DirectoryExists(targetPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);
        Folder destinationChild = FilesystemItemFactory.NewFolder(targetPath, destination);
        destination.Children = [destinationChild];

        Assert.Throws<ValidationException>(() => moving.Move(destination));
    }


    [Fact]
    public void Move_WhenFolderDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string destinationFolderPath = Path.Join(_root, "Destination");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == false &&
            x.DirectoryExists(destinationFolderPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => moving.Move(destination));
    }


    [Fact]
    public void Move_WhenDestinationDoesNotExist_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string originalPath = Path.Join(parentPath, "Moving");
        string destinationFolderPath = Path.Join(_root, "Destination");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == false));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => moving.Move(destination));
    }


    [Fact]
    public void Move_WhenTargetPathExistsInFilesystemButNotInternally_ThrowsFilesystemMismatchException()
    {
        string parentPath = Path.Join(_root, "Parent");
        string movingName = "Moving";
        string originalPath = Path.Join(parentPath, movingName);
        string destinationFolderPath = Path.Join(_root, "Destination");
        string targetPath = Path.Join(destinationFolderPath, movingName);
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.DirectoryExists(parentPath) == true &&
            x.DirectoryExists(originalPath) == true &&
            x.DirectoryExists(destinationFolderPath) == true &&
            x.DirectoryExists(targetPath) == true));

        Folder parent = FilesystemItemFactory.NewFolder(parentPath, null);
        Folder moving = FilesystemItemFactory.NewFolder(originalPath, parent);
        parent.Children = [moving];
        Folder destination = FilesystemItemFactory.NewFolder(destinationFolderPath, null);

        Assert.Throws<FilesystemMismatchException>(() => moving.Move(destination));
    }

    #endregion




    #region LoadChildren Tests

    [Fact]
    public void LoadChildren_LoadsCompleteTree()
    {
        string folderPath = Path.Join(_root, "Folder");
        string[] childFiles = [
            Path.Join(folderPath, "child1.file"),
            Path.Join(folderPath, "child2.file"),
            Path.Join(folderPath, "child3.file")];
        string childFolderPath = Path.Join(folderPath, "ChildFolder1");
        string[] childFolders = [childFolderPath];
        string[] gChildFiles = [
            Path.Join(childFolderPath, "gchild1.file"),
            Path.Join(childFolderPath, "gchild2.file")];
        string gChildFolderPath = Path.Join(childFolderPath, "gChildFolder1");
        string[] gChildFolders = [
            gChildFolderPath,
            Path.Join(childFolderPath, "gChildFolder2"),
            Path.Join(childFolderPath, "gChildFolder3")];
        string[] ggChildFiles = [
            Path.Join(gChildFolderPath, "ggChild1.file")];
        string[] ggChildFolders = [];

        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>(x =>
            x.GetFiles(folderPath) == childFiles &&
            x.GetChildDirectories(folderPath) == childFolders &&
            x.GetFiles(childFolderPath) == gChildFiles &&
            x.GetChildDirectories(childFolderPath) == gChildFolders &&
            x.GetFiles(gChildFolderPath) == ggChildFiles &&
            x.GetChildDirectories(gChildFolderPath) == ggChildFolders));

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        folder.LoadChildren();

        Assert.Equal(childFiles.Length + childFolders.Length, folder.Children.Count);
        Assert.True(childFiles.All(x => folder.Children.Any(y => y.Location == x)));
        Assert.True(childFolders.All(x => folder.Children.Any(y => y.Location == x)));

        Folder childFolder = folder.Children.OfType<Folder>().First(x => x.Location == childFolderPath);
        Assert.Equal(gChildFiles.Length + gChildFolders.Length, childFolder.Children.Count);
        Assert.True(gChildFiles.All(x => childFolder.Children.Any(y => y.Location == x)));
        Assert.True(gChildFolders.All(x => childFolder.Children.Any(y => y.Location == x)));

        Folder gChildFolder = childFolder.Children.OfType<Folder>().First(x => x.Location == gChildFolderPath);
        Assert.Equal(ggChildFiles.Length + ggChildFolders.Length, gChildFolder.Children.Count);
        Assert.True(ggChildFiles.All(x => gChildFolder.Children.Any(y => y.Location == x)));
        Assert.True(ggChildFolders.All(x => gChildFolder.Children.Any(y => y.Location == x)));
    }

    #endregion



    
    #region Location Tests

    [Fact]
    public void LocationSetter_UpdatesDescendantLocations()
    {
        string folderPath = Path.Join(_root, "Parent");
        FilesystemItemFactory.SetDependencies(Mock.Of<IFilesystemService>());

        Folder folder = FilesystemItemFactory.NewFolder(folderPath, null);
        Folder childFolder = FilesystemItemFactory.NewFolder(Path.Join(folder.Location, "ChildFolder"), folder);
        Savefile childFile = FilesystemItemFactory.NewSavefile(Path.Join(folder.Location, "Child.file"), folder);
        folder.Children = [childFolder, childFile];        
        Folder gChildFolder = FilesystemItemFactory.NewFolder(Path.Join(childFolder.Location, "GChildFolder"), childFolder);
        Savefile gChildFile = FilesystemItemFactory.NewSavefile(Path.Join(childFolder.Location, "GChild.file"), childFolder);
        childFolder.Children = [gChildFile, gChildFolder];

        string newLocation = Path.Join(folder.Location, "New");
        folder.Location = newLocation;

        Assert.Equal(newLocation, folder.Location);
        Assert.Equal(Path.Join(folder.Location, childFolder.Name), childFolder.Location);
        Assert.Equal(Path.Join(folder.Location, childFile.Name), childFile.Location);
        Assert.Equal(Path.Join(childFolder.Location, gChildFolder.Name), gChildFolder.Location);
        Assert.Equal(Path.Join(childFolder.Location, gChildFile.Name), gChildFile.Location);
    }

    #endregion
}
