using SaveManager.Exceptions;
using System.Collections.ObjectModel;
using System.IO;

namespace SaveManager.Models;

public class Folder : IFilesystemItem
{
    private string _location;

    public override string ToString() => Name;

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
    public bool Exists => Directory.Exists(Location);
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
    public Folder(string location, Folder? parent)
    {
        _location = location;        
        Parent = parent;
        LoadChildren();
    }




    /// <summary>
    /// Creates a new directory in the filesystem and returns a <see cref="Folder"/> that represents it.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public static Folder Create(string name, Folder parent)
    {
        ValidateFolderName(name, parent.Children);

        if (!parent.Exists)
            throw new FilesystemItemNotFoundException(parent.Location, "The parent folder does not exist.");

        try
        {
            string location = Path.Join(parent.Location, name);
            Directory.CreateDirectory(location);
            Folder newFolder = new(location, parent);
            parent.Children.Add(newFolder);
            parent.SortChildren();
            return newFolder;
        }
        catch (Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException or NotSupportedException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }        
    }


    /// <summary>
    /// Renames the Folder and underlying directory.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Rename(string newName)
    {
        if (Parent == null)
            throw new InvalidOperationException("A Folder representing a game's profiles directory should not be renamed. Create a new instance instead");

        ValidateFolderName(newName, Parent.Children);
        
        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The folder you are trying to rename does not exist.");

        try
        {
            string newLocation = Path.Join(Parent.Location, newName);
            Directory.Move(Location, newLocation);
            Location = newLocation;
            Parent.SortChildren();
        }
        catch(Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException)
                throw new FilesystemException(ex.Message, ex);
                
            throw;
        }
    }


    /// <summary>
    /// Deletes the Folder and underlying directory in the filesystem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void Delete()
    {
        if (Parent == null)
            throw new InvalidOperationException("A Folder representing a game's profiles directory should not be deleted.");

        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The folder you are trying to delete does not exist.");

        try
        {
            Directory.Delete(Location, true);
            Parent.Children = [..Parent.Children.Where(x => x != this)];
        }
        catch (Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }


    /// <summary>
    /// Moves the underlying directory into the folder provided.
    /// </summary>
    /// <param name="newParent"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Move(Folder newParent)
    {
        if (Parent == null)
            throw new InvalidOperationException("A folder without a parent can not be moved.");

        if (newParent == Parent || newParent == this)
            throw new ArgumentException("Invalid parent provided.");

        if (newParent.Children.Any(x => x.Name == Name))
            throw new ValidationException("The destination already contains a folder with this name.");

        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The folder you are trying to move does not exist.");
        
        if (!newParent.Exists)
            throw new FilesystemItemNotFoundException(newParent.Location, "The destination folder does not exist.");

        try
        {
            string newLocation = Path.Join(newParent.Location, Name);
            Directory.Move(Location, newLocation);
            Parent.Children = [..Parent.Children.Where(x => x != this)];
            Location = newLocation;            
            newParent.Children.Add(this);
            newParent.SortChildren();
            Parent = newParent;
        }
        catch (Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException)
            {
                throw new FilesystemException(ex.Message, ex);
            }

            throw;
        }
    }




    /// <summary>
    /// Loads child files and directories from the filesystem and sets Children.
    /// </summary>
    /// <exception cref="FilesystemException"/>
    public void LoadChildren()
    {
        try
        {
            Children = [];
            foreach (string childDirectoryLocation in Directory.GetDirectories(Location))
            {
                Children.Add(new Folder(childDirectoryLocation, this));
            }

            foreach (string childFileLocation in Directory.GetFiles(Location))
            {
                Children.Add(new File(childFileLocation, this));
            }

            SortChildren();
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or IOException or DirectoryNotFoundException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
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
}
