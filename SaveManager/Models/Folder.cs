using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.Services.FilesystemService;
using System.Collections.ObjectModel;
using System.IO;

namespace SaveManager.Models;

public class Folder : IFilesystemItem
{
    private readonly IFilesystemService _filesystemService;
    private string _location;        


    /// <summary>
    /// The full filesystem path of the underlying file / directory.<br/>
    /// Setting a new value updates the location of the Folder and its children in the internal representation but not the filesystem.
    /// </summary>
    public string Location
    { 
        get => _location; 
        set
        {
            _location = value;
        
            foreach (IFilesystemItem child in Children)
            {
                child.Location = Path.Join(_location, child.Name);
            }
        } 
    }

    public string Name => Path.GetFileName(Location);
    public bool Exists => _filesystemService.DirectoryExists(Location);
    public ObservableCollection<IFilesystemItem> Children { get; set; } = [];

    /// <summary>
    /// The parent folder. It is only null for a folder representing a game's profiles directory.
    /// </summary>
    public Folder? Parent { get; set; }

    public bool IsOpen { get; set; } = false;




    /// <summary>
    /// Instantiates a new Folder and automatically loads children from the filesystem.
    /// </summary>
    /// <param name="location">The absolute path of the folder.</param>
    /// <param name="parent">The parent <see cref="Folder"/>.</param>
    /// <exception cref="FilesystemException"></exception>
    public Folder(string location, Folder? parent, IFilesystemService filesystemService)
    {
        _location = location;
        Parent = parent;
        _filesystemService = filesystemService;
        LoadChildren();        
    }




    /// <summary>
    /// Creates a new child directory in the filesystem and returns a <see cref="Folder"/> that represents it.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public Folder CreateChildFolder(string name)
    {
        ValidateFolderName(name, Children);
        string location = Path.Join(Location, name);

        if (!Exists)
            throw new FilesystemMismatchException(Location, "The parent folder does not exist.");

        if (_filesystemService.DirectoryExists(location))
            throw new FilesystemMismatchException(location, "A child folder already exists with the provided name.");

        _filesystemService.CreateDirectory(location);
        Folder newFolder = FilesystemItemFactory.NewFolder(location, this);
        Children.Add(newFolder);
        SortChildren();
        return newFolder;      
    }


    /// <summary>
    /// Renames the Folder and underlying directory.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void Rename(string newName)
    {
        if (Parent == null)
            throw new InvalidOperationException("A Folder representing a game's profiles directory can not be renamed. Create a new instance instead");

        ValidateFolderName(newName, Parent.Children);
        string newLocation = Path.Join(Parent.Location, newName);
        
        if (!Exists)
            throw new FilesystemMismatchException(Location, "The folder you are trying to rename does not exist.");

        if (_filesystemService.DirectoryExists(newLocation))
            throw new FilesystemMismatchException(newLocation, "A folder already exists at the renamed location.");

        _filesystemService.MoveDirectory(Location, newLocation);
        Location = newLocation;
        Parent.SortChildren();
    }


    /// <summary>
    /// Deletes the Folder and underlying directory in the filesystem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void Delete()
    {
        if (Parent == null)
            throw new InvalidOperationException("A Folder representing a game's profiles directory can not be deleted.");

        if (!Exists)
            throw new FilesystemMismatchException(Location, "The folder you are trying to delete does not exist.");

        _filesystemService.DeleteDirectory(Location);
        Parent.Children = [..Parent.Children.Where(x => x != this)];
    }


    /// <summary>
    /// Moves the underlying directory into the folder provided.
    /// </summary>
    /// <param name="newParent"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Move(Folder newParent)
    {
        if (Parent == null)
            throw new InvalidOperationException("A folder without a parent can not be moved.");

        if (newParent == Parent || newParent == this || newParent.Location.IsDescendantOf(Location))
            throw new ArgumentException("Invalid parent provided.");

        if (newParent.Children.Any(x => x.Name == Name))
            throw new ValidationException("The destination already contains a folder with this name.");

        if (!Exists)
            throw new FilesystemMismatchException(Location, "The folder you are trying to move does not exist.");
        
        if (!newParent.Exists)
            throw new FilesystemMismatchException(newParent.Location, "The destination folder does not exist.");

        string newLocation = Path.Join(newParent.Location, Name);

        if (_filesystemService.DirectoryExists(newLocation))
            throw new FilesystemMismatchException(newLocation, "A folder already exists at the new location");

        _filesystemService.MoveDirectory(Location, newLocation);
        Parent.Children = [..Parent.Children.Where(x => x != this)];
        Location = newLocation;            
        newParent.Children.Add(this);
        newParent.SortChildren();
        Parent = newParent;
    }




    /// <summary>
    /// Loads child files and directories from the filesystem and sets Children.
    /// This method is called recursively for the new child folders.
    /// </summary>
    /// <exception cref="FilesystemException"/>
    public void LoadChildren()
    {
        Children = [];
        foreach (string childDirectoryLocation in _filesystemService.GetChildDirectories(Location))
        {
            Children.Add(FilesystemItemFactory.NewFolder(childDirectoryLocation, this));
        }

        foreach (string childFileLocation in _filesystemService.GetFiles(Location))
        {
            Children.Add(FilesystemItemFactory.NewSavefile(childFileLocation, this));
        }

        SortChildren();
    }


    /// <summary>
    /// Sorts the children by file / directory name.
    /// </summary>
    public void SortChildren()
    {
        Children = [..Children.OrderBy(x => x.Name)];
    }


    /// <summary>
    /// Validates folder name. Throws a ValidationException if not valid.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ValidationException"></exception>
    private static void ValidateFolderName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Folder name can not be empty or whitespace.");

        if (Path.GetInvalidFileNameChars().Any(name.Contains))
            throw new ValidationException($"Folder name can not contain any of the following: {new(Path.GetInvalidFileNameChars())}");

        if (siblings.Any(x => x.Name.FilesystemEquals(name)))
            throw new ValidationException("A folder already exists with this name.");
    }


    public override string ToString()
    {
        return Name;
    }
}
