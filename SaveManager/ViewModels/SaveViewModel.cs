using SaveManager.Models;
using SaveManager.Services;
using System.Collections.ObjectModel;

namespace SaveManager.ViewModels;

public class SaveViewModel : NotifyPropertyChanged
{
    private readonly AppdataService _appdataService;

    private ObservableCollection<Game> _games = [];
    private Game? _activeGame;
    private Profile? _activeProfile;
    private IFilesystemItem? _selectedItem;

    public ObservableCollection<Game> Games 
    { 
        get => _games; 
        set => SetProperty(ref _games, value); 
    }

    public Game? ActiveGame
    { 
        get => _activeGame; 
        set
        { 
            SetProperty(ref _activeGame, value); 
            ActiveProfile = value?.Profiles.FirstOrDefault();
        }
    }

    public Profile? ActiveProfile 
    { 
        get => _activeProfile; 
        set => SetProperty(ref _activeProfile, value); 
    }

    public IFilesystemItem? SelectedItem
    { 
        get => _selectedItem; 
        set => SetProperty(ref _selectedItem, value); 
    }

    public SaveViewModel(AppdataService appdataService, IEnumerable<Game> games)
    {
        _appdataService = appdataService;
        Games = [..games];
        ActiveGame = games.FirstOrDefault();
        ActiveProfile = ActiveGame?.Profiles.FirstOrDefault();
    }
    
}
