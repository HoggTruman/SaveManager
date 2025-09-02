using System.Windows;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for InputDialog.xaml
/// </summary>
public partial class InputDialog : Window
{
    public string Input => InputTextBox.Text;
    public string Prompt => PromptLabel.Content.ToString()!;

    public InputDialog(string title, string prompt, string startingInput="", int maxLength=0)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        InputTextBox.Text = startingInput;
        InputTextBox.MaxLength = maxLength;        
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
