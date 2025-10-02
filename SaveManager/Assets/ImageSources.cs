using System.Windows.Media.Imaging;

namespace SaveManager.Assets;

public static class ImageSources
{
    public static BitmapImage Error { get; } = new(new Uri(@"/SaveManager;component/Assets/Error.png", UriKind.Relative));
    public static BitmapImage Question { get; } = new(new Uri(@"/SaveManager;component/Assets/Question.png", UriKind.Relative));
    public static BitmapImage Success { get; } = new(new Uri(@"/SaveManager;component/Assets/Success.png", UriKind.Relative));
    public static BitmapImage Warning { get; } = new(new Uri(@"/SaveManager;component/Assets/Error.png", UriKind.Relative));
}
