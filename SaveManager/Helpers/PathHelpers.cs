using System.IO;

namespace SaveManager.Helpers;

public static class PathHelpers
{
    /// <summary>
    /// Returns true if neither path is the parent of the other, otherwise false.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool AreParentChildDirectories(string first, string second)
    {
        if (first == second)
            return false;

        Uri firstUri = new(first.EndsWith(Path.DirectorySeparatorChar) ? first : first + Path.DirectorySeparatorChar);
        Uri secondUri = new(second.EndsWith(Path.DirectorySeparatorChar) ? second : second + Path.DirectorySeparatorChar);
        return firstUri.IsBaseOf(secondUri) || secondUri.IsBaseOf(firstUri);
    }
}
