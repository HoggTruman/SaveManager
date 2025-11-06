using System.Windows;
using System.Windows.Media;

namespace SaveManager.Components.Dialogs;

/// <summary>
/// Interaction logic for OkDialog.xaml
/// </summary>
public partial class OkDialog : Window
{
    public string Prompt => PromptLabel.Content.ToString()!;

    public OkDialog(string title, string prompt, ImageSource imageSource)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        PromptImage.Source = imageSource;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;             
    }
}
