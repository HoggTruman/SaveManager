using System.IO;
using SaveManager.Exceptions;

namespace SaveManager.Services.FilesystemService;

public interface IFilesystemService
{
    /// <inheritdoc cref="File.Exists(string?)"/>
    public bool FileExists(string location);

    /// <summary>
    /// Copies an existing file to another location. 
    /// </summary>
    /// <param name="sourceLocation">The full path of the file to be copied.</param>
    /// <param name="destinationLocation">The full path to copy the file to.</param>
    /// <param name="overwrite">Whether an existing file at the destination should be overwritten.</param>
    /// <exception cref="FilesystemException"></exception>
    public void CopyFile(string sourceLocation, string destinationLocation, bool overwrite=false);

    /// <summary>
    /// Moves a file to a new location, providing the option to rename it.
    /// </summary>
    /// <param name="sourceLocation">The full path of the file to be moved.</param>
    /// <param name="destinationLocation">The full path to move the file to.</param>
    /// <exception cref="FilesystemException"></exception>
    public void MoveFile(string sourceLocation, string destinationLocation);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="location">The full path of the file to be deleted.</param>
    /// <exception cref="FilesystemException"></exception>
    public void DeleteFile(string location);

    /// <inheritdoc cref="Directory.Exists(string?)"/>
    public bool DirectoryExists(string location);

    /// <summary>
    /// Creates all directories and subdirectories of the specified location unless they already exist.
    /// </summary>
    /// <param name="location">The full path to create a directory at.</param>
    /// <exception cref="FilesystemException"></exception>
    public void CreateDirectory(string location);

    /// <summary>
    /// Moves a directory and its contents to a new location, providing the option to rename it.
    /// </summary>
    /// <param name="sourceLocation">The full path of the directory to be moved.</param>
    /// <param name="destinationLocation">The full path to move the directory to.</param>
    /// <exception cref="FilesystemException"></exception>
    public void MoveDirectory(string sourceLocation, string destinationLocation);

    /// <summary>
    /// Deletes the specified directory and all its contents.
    /// </summary>
    /// <param name="location">The full path of the directory to be deleted.</param>
    /// <exception cref="FilesystemException"></exception>
    public void DeleteDirectory(string location);

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
