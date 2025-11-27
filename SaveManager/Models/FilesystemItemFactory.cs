using SaveManager.Services.FilesystemService;
using SaveManager.Exceptions;

namespace SaveManager.Models;

public static class FilesystemItemFactory
{
    private static IFilesystemService? _filesystemService;

    /// <summary>
    /// Initialize the FilesystemItemFactory with the dependencies needed to create Folders and Savefiles
    /// </summary>
    /// <param name="filesystemService"></param>
    public static void SetDependencies(IFilesystemService filesystemService)
    {
        _filesystemService = filesystemService;
    }


    /// <summary>
    /// Initializes a new Savefile instance with the required dependencies.
    /// </summary>
    /// <param name="location">The full path of the new savefile.</param>
    /// <param name="parent">The parent folder of the new savefile.</param>
    /// <returns>A new Savefile instance.</returns>
    /// <exception cref="InvalidOperationException">The FilesystemItemFactory has not been initialized.</exception>
    public static Savefile NewSavefile(string location, Folder? parent)
    {
        if (_filesystemService == null)
        {
            throw new InvalidOperationException("FilesystemItemFactory has not been initialized.");
        }

        return new Savefile(location, parent, _filesystemService);
    }


    /// <summary>
    /// Initializes a new Folder instance with the required dependencies.
    /// </summary>
    /// <param name="location">The full path of the new folder.</param>
    /// <param name="parent">The parent folder of the new folder.</param>
    /// <returns>A new Folder instance.</returns>
    /// <exception cref="InvalidOperationException">The FilesystemItemFactory has not been initialized.</exception>
    public static Folder NewFolder(string location, Folder? parent)
    {
        if (_filesystemService == null)
        {
            throw new InvalidOperationException("FilesystemItemFactory has not been initialized.");
        }

        return new Folder(location, parent, _filesystemService);
    }
}
