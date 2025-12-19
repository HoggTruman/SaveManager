using System.Text;
using System.Windows.Input;

namespace SaveManager.Services.Hotkey;

/// <summary>
/// A record containing the key combination registered as a hotkey.
/// </summary>
public record Hotkey
{
    public required Key Key { get; init; }
    public required ModifierKeys ModifierKeys { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (ModifierKeys.HasFlag(ModifierKeys.Control))
            sb.Append("Ctrl + ");
        if (ModifierKeys.HasFlag(ModifierKeys.Shift))
            sb.Append("Shift + ");
        if (ModifierKeys.HasFlag(ModifierKeys.Alt))
            sb.Append("Alt + ");
        if (ModifierKeys.HasFlag(ModifierKeys.Windows))
            sb.Append("Win + ");

        sb.Append(Key);

        return sb.ToString();
    }
}
