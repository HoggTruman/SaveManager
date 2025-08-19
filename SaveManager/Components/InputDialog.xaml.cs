using System.Windows;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for InputDialog.xaml
/// </summary>
public partial class InputDialog : Window
{
    public string Input => InputTextBox.Text;

    public InputDialog(string title, string prompt, string startingInput="")
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        InputTextBox.Text = startingInput;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        InputTextBox.SelectAll();
		InputTextBox.Focus();
    }
}
