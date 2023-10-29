using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using SkinManager.ViewModels;
using SkinManager.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace SkinManager
{
    public partial class App : Application
    {
        private const string _gameInfoFile = "GameInfo.json";
        private const string _appliedSkinsFile = "AppliedSkins.json";
        private const string _knownGamesFile = "KnownGames.json";


        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow, _gameInfoFile,
                    _appliedSkinsFile, _knownGamesFile, StrongReferenceMessenger.Default);
                desktop.MainWindow.Closing += ((MainWindowViewModel)desktop.MainWindow.DataContext).OnWindowClosing;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}