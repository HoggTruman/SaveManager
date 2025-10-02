using System.Windows;
using System.Windows.Media;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for YesNoDialog.xaml
/// </summary>
public partial class YesNoDialog : Window
{
    public string Prompt => PromptLabel.Content.ToString()!;

    public YesNoDialog(string title, string prompt, ImageSource? imageSource=null)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        if (imageSource != null)
        {
            PromptImage.Source = imageSource;
        }
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;             
    }
}
