using SaveManager.Exceptions;
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


    public bool CanAddFolder => ActiveGame != null && ActiveGame.ActiveProfile != null;
    public bool CanDelete => false;
    public bool CanRename => false;
    public bool CanRefresh => false;


    public SaveViewModel(AppdataService appdataService, IEnumerable<Game> games)
    {
        _appdataService = appdataService;
        Games = [..games];
        ActiveGame = games.FirstOrDefault();
    }



    /// <summary>
    /// If the selected entry is a folder, shows / hides its children.
    /// </summary>
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


    /// <summary>
    /// Adds a new folder entry, with location based on the current selection.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void AddFolder(string name)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
            throw new InvalidOperationException("The ActiveGame and its ActiveProfile can not be null.");

        Folder parentFolder = GetParentFromSelection();
        Folder newFolder = Folder.Create(name, parentFolder);
        parentFolder.IsOpen = true;
        ActiveGame.ActiveProfile.UpdateSaveListEntries();
        SelectedEntry = newFolder;
    }


    /// <summary>
    /// Reloads the active game's profiles from the filesystem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void RefreshProfiles()
    {
        if (ActiveGame == null)
            return;

        SelectedEntry = null;
        ActiveGame.RefreshProfiles();        
    }


    /// <summary>
    /// Retrieves the parent <see cref="Folder"/> based on the selected save list entry.
    /// </summary>
    /// <returns>The parent <see cref="Folder"/> of the selection.</returns>
    private Folder GetParentFromSelection()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
            throw new InvalidOperationException("The ActiveGame and its ActiveProfile can not be null.");

        if (SelectedEntry == null)
            return ActiveGame.ActiveProfile.Folder;

        if (SelectedEntry is File)
            return SelectedEntry.Parent!;

        return (Folder)SelectedEntry;
    }
}
