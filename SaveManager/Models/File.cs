using SaveManager.Exceptions;
using System.IO;

namespace SaveManager.Models;

public class File : IFilesystemItem
{
    public override string ToString() => Name;

    public string Location { get; set; }
    public string Name => Path.GetFileName(Location);
    public bool Exists => System.IO.File.Exists(Location);

    /// <summary>
    /// The parent folder. It is only null for a file representing a game's savefile.
    /// </summary>
    public Folder? Parent { get; set; }



    
    /// <summary>
    /// Instantiates a new File.
    /// </summary>
    /// <param name="location">The absolute path of the file.</param>
    /// <param name="parent">The parent <See cref="Folder"/>.</param>
    public File(string location, Folder? parent)
    {
        Location = location;
        Parent = parent;
    }
    



    /// <summary>
    /// Creates a copy of the file in the provided folder. Returns the copy.
    /// </summary>
    /// <param name="parentOfCopy">The parent of the new copy.</param>
    /// <returns>The copy of the original file.</returns>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public File CopyTo(Folder parentOfCopy)
    {
        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The file you are trying to copy does not exist.");

        if (!parentOfCopy.Exists)
            throw new FilesystemItemNotFoundException(parentOfCopy.Location, "The parent directory does not exist.");

        try
        {
            string copyLocation = Path.Join(parentOfCopy.Location, GenerateFileName(Name, parentOfCopy.Children));
            System.IO.File.Copy(Location, copyLocation);            
            File copiedFile = new(copyLocation, parentOfCopy);
            parentOfCopy.Children.Add(copiedFile);
            parentOfCopy.SortChildren();
            return copiedFile;
        }
        catch(Exception ex)
        {
            if (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException or 
                FileNotFoundException or IOException or NotSupportedException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }


    /// <summary>
    /// Renames the file in the filesystem.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void Rename(string newName)
    {
        if (Parent == null)
            throw new InvalidOperationException("A file without a parent should not be renamed.");

        ValidateFileName(newName, Parent.Children);
        
        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The file you are trying to rename does not exist.");

        try
        {
            string newLocation = Path.Join(Parent.Location, newName);
            System.IO.File.Move(Location, newLocation);
            Location = newLocation;
            Parent.SortChildren();
        }
        catch(Exception ex)
        {
            if (ex is IOException or FileNotFoundException or ArgumentException or UnauthorizedAccessException or 
                PathTooLongException or DirectoryNotFoundException or NotSupportedException)
                throw new FilesystemException(ex.Message, ex);
                
            throw;
        }
    }


    /// <summary>
    /// Deletes the file in the filesystem and updates its parent.
    /// </summary>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void Delete()
    {
        if (Parent == null)
            throw new InvalidOperationException("A file without a parent should not be deleted.");

        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The file you are trying to delete does not exist.");

        try
        {
            System.IO.File.Delete(Location);
            Parent.Children = [..Parent.Children.Where(x => x != this)];
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException or DirectoryNotFoundException or IOException or NotSupportedException or PathTooLongException 
                or UnauthorizedAccessException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }


    /// <summary>
    /// Moves the underlying file into the folder provided.
    /// </summary>
    /// <param name="newParent"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
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
            throw new FilesystemItemNotFoundException(Location, "The file you are trying to move does not exist.");
        
        if (!newParent.Exists)
            throw new FilesystemItemNotFoundException(newParent.Location, "The destination folder does not exist.");

        try
        {
            string newLocation = Path.Join(newParent.Location, Name);
            System.IO.File.Move(Location, newLocation);
            Parent.Children = [..Parent.Children.Where(x => x != this)];
            Location = newLocation;
            newParent.Children.Add(this);
            newParent.SortChildren();
            Parent = newParent;
        }
        catch (Exception ex)
        {
            if (ex is IOException or FileNotFoundException or ArgumentException or UnauthorizedAccessException or 
                PathTooLongException or DirectoryNotFoundException or NotSupportedException)
            {
                throw new FilesystemException(ex.Message, ex);
            }

            throw;
        }
    }


    /// <summary>
    /// Overwrites the contents of the file with those from the file provided.
    /// </summary>
    /// <param name="file">The file to copy the contents of.</param>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void OverwriteContents(File fileToCopy)
    {
        if (!Exists)
            throw new FilesystemItemNotFoundException(Location, "The file you are trying to overwrite does not exist.");

        if (!fileToCopy.Exists)
            throw new FilesystemItemNotFoundException(fileToCopy.Location, "The file you provided to copy the contents of does not exist.");

        try
        {
            System.IO.File.Copy(fileToCopy.Location, Location, true);      
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException or 
                FileNotFoundException or IOException or NotSupportedException)
                throw new FilesystemException(ex.Message, ex);

            throw;
        }
    }




    /// <summary>
    /// Validates file name. Throws a <see cref="ValidationException"/> if not valid.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ValidationException"></exception>
    internal static void ValidateFileName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("File name can not be empty or whitespace.");

        if (Path.GetInvalidFileNameChars().Any(name.Contains))
            throw new ValidationException($"File name can not contain any of the following: {new(Path.GetInvalidFileNameChars())}");

        if (siblings.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            throw new ValidationException("A file already exists with this name.");
    }


    /// <summary>
    /// Generates a name for a file, appending a suffix when the name is taken by a sibling.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="siblings"></param>
    /// <returns>A unique filename among its siblings.</returns>
    internal static string GenerateFileName(string name, IEnumerable<IFilesystemItem> siblings)
    {
        long suffix = 1;
        string generatedName = name;
        while (siblings.Any(x => x.Name.Equals(generatedName, StringComparison.CurrentCultureIgnoreCase)))
        {
            generatedName = $"{name}_{suffix++}";
        }

        return generatedName;
    }
}
