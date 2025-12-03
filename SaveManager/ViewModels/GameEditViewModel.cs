using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.Models;
using SaveManager.Services.Appdata;
using System.Collections.ObjectModel;

namespace SaveManager.ViewModels;

public class GameEditViewModel : NotifyPropertyChanged
{
    private readonly IAppdataService _appdataService;

    private ObservableCollection<Game> _games = [];
    private Game? _activeGame;


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
    



    /// <summary>
    /// Instantiates a new GameEditViewModel.
    /// </summary>
    /// <param name="appdataService"></param>
    public GameEditViewModel(IAppdataService appdataService)
    {
        _appdataService = appdataService;
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
    }


    /// <summary>
    /// Sets the savefile location of the active game.
    /// </summary>
    /// <param name="location"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void SetSavefileLocation(string? location)
    {
        if (ActiveGame == null || ActiveGame.SavefileLocation == location)
        {
            return;
        }

        if (location != null)
        {
            IEnumerable<string> profilesDirectories = Games.Select(x => x.ProfilesDirectory).OfType<string>();

            if (Games.Any(x => x.SavefileLocation != null && x.SavefileLocation.FilesystemEquals(location)))
                throw new ValidationException("Another game uses this savefile.");

            if (profilesDirectories.Any(location.IsDescendantOf))
                throw new ValidationException("This file is a descendant of a game's profiles directory.");
        }

        ActiveGame.SavefileLocation = location;
    }


    /// <summary>
    /// Sets the profiles directory of the active game.
    /// </summary>
    /// <param name="location"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    public void SetProfilesDirectory(string? location)
    {
        if (ActiveGame == null || ActiveGame.ProfilesDirectory == location)
        {
            return;
        }

        if (location != null)
        {
            IEnumerable<string> profilesDirectories = Games.Where(x => x != ActiveGame).Select(x => x.ProfilesDirectory).OfType<string>();
            IEnumerable<string> savefileLocations = Games.Select(x => x.SavefileLocation).OfType<string>();

            if (profilesDirectories.Any(x => x.FilesystemEquals(location)))
                throw new ValidationException("Another game uses this folder as a profiles directory.");

            if (profilesDirectories.Any(x => x.IsDescendantOf(location) || location.IsDescendantOf(x)))
                throw new ValidationException("This folder is the parent / child of another game's profiles directory.");

            if (savefileLocations.Any(x => x.IsDescendantOf(location)))
                throw new ValidationException("This folder contains a game's savefile.");
        }        

        ActiveGame.ProfilesDirectory = location;        
    }


    /// <summary>
    /// Saves the current games to the appdata file.
    /// </summary>
    /// <exception cref="AppdataException"/>
    public void SaveGamesToAppdata()
    {
        _appdataService.SetGameData(Games);
        _appdataService.SaveAppdata();
    }
}
