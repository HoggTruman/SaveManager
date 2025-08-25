using SaveManager.Services;

namespace SaveManager.ViewModels;

public static class ViewModelFactory
{
    private static AppdataService? _appdataService;

    private static AppdataService AppdataService 
    { 
        get
        {
            _appdataService ??= new();            
            return _appdataService;
        }        
    }

    public static SaveViewModel CreateSaveViewModel()
    {
        return new(AppdataService);
    }

    public static GameProfileViewModel CreateGameProfileViewModel()
    {
        return new(AppdataService);
    }
}
