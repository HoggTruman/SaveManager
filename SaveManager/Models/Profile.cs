using System.IO;

namespace SaveManager.Models;

public class Profile : Folder
{
    public Profile(string location) : base(location) { }
    public Profile(DirectoryInfo directoryInfo) : base(directoryInfo) { }
}
