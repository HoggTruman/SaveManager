using SaveManager.Validators;
using System.Windows;

namespace SaveManager.Components;

/// <summary>
/// Interaction logic for InputDialog.xaml
/// </summary>
public partial class InputDialog : Window
{
    private readonly IValidator? _validator;

    public string Input => InputTextBox.Text;

    public InputDialog(string title, string prompt, int maxLength=0, string startingInput="", IValidator? validator=null)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        InputTextBox.MaxLength = maxLength;
        InputTextBox.Text = startingInput;
        _validator = validator;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (_validator != null && !_validator.IsValid(Input))
        {
            MessageBox.Show(_validator.Message);
            return;
        }

        DialogResult = true;             
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        InputTextBox.SelectAll();
		InputTextBox.Focus();
    }
}
