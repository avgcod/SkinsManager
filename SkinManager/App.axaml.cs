using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using SkinManager.ViewModels;
using SkinManager.Views;

namespace SkinManager{
    public partial class App : Application{
        //private const string _gameInfoFile = "GameInfo.json";
        //private const string _appliedSkinsFile = "AppliedSkins.json";
        //private const string _knownGamesFile = "KnownGames.json";

        public override void Initialize(){
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted(){

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop){
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);

                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = new MainWindowViewModel((MainWindow)desktop.MainWindow);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}