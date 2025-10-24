using System.IO;

namespace SaveManager.Helpers;

public static class PathHelpers
{
    /// <summary>
    /// Returns true if the path represented by the string is a descendant of the directory string provided, otherwise false.
    /// </summary>
    /// <param name="location">The full path of the potential descendant.</param>
    /// <param name="directory">The full path of the potential ancestor.</param>
    /// <returns></returns>
    public static bool IsDescendantOf(this string location, string directory)
    {
        if (Path.TrimEndingDirectorySeparator(location) == Path.TrimEndingDirectorySeparator(directory))
        {
            return false;
        }

        if (!directory.EndsWith(Path.DirectorySeparatorChar))
        {
            directory += Path.DirectorySeparatorChar;
        }

        return location.StartsWith(directory);
    }


    /// <summary>
    /// Returns true if the two strings are considered equal in the filesystem.<br/>
    /// For Windows, case is ignored.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool FilesystemEquals(this string first, string second)
    {
        return first.Equals(second, StringComparison.CurrentCultureIgnoreCase);
    }
}
