using System.IO;

namespace SaveManager.Models;

public class File : IFilesystemItem
{
    private readonly FileInfo _fileInfo;

    public string Location => _fileInfo.FullName;
    public string Name => _fileInfo.Name;

    public File(string location)
    {
        _fileInfo = new(location);
    }

    public File(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }
}
