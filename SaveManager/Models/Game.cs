using SaveManager.Exceptions;
using SaveManager.Helpers;
using SaveManager.ViewModels;
using System.Collections.ObjectModel;

namespace SaveManager.Models;

public class Game : NotifyPropertyChanged
{
    private string _name = "";
    private Savefile? _savefile;
    private Folder? _profilesFolder;    
    private ObservableCollection<Profile> _profiles = [];
    private Profile? _activeProfile;
    


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
    /// The <see cref="Models.Savefile"/> representing the game's savefile if one has been set. Otherwise, null.
    /// </summary>
    public Savefile? Savefile => _savefile;


    /// <summary>
    /// The full path of the savefile.
    /// </summary>
    public string? SavefileLocation
    { 
        get => _savefile?.Location; 
        set
        {
            if (value == null)
            {
                SetProperty(ref _savefile, null);
            }
            else
            {
                Savefile savefile = FilesystemItemFactory.NewSavefile(value, null);
                SetProperty(ref _savefile, savefile); 
            }            
        }
    }


    /// <summary>
    /// The full path of the directory containing the game's profiles.
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
                Folder newProfilesFolder = FilesystemItemFactory.NewFolder(value, null);
                newProfilesFolder.LoadChildren();
                _profilesFolder = newProfilesFolder;
                Profiles = [..newProfilesFolder.Children.OfType<Folder>().Select(x => new Profile(x, this))];             
            }
        }
    }


    public ObservableCollection<Profile> Profiles 
    { 
        get => _profiles; 
        set 
        {
            SetProperty(ref _profiles, value);
            if (ActiveProfile == null || !value.Contains(ActiveProfile))
            {
                ActiveProfile = value.FirstOrDefault();
            }                
        }
    }


    public Profile? ActiveProfile
    {
        get => _activeProfile;
        set => SetProperty(ref _activeProfile, value);
    }


    /// <summary>
    /// Initializes a new <see cref="Game"/> instance.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="savefileLocation"></param>
    /// <param name="profilesDirectory"></param>
    /// <exception cref="ValidationException">An invalid name is provided.</exception>
    public Game(string name)
    {
        Name = name;     
    }


    /// <summary>
    /// Creates a new profile in the game's profiles directory.
    /// Returns a profile instance representing it.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public Profile CreateProfile(string name)
    {
        if (_profilesFolder == null || ProfilesDirectory == null)
        {
            throw new InvalidOperationException("ProfilesDirectory must be set before a profile can be created.");
        }

        if (Profiles.Any(x => x.Name.FilesystemEquals(name)))
        {
            throw new ValidationException($"A profile already exists with this name.");
        }

        Folder profileFolder = _profilesFolder.CreateChildFolder(name);
        Profile profile = new(profileFolder, this);
        Profiles.Add(profile);
        SortProfiles();
        return profile;
    }


    /// <summary>
    /// Reloads the game's profiles from the filesystem.<br/>
    /// If the profiles directory no longer exists, sets it to null and throws a <see cref="FilesystemMismatchException"/>.
    /// </summary>
    /// <exception cref="FilesystemException"></exception>
    /// <exception cref="FilesystemMismatchException"></exception>
    public void RefreshProfiles()
    {
        if (_profilesFolder == null || ProfilesDirectory == null)
        {
            Profiles = [];
            return;
        }

        if (!_profilesFolder.Exists)
        {
            FilesystemMismatchException ex = new(ProfilesDirectory, "The profiles directory no longer exists.");
            ProfilesDirectory = null;
            throw ex;
        }

        string? oldActiveProfileName = ActiveProfile?.Name;
        _profilesFolder.LoadChildren();
        Profiles = [.._profilesFolder.Children.OfType<Folder>().Select(x => new Profile(x, this))];
        TrySetActiveProfileByName(oldActiveProfileName);
    }


    /// <summary>
    /// If the game has a profile with the name provided, it is set as the active profile.
    /// </summary>
    /// <param name="name"></param>
    public void TrySetActiveProfileByName(string? name)
    {
        Profile? match = Profiles.FirstOrDefault(x => x.Name == name);
        if (match != null)
        {
            ActiveProfile = match;
        }
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


    public override string ToString()
    {
        return Name;
    }
}
