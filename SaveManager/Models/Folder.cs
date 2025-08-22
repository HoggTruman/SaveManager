using System.IO;

namespace SaveManager.Models;

public class Folder : IFilesystemItem
{
    private readonly DirectoryInfo _directoryInfo;

    public string Location => _directoryInfo.FullName;
    public string Name => _directoryInfo.Name;
    public List<IFilesystemItem> Children { get; set; } = [];

    public Folder(string location)
    {
        _directoryInfo = new(location);
    }

    public Folder(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo;
    }

    /// <summary>
    /// Loads files and directories in the filesystem folder and sets Children.
    /// </summary>
    /// <exception cref="IOException"/>
    public void LoadChildren()
    {
        foreach (DirectoryInfo childDirectoryInfo in _directoryInfo.GetDirectories())
        {
            Folder childFolder = new(childDirectoryInfo);
            childFolder.LoadChildren();
            Children.Add(childFolder);
        }

        foreach (FileInfo childFileInfo in _directoryInfo.GetFiles())
        {
            Children.Add(new File(childFileInfo));
        }
    }
}
