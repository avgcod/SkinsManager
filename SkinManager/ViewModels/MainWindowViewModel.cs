using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkinManager.Models;
using SkinManager.Services;
using SkinManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewGameMessage>, IRecipient<OperationErrorMessage>, IRecipient<DirectoryNotEmptyMessage>
    {
        #region Variables
        private readonly Locations _locations;
        private ISkinsAccessService _skinsAccessService;
        private readonly IFileAccessService _fileAccessService;
        private readonly Window _currentWindow;
        private readonly IMessenger _theMessenger;
        private readonly IServiceScopeFactory _scopeFactory;
        #endregion

        #region Collections
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SkinTypes))]
        private ObservableCollection<Skin> _skins = [];

        public IEnumerable<SkinType> SkinTypes => Skins.DistinctBy(x => x.SkinType.Name).Select(x => x.SkinType);

        public IEnumerable<string> AvailableSkinNames => Skins.Where(x => x.SkinType.Name == SelectedSkinType.Name && x.SubType == SelectedSkinSubType).Select(x => x.Name);

        [ObservableProperty]
        private ObservableCollection<Skin> _appliedSkins = [];

        public IEnumerable<string> OriginalSkinNames => Skins.Where(x => x.IsOriginal == true).Select(x => x.Name);

        [ObservableProperty]
        private ObservableCollection<GameInfo> _gamesList = [];

        [ObservableProperty]
        private ObservableCollection<KnownGameInfo> _knownGamesList = [];

        #endregion

        #region Properties
        public bool GameIsKnown => KnownGamesList.Where(x => x.GameName == SelectedGame.GameName).Any();
        [ObservableProperty]
        private SkinsSource _selectedSource = SkinsSource.Local;
        public string SelectedSkinLocation => Skins.SingleOrDefault(x => x.Name == SelectedSkinName)?.Location ?? string.Empty;
        public string AppliedSkinName => GetAppliedSkinNameFromLocation();
        public string BackUpLocation => Path.Combine(SelectedGame.SkinsLocation, SelectedSkinType.Name, "Originals", SelectedSkinSubType);

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public string _selectedSkinName = string.Empty;


        [ObservableProperty]
        public SkinType _selectedSkinType = new(string.Empty, new List<string>());

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
        [NotifyPropertyChangedFor(nameof(AppliedSkinName))]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        public string _selectedSkinSubType = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(GameIsKnown))]
        public GameInfo _selectedGame = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public bool _structureCreated = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public bool _busy = false;
        #endregion

        public MainWindowViewModel(IServiceScopeFactory scopeFactory, MainWindow currentWindow, Locations locations,
            ISkinsAccessService skinsAccessService, IFileAccessService fileAccessService, IMessenger theMessenger)
        {
            _scopeFactory = scopeFactory;

            _locations = locations;

            _currentWindow = currentWindow;
            _fileAccessService = fileAccessService;
            _theMessenger = theMessenger;
            _skinsAccessService = skinsAccessService;

            this.PropertyChanged += SkinManagerViewModel_PropertyChanged;

            _currentWindow.Closing += OnWindowClosing;
            _currentWindow.Loaded += WindowLoaded;

            _theMessenger.RegisterAll(this);

        }

        private async void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            await LoadGameInfoAsync();
        }

        public async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _theMessenger.UnregisterAll(this);
            _currentWindow.Closing -= OnWindowClosing;
            _currentWindow.Loaded -= WindowLoaded;

            if (GamesList.Count > 1)
            {
                using IServiceScope serviceScope = _scopeFactory.CreateScope();
                ISettingsLoaderService settingsLoaderService = serviceScope.ServiceProvider.GetRequiredService<ISettingsLoaderService>();
                await settingsLoaderService.SaveGameInfoAsync(GamesList, _locations.GameInfoFile);
                for (int i = 0; i < GamesList.Count; i++)
                {
                    if (GamesList[i].AppliedSkins.Count > 0)
                    {
                        await _fileAccessService.SaveAppliedSkinsAsync(GamesList[i].AppliedSkins, Path.Combine(SelectedGame.GameName, _locations.AppliedSkinsFile));
                    }
                }
            }
        }
        private async void SkinManagerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGame))
            {
                await GameChanged();
            }
            else if (e.PropertyName == nameof(SelectedSource))
            {
                SetSkinAccessService();
            }
        }

        private async Task GameChanged()
        {
            if (SelectedGame is not null)
            {
                await LoadSkinsAsync();
                await LoadAppliedSkinsAsync();

                if (_currentWindow.Find<ComboBox>("skinsSourceCbx") is { } skinsSourcecbx)
                {
                    skinsSourcecbx.SelectedIndex = GamesList.IndexOf(SelectedGame);
                }
            }

        }


        #region LoadMethods
        private async Task LoadGameInfoAsync()
        {
            GameInfo newgameInfo = new();
            ISettingsLoaderService settingsLoaderService;

            using (IServiceScope serviceScope = _scopeFactory.CreateScope())
            {
                settingsLoaderService = serviceScope.ServiceProvider.GetRequiredService<ISettingsLoaderService>();
            }

            GamesList = new ObservableCollection<GameInfo>(await settingsLoaderService.GetGameInfoAsync(_locations.GameInfoFile));
            KnownGamesList = new ObservableCollection<KnownGameInfo>(await settingsLoaderService.GetKnowGamesInfoAsync(_locations.KnownGamesFile));

            foreach (KnownGameInfo knownGame in KnownGamesList)
            {
                if (GamesList.SingleOrDefault(x => x.GameName == knownGame.GameName) == null)
                {
                    GamesList.Add(new GameInfo()
                    {
                        GameName = knownGame.GameName
                    });
                }
            }

            if (GamesList.SingleOrDefault(x => x.GameName == newgameInfo.GameName) is null)
            {
                GamesList.Add(newgameInfo);
            }
            else
            {
                if (_currentWindow.Find<ComboBox>("gamesListcbx") is { } gamesListcbx)
                {
                    gamesListcbx.SelectedIndex = 0;
                }
            }
        }
        private async Task LoadAppliedSkinsAsync()
        {
            SelectedGame.AppliedSkins = (await _fileAccessService.GetAppliedSkinsAsync(Path.Combine(SelectedGame.GameName, _locations.AppliedSkinsFile))).ToList();
        }
        public async Task LoadSkinsAsync()
        {
            Skins = new ObservableCollection<Skin>(await _skinsAccessService.GetAvailableSkinsAsync(SelectedGame.SkinsLocation));

            if (Skins.Any())
            {
                LoadSkinTypeSubTypes();
                SelectedSkinType = SkinTypes.First();
                if (SelectedSkinType.SubTypes.Count > 0)
                {
                    SelectedSkinSubType = SelectedSkinType.SubTypes.First();
                }

                StructureCreated = true;
            }
            else
            {
                StructureCreated = false;
            }
        }
        private void LoadSkinTypeSubTypes()
        {
            foreach (SkinType currentSkinType in SkinTypes)
            {
                currentSkinType.SubTypes = Skins.Where(x => x.SkinType.Name == currentSkinType.Name).Select(x => x.SubType).Distinct().ToList();
            }
        }
        #endregion

        public void AddAppliedSkin(string appliedSkinName)
        {
            if (!SelectedGame.AppliedSkins.Contains(appliedSkinName))
            {
                SelectedGame.AppliedSkins.Add(appliedSkinName);
            }
        }

        public void RemoveAppliedSkin(string removedSkinName)
        {
            SelectedGame.AppliedSkins.Remove(removedSkinName);
        }

        #region Commands
        public bool CanStartGame => !string.IsNullOrEmpty(SelectedGame.GameExecutable) && !Busy;
        public bool CanReloadSkins => !string.IsNullOrEmpty(SelectedGame.SkinsLocation) && !Busy;
        public bool CanCreateStructure => !string.IsNullOrEmpty(SelectedGame.SkinsLocation) && !StructureCreated && !Busy;
        public bool CanBackup => StructureCreated && !string.IsNullOrEmpty(SelectedGame.SkinsLocation)
                && !string.IsNullOrEmpty(SelectedGame.GameLocation)
                && !string.IsNullOrEmpty(SelectedSkinName) && !Busy;
        public bool CanBrowse => !string.IsNullOrEmpty(SelectedGame?.GameName) && !string.Equals(SelectedGame?.GameName, "New") && !Busy;
        public bool CanApply => !string.IsNullOrEmpty(SelectedGame.SkinsLocation) && !string.IsNullOrEmpty(SelectedGame.GameExecutable) && !Busy;
        public bool CanRestore => Directory.Exists(BackUpLocation) && !Busy;

        [RelayCommand]
        public async Task AddNewGame()
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            AddGameView addGameView = serviceScope.ServiceProvider.GetRequiredService<AddGameView>();
            addGameView.DataContext = serviceScope.ServiceProvider.GetRequiredService<AddGameViewModel>();
            await addGameView.ShowDialog(_currentWindow);
        }

        [RelayCommand(CanExecute = nameof(CanStartGame))]
        public async Task StartGameAsync()
        {
            Busy = true;

            await _fileAccessService.StartGameAsync(SelectedGame.GameExecutable);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanReloadSkins))]
        public async Task ReloadSkinsAsync()
        {
            Busy = true;

            await LoadSkinsAsync();

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanCreateStructure))]
        public async Task CreateStructureAsync()
        {
            Busy = true;

            if (KnownGamesList.SingleOrDefault(x => x.GameName == SelectedGame.GameName) != null)
            {
                IEnumerable<SkinType> knownStructure = await _fileAccessService.GetSkinTypesAsync(SelectedGame.GameName + ".json");
                await _fileAccessService.CreateStructureAsync(knownStructure, SelectedGame.SkinsLocation);
            }
            else
            {
                await _fileAccessService.CreateStructureAsync(Skins.Select(x => x.SkinType).Distinct(), SelectedGame.SkinsLocation);
            }

            StructureCreated = true;

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseFolder(object? parameter)
        {
            Busy = true;

            string titleText = string.Empty;

            if (parameter?.ToString() == "Skins")
            {
                titleText = "Choose Skins Location";
            }
            else
            {
                titleText = "Choose Game Location";
            }

            IReadOnlyList<IStorageFolder> location = await _currentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = titleText,
                AllowMultiple = false
            });

            if (location.Any())
            {
                if (parameter?.ToString() == "Skins" && location[0].CanBookmark)
                {
                    SelectedGame.SkinsLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
                else
                {
                    SelectedGame.GameLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseExecutable()
        {
            Busy = true;

            IReadOnlyList<IStorageFile> location = await _currentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose Game Executable",
                AllowMultiple = false
            });

            if (location.Any())
            {
                if (location[0].CanBookmark)
                {
                    SelectedGame.GameExecutable = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBackup))]
        public async Task CreateBackup()
        {
            Busy = true;

            await _fileAccessService.CreateBackUpAsync(SelectedSkinLocation, BackUpLocation, SelectedGame.GameLocation);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        public async Task ApplySkin()
        {
            Busy = true;

            await _fileAccessService.ApplySkinAsync(SelectedSkinLocation, SelectedGame.GameLocation);
            AddAppliedSkin(SelectedSkinName);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRestore))]
        public async Task Restore()
        {
            Busy = true;

            await _fileAccessService.RestoreBackupAsync(BackUpLocation, SelectedGame.GameLocation);
            RemoveAppliedSkin(SelectedSkinName);

            Busy = false;
        }
        #endregion

        #region Message Handling
        public void Receive(NewGameMessage message)
        {
            HandleNewGameMessage(message);
        }

        public async void Receive(OperationErrorMessage message)
        {
            await HandleOperationErrorMessageAsync(message);
        }

        public async void Receive(DirectoryNotEmptyMessage message)
        {
            await HandleDirectoryNotEmptyMessageAsync(message);
        }

        private void HandleNewGameMessage(NewGameMessage message)
        {
            Busy = true;

            GamesList.Add(new GameInfo()
            {
                GameName = message.NewGameName
            });

            SelectedGame = GamesList[GamesList.Count - 1];

            ComboBox? gamesListcbx = _currentWindow.Find<ComboBox>("gamesListcbx");
            if (gamesListcbx != null)
            {
                gamesListcbx.SelectedIndex = GamesList.Count - 1;
            }

            Busy = false;
        }

        private async Task HandleOperationErrorMessageAsync(OperationErrorMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            _theMessenger.Send<ErrorMessage>(new(message.ErrorType, message.ErrorText));
            await emboxView.ShowDialog(_currentWindow);

        }

        private async Task HandleDirectoryNotEmptyMessageAsync(DirectoryNotEmptyMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            MessageBoxView mboxView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
            mboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
            _theMessenger.Send<MessageBoxMessage>(new(message.DirectoryPath + " is not empty." + Environment.NewLine + "Please select an empty directory."));
            await mboxView.ShowDialog(_currentWindow);
        }

        #endregion


        private void SetSkinAccessService()
        {
            if (SelectedSource == SkinsSource.Local)
            {
                using IServiceScope serviceScope = _scopeFactory.CreateScope();
                _skinsAccessService = serviceScope.ServiceProvider.GetRequiredService<ILocalSkinsAccessServiceFactory>().Create();

            }
            else
            {
                using IServiceScope serviceScope = _scopeFactory.CreateScope();
                _skinsAccessService = serviceScope.ServiceProvider.GetRequiredService<IWebSkinsAccessServiceFactory>().Create();
            }
        }

        /// <summary>
        /// Gets the name of the applied skin from the skin location.
        /// </summary>
        /// <returns>name of the applied skin or none if none are found.</returns>
        private string GetAppliedSkinNameFromLocation()
        {
            if (!string.IsNullOrEmpty(SelectedGame.GameName))
            {
                foreach (string skinLocation in SelectedGame.AppliedSkins)
                {
                    Skin? foundSkin = Skins.SingleOrDefault(x => x.Location == skinLocation);
                    if (foundSkin?.SkinType.Name == SelectedSkinType.Name && foundSkin?.SubType == SelectedSkinSubType)
                    {
                        return foundSkin.Name;
                    }
                }
            }

            return "None";
        }

    }
}