namespace SaveManager.Services.Appdata;


public class StartupPreferences
{
    public string? SelectedGame { get; set; } = null;
    public string? SelectedProfile { get; set; } = null;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
}
