using System.Windows.Input;

namespace SaveManager.Views.Save;

public static class SaveListCommands
{
    public static readonly RoutedUICommand AddFolder = new(
        "Add Folder", 
        "Add Folder", 
        typeof(SaveListCommands));


    public static readonly RoutedUICommand Delete = new(
        "Delete", 
        "Delete", 
        typeof(SaveListCommands), 
        [new KeyGesture(Key.Delete)]);


    public static readonly RoutedUICommand Rename = new(
        "Rename",
        "Rename",
        typeof(SaveListCommands),
        [new KeyGesture(Key.F12)]);

    public static readonly RoutedUICommand Refresh = new(
        "Refresh",
        "Refresh",
        typeof(SaveListCommands));
}
