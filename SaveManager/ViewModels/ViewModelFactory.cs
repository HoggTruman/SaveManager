using SaveManager.Models;
using SaveManager.Services;

namespace SaveManager.ViewModels;

public static class ViewModelFactory
{
    private static AppdataService? _appdataService;

    public static AppdataService AppdataService 
    { 
        get
        {
            _appdataService ??= new();            
            return _appdataService;
        }        
    }

    public static SaveViewModel CreateSaveViewModel(IEnumerable<Game> games)
    {
        return new(AppdataService, games);
    }

    public static GameProfileViewModel CreateGameProfileViewModel(IEnumerable<Game> games)
    {
        return new(AppdataService, games);
    }
}
