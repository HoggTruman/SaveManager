using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.Services.FilesystemService;
using System.IO;

namespace SaveManager.Models;

public class Savefile : IFilesystemItem
{
    private readonly IFilesystemService _filesystemService;

    public string Location { get; set; }
    public string Name => Path.GetFileName(Location);
    public bool Exists => _filesystemService.FileExists(Location);

    /// <summary>
    /// The parent folder. It is only null for a game's current savefile.
    /// </summary>
    public Folder? Parent { get; set; }



    
    /// <summary>
    /// Instantiates a new Savefile.
    /// </summary>
    /// <param name="location">The absolute path of the savefile.</param>
    /// <param name="parent">The parent <See cref="Folder"/>.</param>
    public Savefile(string location, Folder? parent, IFilesystemService filesystemService)
    {
        Location = location;
        Parent = parent;
        _filesystemService = filesystemService;
    }
    



    /// <summary>
    /// Creates a copy of the savefile in the provided folder. Returns the copy.
    /// </summary>
    /// <param name="destinationFolder">The parent of the new copy.</param>
    /// <returns>The copy of the original file.</returns>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public Savefile CopyTo(Folder destinationFolder)
    {
        if (!Exists)
            throw new FilesystemMismatchException(Location, "The file you are trying to copy does not exist.");

        if (!destinationFolder.Exists)
            throw new FilesystemMismatchException(destinationFolder.Location, "The parent directory does not exist.");

        string copyLocation = Path.Join(destinationFolder.Location, GenerateFileName(Name, destinationFolder.Children));

        if (_filesystemService.FileExists(copyLocation))
            throw new FilesystemMismatchException(copyLocation, "A file already exists at the copy location");

        _filesystemService.CopyFile(Location, copyLocation);            
        Savefile copiedFile = FilesystemItemFactory.NewSavefile(copyLocation, destinationFolder);
        destinationFolder.Children.Add(copiedFile);
        destinationFolder.SortChildren();
        return copiedFile;
    }


    /// <summary>
    /// Renames the savefile in the filesystem.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void Rename(string newName)
    {
        if (Parent == null)
            throw new InvalidOperationException("A file without a parent should not be renamed.");

        ValidateFileName(newName, Parent.Children);
        string newLocation = Path.Join(Parent.Location, newName);
        
        if (!Exists)
            throw new FilesystemMismatchException(Location, "The file you are trying to rename does not exist.");

        if (_filesystemService.FileExists(newLocation))
            throw new FilesystemMismatchException(newLocation, "A file already exists at the renamed location");

        _filesystemService.MoveFile(Location, newLocation);
        Location = newLocation;
        Parent.SortChildren();
    }


    /// <summary>
    /// Deletes the savefile in the filesystem and updates its parent.
    /// </summary>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Delete()
    {
        if (Parent == null)
            throw new InvalidOperationException("A file without a parent should not be deleted.");

        if (!Exists)
            throw new FilesystemMismatchException(Location, "The file you are trying to delete does not exist.");

        _filesystemService.DeleteFile(Location);
        Parent.Children = [..Parent.Children.Where(x => x != this)];
    }


    /// <summary>
    /// Moves the savefile into the folder provided in the filesystem.
    /// </summary>
    /// <param name="newParent"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Move(Folder newParent)
    {
        if (Parent == null)
            throw new InvalidOperationException("A file without a parent can not be moved.");

        if (newParent == Parent)
            throw new InvalidOperationException("The file is already a child of the new parent.");

        if (newParent.Children.Any(x => x.Name == Name))
            throw new ValidationException("The destination already contains a file with this name.");

        if (!Exists)
            throw new FilesystemMismatchException(Location, "The file you are trying to move does not exist.");
        
        if (!newParent.Exists)
            throw new FilesystemMismatchException(newParent.Location, "The destination folder does not exist.");

        string newLocation = Path.Join(newParent.Location, Name);

        if (_filesystemService.FileExists(newLocation))
            throw new FilesystemMismatchException(newLocation, "A file already exists at the new location");

        _filesystemService.MoveFile(Location, newLocation);
        Parent.Children = [..Parent.Children.Where(x => x != this)];
        Location = newLocation;
        newParent.Children.Add(this);
        newParent.SortChildren();
        Parent = newParent;
    }


    /// <summary>
    /// Overwrites the contents of the savefile with those from the savefile provided.
    /// </summary>
    /// <param name="fileToCopy">The savefile to copy the contents of.</param>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void OverwriteContents(Savefile fileToCopy)
    {
        if (!Exists)
            throw new FilesystemMismatchException(Location, "The file you are trying to overwrite does not exist.");

        if (!fileToCopy.Exists)
            throw new FilesystemMismatchException(fileToCopy.Location, "The file you provided to copy the contents of does not exist.");

        _filesystemService.CopyFile(fileToCopy.Location, Location, true);
    }




    /// <summary>
    /// Validates file name. Throws a <see cref="ValidationException"/> if not valid.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ValidationException"></exception>
    private static void ValidateFileName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("File name can not be empty or whitespace.");

        if (Path.GetInvalidFileNameChars().Any(name.Contains))
            throw new ValidationException($"File name can not contain any of the following: {new(Path.GetInvalidFileNameChars())}");

        if (siblings.Any(x => x.Name.FilesystemEquals(name)))
            throw new ValidationException("A file already exists with this name.");
    }


    /// <summary>
    /// Generates a name for a savefile, appending a suffix when the name is taken by a sibling.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="siblings"></param>
    /// <returns>A unique filename among its siblings.</returns>
    public static string GenerateFileName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        long suffix = 1;
        string generatedName = name;
        while (siblings.Any(x => x.Name.FilesystemEquals(generatedName)))
        {
            generatedName = $"{name}_{suffix++}";
        }

        return generatedName;
    }


    public override string ToString()
    {
        return Name;
    }
}
