using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using SkinManager.ViewModels;
using SkinManager.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using Avalonia.Controls;
using System;
using SkinManager.Services;
using System.Net.Http;

namespace SkinManager
{
    public partial class App : Application
    {
        //private const string _gameInfoFile = "GameInfo.json";
        //private const string _appliedSkinsFile = "AppliedSkins.json";
        //private const string _knownGamesFile = "KnownGames.json";
        private IHost? host;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            host = CreateHostBuilder([]).Build();
            host.Start();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);

                desktop.MainWindow = host.Services.GetRequiredService<MainWindow>();
                desktop.MainWindow.DataContext = host.Services.GetRequiredService<MainWindowViewModel>();
                desktop.Exit += Desktop_Exit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            if(host is not null)
            {
                await host.StopAsync();
                host.Dispose();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<Locations>(new Locations("GameInfo.xml", "KnownGames.xml"));

            services.AddSingleton<HttpClient>(new HttpClient());

            services.AddSingleton<FileAccessService>();
            services.AddSingleton<IFileAccessService, FileAccessService>(provider => provider.GetRequiredService<FileAccessService>());

            services.AddSingleton<ISkinsAccessService, SkinsAccessService>();

            services.AddSingleton<LocalSkinsAccessService>();

            services.AddSingleton<PSOUniversePSWebAccessService>();

            services.AddSingleton<HttpClient>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<StrongReferenceMessenger>();
            services.AddSingleton<IMessenger, StrongReferenceMessenger>(provider => provider.GetRequiredService<StrongReferenceMessenger>());

            services.AddScoped<AddGameView>();
            services.AddScoped<AddGameViewModel>();

            services.AddScoped<MessageBoxView>();
            services.AddScoped<MessageBoxViewModel>();

            services.AddScoped<ErrorMessageBoxView>();
            services.AddScoped<ErrorMessageBoxViewModel>();
        });
    }
}