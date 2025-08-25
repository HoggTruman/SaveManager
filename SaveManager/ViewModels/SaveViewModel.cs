using SaveManager.Models;
using SaveManager.Services;
using System.Collections.ObjectModel;
using System.IO;

namespace SaveManager.ViewModels;

public class SaveViewModel
{
    private readonly AppdataService _appdataService;

    public ObservableCollection<Game> Games { get; set; } = [];
    public Game? ActiveGame { get; set; }

    public ObservableCollection<Profile> Profiles { get; set; } = [];
    public Profile? ActiveProfile { get; set; }

    public ObservableCollection<IFilesystemItem> SaveListItems { get; set; } = [];
    public IFilesystemItem? SelectedItem { get; set; }

    public SaveViewModel(AppdataService appdataService)
    {
        _appdataService = appdataService;
    }
    
}
