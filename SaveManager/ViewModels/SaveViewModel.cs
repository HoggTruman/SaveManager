using SaveManager.Models;
using SaveManager.Services;
using System.Collections.ObjectModel;

namespace SaveManager.ViewModels;

public class SaveViewModel : NotifyPropertyChanged
{
    private readonly AppdataService _appdataService;

    private ObservableCollection<Game> _games = [];
    private Game? _activeGame;
    private IFilesystemItem? _selectedEntry;


    public ObservableCollection<Game> Games 
    { 
        get => _games; 
        set => SetProperty(ref _games, value);
    }


    public Game? ActiveGame
    { 
        get => _activeGame; 
        set => SetProperty(ref _activeGame, value);
    }


    public IFilesystemItem? SelectedEntry
    { 
        get => _selectedEntry; 
        set => SetProperty(ref _selectedEntry, value); 
    }




    public SaveViewModel(AppdataService appdataService, IEnumerable<Game> games)
    {
        _appdataService = appdataService;
        Games = [..games];
        ActiveGame = games.FirstOrDefault();
    }




    public void OpenCloseSelectedEntry()
    {
        if (SelectedEntry == null || SelectedEntry is File)
        {
            return;
        }            

        Folder folder = (Folder)SelectedEntry;
        folder.IsOpen = !folder.IsOpen;
        ActiveGame?.ActiveProfile?.UpdateSaveListEntries();
    }
}
