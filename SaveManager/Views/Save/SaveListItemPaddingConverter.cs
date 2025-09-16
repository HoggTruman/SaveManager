using SaveManager.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SaveManager.Views.Save;

[ValueConversion(typeof(IFilesystemItem), typeof(Thickness))]
public class SaveListItemPaddingConverter : IValueConverter
{
    /// <summary>
    /// Represents the 16 from folder icon + the 5 left padding on the text
    /// </summary>
    private const double LeftPad = 21;

    /// <summary>
    /// A ProfilesDirectory folder has -2 depth, A Profile folder has -1 depth and children of a Profile have 0.<br/>
    /// This means the root level items in the SaveList receive no extra padding.
    /// </summary>
    private const int BaseDepth = -2;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        IFilesystemItem item = (IFilesystemItem)value;
        double depth = BaseDepth;
        while (item.Parent != null)
        {
            ++depth;
            item = item.Parent;
        }

        return new Thickness(depth * LeftPad, 0 , 0 , 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
