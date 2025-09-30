using System.IO;

namespace SaveManager.Helpers;

public static class PathHelpers
{
    /// <summary>
    /// Returns true if neither path is a descendant of the other, otherwise false.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool IsEitherDirectoryDescendant(string first, string second)
    {
        if (first == second)
            return false;

        Uri firstUri = new(first.EndsWith(Path.DirectorySeparatorChar) ? first : first + Path.DirectorySeparatorChar);
        Uri secondUri = new(second.EndsWith(Path.DirectorySeparatorChar) ? second : second + Path.DirectorySeparatorChar);
        return firstUri.IsBaseOf(secondUri) || secondUri.IsBaseOf(firstUri);
    }


    /// <summary>
    /// Returns true if the file is a descendant of the directory, otherwise false.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static bool IsDescendant(string file, string directory)
    {
        Uri fileUri = new(file);
        Uri directoryUri = new (directory.EndsWith(Path.DirectorySeparatorChar) ? directory : directory + Path.DirectorySeparatorChar);
        return directoryUri.IsBaseOf(fileUri);
    }
}
