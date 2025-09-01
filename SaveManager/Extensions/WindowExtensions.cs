using System.Windows;

namespace SaveManager.Extensions;

public static class WindowExtensions
{
    /// <summary>
    /// Opens a window and returns only when the newly opened window is closed.
    /// </summary>
    /// <param name="window">The window to show.</param>
    /// <param name="owner">The owner of the window</param>
    /// <returns>A <see cref="Nullable{T}"/> of type <see cref="Boolean"/> that specifies whether the activity was accepted (true) or canceled (false).</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool? ShowDialog(this Window window, Window owner)
    {
        window.Owner = owner;
        return window.ShowDialog();
    }
}
