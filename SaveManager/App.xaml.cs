using SaveManager.ViewModels;
using SaveManager.Views.Save;
using System.Windows;

namespace SaveManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);    
        SaveWindow mainWindow = new(ViewModelFactory.CreateSaveViewModel());
        mainWindow.Show();            
    }
}
