using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.ViewModels;

namespace SaveManager.Models;

public class Profile : NotifyPropertyChanged
{
    private List<IFilesystemItem> _saveListEntries = [];

    public Game Game { get; }
    public Folder Folder { get; }
    public string Name => Folder.Name;
    public List<IFilesystemItem> SaveListEntries 
    { 
        get => _saveListEntries; 
        set => SetProperty(ref _saveListEntries, value); 
    }

    /// <summary>
    /// Initializes a new <see cref="Profile"/> instance.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="game"></param>
    public Profile(Folder folder, Game game)
    {
        Folder = folder;
        Game = game;
        UpdateSaveListEntries();
    }




    /// <summary>
    /// Renames the Profile.
    /// </summary>
    /// <param name="newName"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void Rename(string newName)
    {
        if (Game.Profiles.Any(x => x.Name.FilesystemEquals(newName)))
        {
            throw new ValidationException("A Profile already exists with this name");
        }

        Folder.Rename(newName);
        Game.SortProfiles();
        OnPropertyChanged(nameof(Name));
    }


    /// <summary>
    /// Deletes the <see cref="Profile"/>.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void Delete()
    {
        Folder.Delete();
        Game.Profiles = [..Game.Profiles.Where(x => x != this)];
    }


    /// <summary>
    /// Updates <see cref="SaveListEntries"/> based on the current state of <see cref="Folder"/>.
    /// </summary>
    public void UpdateSaveListEntries()
    {
        SaveListEntries = GetSaveListEntries(Folder);
    }


    public override string ToString()
    {
        return Name;
    }


    /// <summary>
    /// Returns a list of visible entries for a given <see cref="Folder"/>, ordered by folder then alphabetical.
    /// The <see cref="Folder"/> for the active <see cref="Profile"/> should be passed in.
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    private static List<IFilesystemItem> GetSaveListEntries(Folder folder)
    {
        List<IFilesystemItem> entries = [];
        foreach (IFilesystemItem item in folder.Children.OrderByDescending(x => x is Folder))
        {
            entries.Add(item);
            if (item is Folder childFolder && childFolder.IsOpen)
                entries.AddRange(GetSaveListEntries(childFolder));
        }

        return entries;
    }
}
