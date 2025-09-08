using SaveManager.Exceptions;
using SaveManager.ViewModels;
using System.Collections.ObjectModel;

namespace SaveManager.Models;

public class Game : NotifyPropertyChanged
{
    private string _name = "";
    private string? _savefileLocation;
    private Folder? _profilesFolder;
    private ObservableCollection<Profile> _profiles = [];
    


    /// <summary>
    /// The name of the game.
    /// </summary>
    /// <exception cref="ValidationException"></exception>
    public string Name
    { 
        get => _name; 
        set
        {
            ValidateName(value);
            SetProperty(ref _name, value);
        }
    }

    /// <summary>
    /// The full path of the savefile.
    /// </summary>
    public string? SavefileLocation
    { 
        get => _savefileLocation; 
        set => SetProperty(ref _savefileLocation, value); 
    }

    /// <summary>
    /// The <see cref="Folder"/> representing the profiles directory if one has been set. Otherwise, null.
    /// </summary>
    public Folder? ProfilesFolder => _profilesFolder;


    /// <summary>
    /// The full path of the directory containing profile directories.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    public string? ProfilesDirectory
    { 
        get => _profilesFolder?.Location; 
        set
        {
            if (value == null)
            {
                SetProperty(ref _profilesFolder, null);
                Profiles = [];
            }
            else
            {
                try
                {
                    Folder newFolder = new(value, null);
                    SetProperty(ref _profilesFolder, newFolder);
                    Profiles = [..newFolder.Children.Where(x => x is Folder).Select(x => new Profile((Folder)x, this))];
                }
                catch (Exception ex)
                {
                    // If profiles directory can not be accessed, remove it to prevent locking app.
                    if (ex is FilesystemException)
                    {
                        SetProperty(ref _profilesFolder, null);
                        Profiles = [];
                    }
                    
                    throw;
                }
            }
        }
    }

    public ObservableCollection<Profile> Profiles 
    { 
        get => _profiles; 
        set => SetProperty(ref _profiles, value); 
    }





    /// <summary>
    /// Initializes a new <see cref="Game"/> instance. 
    /// Automatically loads its profiles from the filesystem.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="savefileLocation"></param>
    /// <param name="profilesDirectory"></param>
    /// <exception cref="ValidationException">An invalid name is provided.</exception>
    /// <exception cref="FilesystemException"></exception>
    public Game(string name, string? savefileLocation=null, string? profilesDirectory=null)
    {
        Name = name;
        SavefileLocation = savefileLocation;
        try
        {
            ProfilesDirectory = profilesDirectory;
        }
        catch (Exception ex)
        {
            // Games are only constructed with a profilesDirectory on app startup.
            // If the directory is no longer accessible, it will be set to null in ProfilesDirectory setter and we can continue.
            if (ex is FilesystemException)
                return;

            throw;
        }        
    }


    /// <summary>
    /// Reloads the game's profiles from the filesystem.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    public void RefreshProfiles()
    {
        ProfilesDirectory = _profilesFolder?.Location;
    }


    /// <summary>
    /// Sorts profiles by name.
    /// </summary>
    public void SortProfiles()
    {
        Profiles = [..Profiles.OrderBy(x => x.Name)];
    }


    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Game name can not be empty or whitespace.");
    }
}
