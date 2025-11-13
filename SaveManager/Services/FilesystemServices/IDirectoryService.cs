using SaveManager.Exceptions;
using System.IO;

namespace SaveManager.Services.FilesystemServices;

public interface IDirectoryService
{
    /// <inheritdoc cref="Directory.Exists(string?)"/>
    public bool Exists(string location);

    /// <summary>
    /// Creates all directories and subdirectories of the specified location unless they already exist.
    /// </summary>
    /// <param name="location">The full path to create a directory at.</param>
    /// <exception cref="FilesystemException"></exception>
    public void Create(string location);

    /// <summary>
    /// Moves a directory and its contents to a new location, providing the option to rename it.
    /// </summary>
    /// <param name="sourceLocation">The full path of the directory to be moved.</param>
    /// <param name="destinationLocation">The full path to move the directory to.</param>
    public void Move(string sourceLocation, string destinationLocation);

    /// <summary>
    /// Deletes the specified directory and all its contents.
    /// </summary>
    /// <param name="location">The full path of the directory to be deleted.</param>
    public void Delete(string location);

    /// <summary>
    /// Gets the full path of each child directory in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The full path of the directory.</param>
    /// <returns>An array containing the full path of each child directory.</returns>
    /// <exception cref="FilesystemException"></exception>
    public string[] GetChildDirectories(string directoryPath);

    /// <summary>
    /// Gets the full path of each file in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The full path of the directory.</param>
    /// <returns>An array containing the full path of each file.</returns>
    /// <exception cref="FilesystemException"></exception>
    public string[] GetFiles(string directoryPath);
}
