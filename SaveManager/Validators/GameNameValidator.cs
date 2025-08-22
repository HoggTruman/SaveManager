using SaveManager.ViewModels;

namespace SaveManager.Validators;

public class GameNameValidator : IValidator
{
    private readonly GameProfileViewModel _gameProfileViewModel;
    public string Message { get; private set; } = "";

    public GameNameValidator(GameProfileViewModel gameProfileViewModel)
    {
        _gameProfileViewModel = gameProfileViewModel;
    }

    public bool IsValid(string gameName)
    {
        if (gameName.Length == 0) 
        { 
            Message = "A game's name can not be empty.";
            return false;
        } 

        if (_gameProfileViewModel.Games.Any(x => x.Name == gameName))
        {
            Message = "A game already exists with this name.";
            return false;
        }

        Message = "";
        return true;
    }
}
