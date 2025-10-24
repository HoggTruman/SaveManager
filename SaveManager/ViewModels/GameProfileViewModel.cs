using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.Models;
using SaveManager.Services.Appdata;
using System.Collections.ObjectModel;

namespace SaveManager.ViewModels;

public class GameProfileViewModel : NotifyPropertyChanged
{
    private readonly AppdataService _appdataService;

    private ObservableCollection<Game> _games = [];
    private Game? _activeGame;
    private Profile? _selectedProfile;


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

    public Profile? SelectedProfile
    { 
        get => _selectedProfile;
        set => SetProperty(ref _selectedProfile, value);
    }
    



    /// <summary>
    /// Instantiates a new GameProfileViewModel.
    /// </summary>
    /// <param name="appdataService"></param>
    public GameProfileViewModel(AppdataService appdataService, IEnumerable<Game> games, Game? activeGame)
    {
        _appdataService = appdataService;
        Games = [..games];
        ActiveGame = activeGame;
    }




    /// <summary>
    /// Adds a new game.
    /// </summary>
    /// <param name="name">The name of the new game.</param>
    /// <exception cref="ValidationException"></exception>
    public void AddGame(string name)
    {
        if (Games.Any(x => x.Name.FilesystemEquals(name)))
        {
            throw new ValidationException($"A game already exists with this name.");
        }

        Game newGame = new(name);
        Games = [..Games.Append(newGame).OrderBy(x => x.Name)];
        ActiveGame = newGame;
        SelectedProfile = null;
    }


    /// <summary>
    /// Renames the active game.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    public void RenameGame(string newName)
    {
        if (ActiveGame == null)
        {
            return;
        }

        if (Games.Any(x => x.Name.FilesystemEquals(newName)))
        {
            throw new ValidationException($"A game already exists with name.");
        }

        ActiveGame.Name = newName;
        Games = [..Games.OrderBy(x => x.Name)];
    }


    /// <summary>
    /// Removes the active game.
    /// </summary>
    public void RemoveGame()
    {
        if (ActiveGame == null)
        {
            return;
        }

        Games = [..Games.Where(x => x != ActiveGame)];
        ActiveGame = Games.LastOrDefault();
        SelectedProfile = null;
    }


    /// <summary>
    /// Sets the savefile location of the active game.
    /// </summary>
    /// <param name="location"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void SetSavefileLocation(string location)
    {
        if (ActiveGame == null)
        {
            return;
        }

        IEnumerable<string> profilesDirectories = Games.Where(x => x.ProfilesDirectory != null).Select(x => x.ProfilesDirectory!);

        if (Games.Any(x => x.SavefileLocation != null && x.SavefileLocation.FilesystemEquals(location)))
            throw new ValidationException("Another game uses this savefile.");

        if (profilesDirectories.Any(location.IsDescendantOf))
            throw new ValidationException("This file is a descendant of a game's profiles directory.");

        ActiveGame.SavefileLocation = location;
    }


    /// <summary>
    /// Sets the profiles directory of the active game.
    /// </summary>
    /// <param name="location"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void SetProfilesDirectory(string location)
    {
        if (ActiveGame == null || ActiveGame.ProfilesDirectory == location)
        {
            return;
        }

        IEnumerable<string> profilesDirectories = Games.Select(x => x.ProfilesDirectory).OfType<string>();
        IEnumerable<string> savefileLocations = Games.Select(x => x.SavefileLocation).OfType<string>();

        if (profilesDirectories.Contains(location))
            throw new ValidationException("Another game uses this folder as a profiles directory.");

        if (profilesDirectories.Any(x => x.IsDescendantOf(location) || location.IsDescendantOf(x)))
            throw new ValidationException("This folder is the parent / child of another game's profiles directory.");

        if (savefileLocations.Any(x => x.IsDescendantOf(location)))
            throw new ValidationException("This folder contains a game's savefile.");

        ActiveGame.ProfilesDirectory = location;        
    }


    /// <summary>
    /// Creates a new profile.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void CreateProfile(string name)
    {
        if (ActiveGame == null)
        {
            return;
        }

        Profile.Create(name, ActiveGame);
    }


    /// <summary>
    /// Renames the selected profile.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void RenameProfile(string newName)
    {
        if (SelectedProfile == null)
        {
            return;
        }

        SelectedProfile.Rename(newName);        
    }


    /// <summary>
    /// Deletes the selected profile.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemItemNotFoundException"></exception>
    public void DeleteProfile()
    {
        if (SelectedProfile == null)
        {
            return;
        }

        SelectedProfile.Delete();
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

        SelectedProfile = null;
        ActiveGame.RefreshProfiles();        
    }



    /// <summary>
    /// Saves the current games to the appdata file.
    /// </summary>
    /// <exception cref="AppdataException"/>
    public void SaveGameChanges()
    {
        _appdataService.SetGameData(Games);
        _appdataService.SaveAppdata();
    }
}
