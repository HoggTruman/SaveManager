using SaveManager.Models;

namespace SaveManagerTests.TestHelpers;

public class TestFolder
{
    public required string Name { get; set; }
    public TestFolder[] Folders { get; set; } = [];
    public string[] Files { get; set; } = [];

    /// <summary>
    /// Creates the test folder in the filesystem in the provided parent directory.
    /// </summary>
    /// <param name="parentDirectory"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public void Create(string parentDirectory)
    {
        if (!Directory.Exists(parentDirectory))
            throw new DirectoryNotFoundException("Parent Directory does not exist.");

        string folderPath = Path.Join(parentDirectory, Name);
        Directory.CreateDirectory(folderPath);

        foreach (string fileName in Files)
        {
            string filePath = Path.Join(folderPath, fileName);
            using FileStream stream = System.IO.File.Create(filePath);
        }

        foreach (TestFolder childFolder in Folders)
        {
            childFolder.Create(folderPath);
        }        
    }


    public bool IsEquivalentTo(Folder? folder)
    {
        return folder != null
            && Name == folder.Name
            && Folders.Length + Files.Length == folder.Children.Count
            && Files.All(fileName => folder.Children.Any(y => y is SaveManager.Models.File && y.Name == fileName))
            && Folders.All(x => x.IsEquivalentTo((Folder?)folder.Children.FirstOrDefault(y => y is Folder && y.Name == x.Name)));            
    }
}
