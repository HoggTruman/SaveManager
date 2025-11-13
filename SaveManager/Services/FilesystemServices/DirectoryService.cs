using SaveManager.Exceptions;
using System.IO;

namespace SaveManager.Services.FilesystemServices;

/// <summary>
/// A class to handle operations which interact with directories in the filesystem.
/// </summary>
public class DirectoryService : IDirectoryService
{
    public bool Exists(string location)
    {
        return Directory.Exists(location);
    }


    public void Create(string location)
    {
        try
        {
            Directory.CreateDirectory(location);
        }
        catch (Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException 
                or NotSupportedException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                

            throw;
        }
    }


    public void Move(string sourceLocation, string destinationLocation)
    {
        try
        {
            Directory.Move(sourceLocation, destinationLocation);
        }
        catch(Exception ex)
        {
            if (ex is IOException or UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                
                
            throw;
        }
    }


    public void Delete(string location)
    {
        try
        {
            Directory.Delete(location, true);
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


    public string[] GetChildDirectories(string directoryPath)
    {
        try
        {
            return Directory.GetDirectories(directoryPath);
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or IOException or DirectoryNotFoundException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                

            throw;
        }
    }


    public string[] GetFiles(string directoryPath)
    {
        try
        {
            return Directory.GetFiles(directoryPath);
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
}
