using System.Windows;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for YesNoDialog.xaml
/// </summary>
public partial class YesNoDialog : Window
{
    public string Prompt => PromptLabel.Content.ToString()!;

    public YesNoDialog(string title, string prompt)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;             
    }
}
