using SaveManager.Exceptions;
using System.IO;

namespace SaveManager.Services.FilesystemServices;

/// <summary>
/// A class to handle operations which interact with files in the filesystem.
/// </summary>
public class FileService : IFileService
{
    public bool Exists(string location)
    {
        return File.Exists(location);
    }


    public void Copy(string sourceLocation, string destinationLocation, bool overwrite=false)
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


    public void Move(string sourceLocation, string destinationLocation)
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


    public void Delete(string location)
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
}
