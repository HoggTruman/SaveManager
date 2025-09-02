using System.Windows;

namespace SaveManager.Extensions;

public static class WindowExtensions
{
    
    /// <inheritdoc cref="Window.ShowDialog()"></inheritdoc>
    /// <param name="window">The window to show.</param>
    /// <param name="owner">The owner of the window</param>
    public static bool? ShowDialog(this Window window, Window owner)
    {
        window.Owner = owner;
        return window.ShowDialog();
    }
}
