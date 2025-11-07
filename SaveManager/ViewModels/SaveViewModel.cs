using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.Models;
using SaveManager.Services.Appdata;
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

    public bool CanClickCreateProfile => ActiveGame != null;
    public bool CanRenameProfile => ActiveGame != null && ActiveGame.ActiveProfile != null;
    public bool CanDeleteProfile => ActiveGame != null && ActiveGame.ActiveProfile != null;
    public bool CanAddFolder => ActiveGame != null && ActiveGame.ActiveProfile != null;
    public bool CanDelete => ActiveGame != null && ActiveGame.ActiveProfile != null && SelectedEntry != null;
    public bool CanRename => ActiveGame != null && ActiveGame.ActiveProfile != null && SelectedEntry != null;
    public bool CanRefresh => ActiveGame != null;
    public bool CanImportSave => ActiveGame != null && ActiveGame.ActiveProfile != null;
    public bool CanLoadSave => ActiveGame != null && ActiveGame.ActiveProfile != null && SelectedEntry is Savefile;
    public bool CanReplaceSave => ActiveGame != null && ActiveGame.ActiveProfile != null && SelectedEntry is Savefile;




    public SaveViewModel(AppdataService appdataService, IEnumerable<Game> games)
    {
        _appdataService = appdataService;
        Games = [..games];
        ActiveGame = games.FirstOrDefault();
    }




    /// <summary>
    /// Raises OnPropertyChanged for the properties which control whether buttons are enabled.
    /// </summary>
    public void EnableDisableButtons()
    {
        OnPropertyChanged(nameof(CanClickCreateProfile));
        OnPropertyChanged(nameof(CanRenameProfile));
        OnPropertyChanged(nameof(CanDeleteProfile));
        OnPropertyChanged(nameof(CanImportSave));
        OnPropertyChanged(nameof(CanLoadSave));
        OnPropertyChanged(nameof(CanReplaceSave));
    }


    /// <summary>
    /// Creates a new profile.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void CreateProfile(string name)
    {
        if (ActiveGame == null)
        {
            return;
        }

        ActiveGame.ActiveProfile = Profile.Create(name, ActiveGame);
    }


    /// <summary>
    /// Renames the selected profile.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void RenameProfile(string newName)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
        {
            return;
        }

        ActiveGame.ActiveProfile.Rename(newName);
    }


    /// <summary>
    /// Deletes the selected profile.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void DeleteProfile()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
        {
            return;
        }

        ActiveGame.ActiveProfile.Delete();
    }


    /// <summary>
    /// If the selected entry is a folder, shows / hides its children.
    /// </summary>
    public void OpenCloseSelectedEntry()
    {
        if (SelectedEntry == null || SelectedEntry is Savefile)
        {
            return;
        }            

        Folder folder = (Folder)SelectedEntry;
        folder.IsOpen = !folder.IsOpen;
        ActiveGame?.ActiveProfile?.UpdateSaveListEntries();
    }


    /// <summary>
    /// Moves an entry into a different folder based on selection.
    /// </summary>
    /// <param name="movingEntry"></param>
    /// <param name="targetEntry"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void MoveEntry(IFilesystemItem movingEntry, IFilesystemItem? targetEntry)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
            throw new InvalidOperationException("The ActiveGame and ActiveProfile can not be null");
        
        Folder newParent = GetParentFromSelection(targetEntry);

        if (movingEntry.Parent != newParent && movingEntry != newParent)
        {
            movingEntry.Move(newParent);
            newParent.IsOpen = true;
            ActiveGame.ActiveProfile.UpdateSaveListEntries();
            SelectedEntry = movingEntry;
        }
    }




    /// <summary>
    /// Adds a new folder entry, with location based on the current selection.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="ValidationException"></exception>
    public void AddFolder(string name)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
            throw new InvalidOperationException("The ActiveGame and its ActiveProfile can not be null.");

        Folder parentFolder = GetParentFromSelection(SelectedEntry);
        Folder newFolder = Folder.Create(name, parentFolder);
        parentFolder.IsOpen = true;
        ActiveGame.ActiveProfile.UpdateSaveListEntries();
        SelectedEntry = newFolder;
    }


    /// <summary>
    /// Deletes the selected entry.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void DeleteSelectedEntry()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null || SelectedEntry == null)
            throw new InvalidOperationException("The ActiveGame, ActiveProfile and SelectedEntry can not be null");

        SelectedEntry.Delete();
        ActiveGame.ActiveProfile.UpdateSaveListEntries();
        SelectedEntry = null;
    }


    /// <summary>
    /// Renames the selected entry.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void RenameSelectedEntry(string newName)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null || SelectedEntry == null)
            throw new InvalidOperationException("The ActiveGame, ActiveProfile and SelectedEntry can not be null");

        SelectedEntry.Rename(newName);
        ActiveGame.ActiveProfile.UpdateSaveListEntries();
    }


    /// <summary>
    /// Reloads the active game's profiles from the filesystem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void RefreshProfiles()
    {
        if (ActiveGame == null)
            return;

        SelectedEntry = null;
        ActiveGame.RefreshProfiles();
    }




    /// <summary>
    /// Creates a copy of the active game's savefile in a folder based on the current selection.
    /// </summary>
    /// <exception cref="SavefileNotFoundException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void ImportSavefile()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null || ActiveGame.Savefile == null)
        {
            return;
        }

        try
        {
            Folder parent = GetParentFromSelection(SelectedEntry);        
            Savefile copiedSavefile = ActiveGame.Savefile.CopyTo(parent);
            parent.IsOpen = true;
            ActiveGame.ActiveProfile.UpdateSaveListEntries();
            SelectedEntry = copiedSavefile;
        }
        catch (FilesystemMismatchException ex)
        {
            if (ex.Location == ActiveGame.SavefileLocation)
            {
                ActiveGame.SavefileLocation = null;
                throw new SavefileNotFoundException("The active game's save file does not exist.");
            }
            
            throw;
        }
    }


    /// <summary>
    /// Overwrites the active game's savefile with the selected entry if it is a file.
    /// </summary>
    /// <exception cref="SavefileNotFoundException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void LoadSelectedEntry()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null || ActiveGame.Savefile == null || SelectedEntry is not Savefile)
        {
            return;
        }

        try
        {
            ActiveGame.Savefile.OverwriteContents((Savefile)SelectedEntry);
        }
        catch (FilesystemMismatchException ex)
        {
            if (ex.Location == ActiveGame.SavefileLocation)
            {
                ActiveGame.SavefileLocation = null;
                throw new SavefileNotFoundException("The active game's save file does not exist.");
            }                

            throw;
        }
    }


    /// <summary>
    /// Overwrites the selected entry if it is a file, with the active game's savefile.
    /// </summary>
    /// <exception cref="SavefileNotFoundException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void ReplaceSelectedEntry()
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null || ActiveGame.Savefile == null || SelectedEntry is not Savefile)
        {
            return;
        }

        try
        {
            ((Savefile)SelectedEntry).OverwriteContents(ActiveGame.Savefile);
        }
        catch (FilesystemMismatchException ex)
        {
            if (ex.Location == ActiveGame.SavefileLocation)
            {
                ActiveGame.SavefileLocation = null;
                throw new SavefileNotFoundException("The active game's save file does not exist.");
            }                

            throw;
        }
    }


    /// <summary>
    /// Saves games and startup preferences to the appdata file.
    /// </summary>
    /// <param name="startupPreferences"></param>
    /// <exception cref="AppdataException"></exception>
    public void SaveAppdata(StartupPreferences startupPreferences)
    {
        _appdataService.SetStartupPreferences(startupPreferences);
        _appdataService.SetGameData(Games);
        _appdataService.SaveAppdata();
    }


    /// <summary>
    /// Returns true if the dragged item is allowed to be dropped on the drop item.
    /// </summary>
    /// <param name="dragged">The item being dragged.</param>
    /// <param name="drop">The item being dropped on.</param>
    /// <returns></returns>
    public static bool IsValidDropTarget(IFilesystemItem dragged, IFilesystemItem? drop)
    {
        if (dragged is Savefile || drop == null || dragged == drop)
        {
            return true;
        }

        return !drop.Location.IsDescendantOf(dragged.Location);
    }


    /// <summary>
    /// Retrieves the parent <see cref="Folder"/> based on the selected save list entry.
    /// </summary>
    /// <returns>The parent <see cref="Folder"/> of the selection.</returns>
    private Folder GetParentFromSelection(IFilesystemItem? selection)
    {
        if (ActiveGame == null || ActiveGame.ActiveProfile == null)
            throw new InvalidOperationException("The ActiveGame and its ActiveProfile can not be null.");

        if (selection is Folder)
            return (Folder)selection;

        if (selection is Savefile)
            return selection.Parent!;
        
        return ActiveGame.ActiveProfile.Folder;
    }
}
