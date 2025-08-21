namespace SaveManager.Models;

public interface IFilesystemItem
{
    public string Location { get; }
    public string Name { get; }
}
