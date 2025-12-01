namespace SaveManagerTests;

public class FilesystemFixture : IDisposable
{
    public readonly string TestDirectory = Path.Join(Directory.GetCurrentDirectory(), "SaveManagerTests");

    public FilesystemFixture()
    {
        if (Directory.Exists(TestDirectory))
        {
            Directory.Delete(TestDirectory, true);
        }            

        Directory.CreateDirectory(TestDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(TestDirectory))
        {
            Directory.Delete(TestDirectory, true);
        }            
    }
}
