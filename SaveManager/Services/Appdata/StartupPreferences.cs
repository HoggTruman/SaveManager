namespace SaveManager.Services.Appdata;


public class StartupPreferences
{
    public string? ActiveGame { get; set; } = null;
    public string? ActiveProfile { get; set; } = null;
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
}
