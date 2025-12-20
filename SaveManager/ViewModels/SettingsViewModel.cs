using SaveManager.Services.Appdata;
using SaveManager.Services.Hotkey;

namespace SaveManager.ViewModels;

public class SettingsViewModel : NotifyPropertyChanged
{
    private readonly IHotkeyService _hotkeyService;
    private readonly Settings _currentSettings;

    private bool _globalHotkeys = false;
    private Hotkey? _loadSelectedSaveHotkey;
    private Hotkey? _importSaveHotkey;

    public bool GlobalHotkeys 
    { 
        get => _globalHotkeys; 
        set => SetProperty(ref _globalHotkeys, value); 
    }

    public Hotkey? LoadSelectedSaveHotkey 
    {
        get => _loadSelectedSaveHotkey; 
        set => SetProperty(ref _loadSelectedSaveHotkey, value); 
    }

    public Hotkey? ImportSaveHotkey 
    { 
        get => _importSaveHotkey; 
        set => SetProperty(ref _importSaveHotkey, value); 
    }


    public SettingsViewModel(IHotkeyService hotkeyService, IAppdataService appdataService)
    {
        _hotkeyService = hotkeyService;
        _currentSettings = appdataService.GetSettings();
        GlobalHotkeys = _currentSettings.GlobalHotkeys;
        LoadSelectedSaveHotkey = _currentSettings.LoadSelectedSaveHotkey;
        ImportSaveHotkey = _currentSettings.ImportSaveHotkey;
    }
}
