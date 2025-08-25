using SaveManager.Exceptions;
using SaveManager.Models;
using SaveManager.Services;
using SaveManager.Validators;
using System.Collections.ObjectModel;
using System.IO;

namespace SaveManager.ViewModels;

public class GameProfileViewModel
{
    private readonly AppdataService _appdataService;

    public ObservableCollection<Game> Games { get; set; } = [];
    public Game? ActiveGame { get; set; }

    public ObservableCollection<Profile> Profiles { get; set; } = [];
    public Profile? ActiveProfile { get; set; }

    public GameNameValidator GameNameValidator { get; }

    public GameProfileViewModel(AppdataService appdataService)
    {
        _appdataService = appdataService;
        GameNameValidator = new(this);
    }




    /// <summary>
    /// Adds a new game and updates appdata.
    /// </summary>
    /// <param name="gameName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="AppdataException"></exception>
    public void AddGame(string gameName)
    {
        if (Games.Any(x => x.Name.Equals(gameName, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"A game already exists with name: {gameName}");
        }

        Game newGame = new() { Name = gameName };
        _appdataService.AddGame(newGame);

        Games = [..Games.Append(newGame).OrderBy(x => x.Name)];
        ActiveGame = newGame;
        Profiles = [];
        ActiveProfile = null;
    }


    /// <summary>
    /// Renames a game and updates appdata.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="newName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="AppdataException"></exception>
    public void RenameGame(Game game, string newName)
    {
        if (game == null)
        {
            return;
        }

        if (Games.Any(x => x.Name.Equals(newName, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"A game already exists with name: {newName}");
        }

        _appdataService.RenameGame(game, newName);
        game.Name = newName;
        Games = [..Games.OrderBy(x => x.Name)];
    }


    /// <summary>
    /// Deletes a game and updates appdata.
    /// </summary>
    /// <param name="game"></param>
    /// <exception cref="AppdataException"></exception>
    public void DeleteGame(Game game)
    {
        if (game == null)
        {
            return;
        }

        _appdataService.DeleteGame(game);
        Games = [..Games.Where(x => x != game)];
        ActiveGame = Games.LastOrDefault();
        Profiles = ActiveGame != null? [..ActiveGame.Profiles.OrderBy(x => x.Name)]: [];
        ActiveProfile = null;
    }




    /// <summary>
    /// Reloads the Games and Profiles from the filesystem.
    /// </summary>
    /// <exception cref="IOException"/>
    public void Refresh()
    {
        LoadGames();
        LoadProfiles();
    }


    internal void LoadGames()
    {
        Games = [.. _appdataService.GetGames().OrderBy(x => x.Name)];
        Game? gameWithSameName = Games.FirstOrDefault(x => x.Name == ActiveGame?.Name);
        ActiveGame = gameWithSameName ?? Games.FirstOrDefault();
    }

    internal void LoadProfiles()
    {
        foreach (Game game in Games)
        {
            game.LoadProfiles();
            foreach (Profile profile in game.Profiles)
            {
                profile.LoadChildren();
            }
        }

        if (ActiveGame == null)
        {
            ActiveProfile = null;
            Profiles = [];
        }
        else
        {
            Profiles = [..ActiveGame.Profiles.OrderBy(x => x.Name)];
            Profile? profileWithSameLocation = Profiles.FirstOrDefault(x => x.Location == ActiveProfile?.Location);
            ActiveProfile = profileWithSameLocation ?? Profiles.FirstOrDefault();
        }
    }
}
