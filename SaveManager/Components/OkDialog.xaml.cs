using System.Windows;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for OkDialog.xaml
/// </summary>
public partial class OkDialog : Window
{
    public string Prompt => PromptLabel.Content.ToString()!;

    public OkDialog(string title, string prompt)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;             
    }
}
