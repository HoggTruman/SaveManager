using SaveManager.Exceptions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SaveManager.Services.Hotkey;

public class HotkeyService : IHotkeyService, IDisposable
{
    /// <summary>
    /// Windows message hotkey code
    /// </summary>
    private const int  WM_HOTKEY = 0x0312;

    /// <summary>
    /// Keys reserved by the application for other operations
    /// </summary>
    private readonly HashSet<Key> ReservedKeys = [Key.Delete, Key.F2, Key.F12];  


    private readonly IntPtr _windowHandle;
    private readonly HwndSource _source;

    /// <summary>
    /// A dictionary containing mappings of HotkeyActionId and associated action.
    /// </summary>
    public Dictionary<HotkeyActionId, Action> HotkeyActions { get; } = [];

    /// <summary>
    /// A dictionary containing mappings of HotKeyActionId the registered hotkey.
    /// </summary>
    private Dictionary<HotkeyActionId, Hotkey> Hotkeys { get; } = [];
    
    


    /// <summary>
    /// Initializes a HotkeyService for the provided window.
    /// </summary>
    /// <param name="window">The window the hotkeys will be registered to.</param>
    public HotkeyService(Window window)
    {
        _windowHandle = new  WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(_windowHandle);
        _source.AddHook(HwndHook);
    }




    /// <summary>
    /// Registers the provided key combination as a global hotkey for the action with corresponding HotkeyActionId.
    /// </summary>
    /// <param name="hotkeyActionId">The action to register the hotkey to.</param>
    /// <param name="hotkey">The hotkey containing the key combination to register.</param>
    /// <exception cref="ValidationException">An exception thrown if the user has provided an invalid hotkey.</exception>
    /// <exception cref="ExternalException">An exception thrown if a global hotkey fails to register.</exception>
    public void RegisterHotKey(HotkeyActionId hotkeyActionId, Hotkey hotkey)
    {
        if (!HotkeyActions.ContainsKey(hotkeyActionId))
        {
            throw new InvalidOperationException("An action must be registered to this id before setting a hotkey.");
        }

        if (ReservedKeys.Contains(hotkey.Key))
        {
            throw new ValidationException($"{hotkey.Key} is reserved. Please choose a different key.");
        }

        if (Hotkeys.ContainsValue(hotkey))
        {
            throw new ValidationException("This hotkey is already registered to an action.");
        }

        int id = (int)hotkeyActionId;
        uint fsModifiers = (uint)hotkey.ModifierKeys;
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(hotkey.Key);

        if (Hotkeys.ContainsKey(hotkeyActionId))
        {
            if (!UnregisterHotKey(_windowHandle, id))
            {
                throw new ExternalException("An error occured while unregistering the existing hotkey.");
            }

            Hotkeys.Remove(hotkeyActionId);
        }

        if (!RegisterHotKey(_windowHandle, id, fsModifiers, vk))
        {
            throw new ExternalException("An error occured while registering the hotkey.");
        }

        Hotkeys[hotkeyActionId] = hotkey;
    }


    /// <summary>
    /// Unregisters the hotkey assigned to an action.
    /// </summary>
    /// <param name="hotkeyActionId">The action to unregister a hotkey from.</param>
    /// <exception cref="ExternalException">An exception thrown if a global hotkey fails to unregister.</exception>
    public void UnregisterHotkey(HotkeyActionId hotkeyActionId)
    {
        if (!HotkeyActions.ContainsKey(hotkeyActionId))
        {
            throw new InvalidOperationException("No action is registered to this id.");
        }

        if (!Hotkeys.ContainsKey(hotkeyActionId))
        {
            throw new InvalidOperationException("A hotkey is not registered for the provided action");
        }

        if (!UnregisterHotKey(_windowHandle, (int)hotkeyActionId))
        {
            throw new ExternalException("An error occured while unregistering the hotkey.");
        }

        Hotkeys.Remove(hotkeyActionId);
    }


    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
  

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


    /// <summary>
    /// The event handler that receives all window messages.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="msg">The message ID.</param>
    /// <param name="wParam">The message's wParam value.</param>
    /// <param name="lParam">The message's lParam value.</param>
    /// <param name="handled">A value that indicates whether the message was handled. Set the value to true if the message was handled; otherwise, false.</param>
    /// <returns></returns>
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            HotkeyActionId actionId = (HotkeyActionId)wParam.ToInt32();
            HotkeyActions[actionId].Invoke();
            handled = true;
        }        

        return IntPtr.Zero;
    }


    private void ClearHotkeys()
    {
        foreach (HotkeyActionId id in Hotkeys.Keys)
        {
            UnregisterHotKey(_windowHandle, (int)id);
        }

        Hotkeys.Clear();
    }


    public void Dispose()
    {
        ClearHotkeys();
        _source.RemoveHook(HwndHook);
    }
}
