using SaveManager.Exceptions;
using System.Collections.ObjectModel;
using System.IO;

namespace SaveManager.Models;

public class Folder : IFilesystemItem
{
    private DirectoryInfo _directoryInfo;

    public string Location => _directoryInfo.FullName;
    public string Name => _directoryInfo.Name;
    public bool Exists => _directoryInfo.Exists;
    public ObservableCollection<IFilesystemItem> Children { get; set; } = [];

    /// <summary>
    /// The parent folder. It is only null for a folder representing a game's profiles directory.
    /// </summary>
    public Folder? Parent { get; set; }




    /// <summary>
    /// Instantiates new Folder and automatically loads children from the filesystem.
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <param name="parent"></param>
    /// <exception cref="FilesystemException"></exception>
    public Folder(DirectoryInfo directoryInfo, Folder? parent)
    {
        _directoryInfo = directoryInfo;
        Parent = parent;
        LoadChildren();
    }

    /// <inheritdoc cref="Folder(DirectoryInfo, Folder?)"/>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public Folder(string location, Folder? parent)
    {
        try
        {
            _directoryInfo = new(location);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException or PathTooLongException or System.Security.SecurityException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
        
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
            throw new FilesystemItemNotFoundException("The parent folder does not exist.");

        try
        {
            string location = Path.Join(parent.Location, name);
            DirectoryInfo directoryInfo = Directory.CreateDirectory(location);
            return new(directoryInfo, parent);
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
            throw new FilesystemItemNotFoundException("The folder you are trying to rename does not exist.");

        try
        {
            string newLocation = Path.Join(Parent.Location, newName);
            _directoryInfo.MoveTo(newLocation);

            foreach (IFilesystemItem child in Children)
            {
                string newChildLocation = Path.Join(newLocation, child.Name);
                child.UpdateLocation(newChildLocation);
            }
        }
        catch(Exception ex)
        {
            if (ex is ArgumentException or IOException or System.Security.SecurityException or DirectoryNotFoundException)
                throw new FilesystemException(ex.Message, ex);
                
            throw;
        }       

        Parent.SortChildren();
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
            throw new FilesystemItemNotFoundException("The folder you are trying to delete does not exist.");

        try
        {
            _directoryInfo.Delete(true);
            Parent.Children = [..Parent.Children.Where(x => x != this)];
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or DirectoryNotFoundException or IOException or System.Security.SecurityException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }


    /// <summary>
    /// Updates the Folder's location for the internal filesystem representation.
    /// Does not actually affect any files or directories in the filesytem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    public void UpdateLocation(string newLocation)
    {
        try
        {
            _directoryInfo = new(newLocation);
        }
        catch (Exception ex)
        {
            if (ex is System.Security.SecurityException or ArgumentException or PathTooLongException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
        
        foreach (IFilesystemItem child in Children)
        {
            string newChildLocation = Path.Join(Location, child.Name);
            child.UpdateLocation(newChildLocation);
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
            foreach (DirectoryInfo childDirectoryInfo in _directoryInfo.GetDirectories())
            {
                Folder childFolder = new(childDirectoryInfo, this);
                Children.Add(childFolder);
            }

            foreach (FileInfo childFileInfo in _directoryInfo.GetFiles())
            {
                Children.Add(new File(childFileInfo));
            }

            SortChildren();
        }
        catch (Exception ex)
        {
            if (ex is DirectoryNotFoundException or System.Security.SecurityException or UnauthorizedAccessException)
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

        if (siblings.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            throw new ValidationException("A folder already exists with this name.");
    }
}
