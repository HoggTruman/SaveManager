namespace SaveManager.Services.Hotkey;

public interface IHotkeyService
{
    /// <summary>
    /// A dictionary containing mappings of HotkeyActionId and the associated action.
    /// </summary>
    public Dictionary<HotkeyActionId, Action> HotkeyActions { get; }

    /// <summary>
    /// Registers the provided key combination as a global hotkey for the action with corresponding HotkeyActionId.
    /// </summary>
    /// <param name="hotkeyActionId">The action to register the hotkey to.</param>
    /// <param name="hotkey">The hotkey containing the key combination to register.</param>
    /// <exception cref="ValidationException">An exception thrown if the user has provided an invalid hotkey.</exception>
    /// <exception cref="ExternalException">An exception thrown if a global hotkey fails to register.</exception>
    public void RegisterHotKey(HotkeyActionId hotkeyActionId, Hotkey hotkey);

    /// <summary>
    /// Unregisters the hotkey assigned to an action.
    /// </summary>
    /// <param name="hotkeyActionId">The action to unregister a hotkey from.</param>
    /// <exception cref="ExternalException">An exception thrown if a global hotkey fails to unregister.</exception>
    public void UnregisterHotkey(HotkeyActionId hotkeyActionId);
}