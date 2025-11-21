using SaveManager.Exceptions;
using System.IO;

namespace SaveManager.Services.FilesystemService;

/// <summary>
/// A class to handle operations which interact with the filesystem.
/// </summary>
public class FilesystemService : IFilesystemService
{
    public bool FileExists(string location)
    {
        return File.Exists(location);
    }


    public void CopyFile(string sourceLocation, string destinationLocation, bool overwrite=false)
    {
        try
        {
            File.Copy(sourceLocation, destinationLocation, overwrite);
        }
        catch (Exception ex)
        {
            if (ex is UnauthorizedAccessException or ArgumentException or PathTooLongException or DirectoryNotFoundException or 
                FileNotFoundException or IOException or NotSupportedException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                

            throw;
        }
    }


    public void MoveFile(string sourceLocation, string destinationLocation)
    {
        try
        {
            File.Move(sourceLocation, destinationLocation);
        }
        catch(Exception ex)
        {
            if (ex is IOException or FileNotFoundException or ArgumentException or UnauthorizedAccessException or 
                PathTooLongException or DirectoryNotFoundException or NotSupportedException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                
                
            throw;
        }
    }


    public void DeleteFile(string location)
    {
        try
        {
            File.Delete(location);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException or DirectoryNotFoundException or IOException or NotSupportedException or PathTooLongException 
                or UnauthorizedAccessException)
            {
                throw new FilesystemException(ex.Message, ex);
            }                

            throw;
        }
    }


    public bool DirectoryExists(string location)
    {
        return Directory.Exists(location);
    }


    public void CreateDirectory(string location)
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


    public void MoveDirectory(string sourceLocation, string destinationLocation)
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


    public void DeleteDirectory(string location)
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
