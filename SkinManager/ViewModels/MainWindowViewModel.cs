using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Services;
using SkinManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewGameMessage>, IRecipient<OperationErrorMessage>, IRecipient<DirectoryNotEmptyMessage>
    {
        #region Variables
        private readonly string _directoriesFile;
        private readonly string _appliedSkinsFile;
        private readonly string _knownGamesFile;
        private ISkinsAccessService _skinsAccessService;
        private readonly Window _currentWindow;
        private readonly IMessenger _theMessenger;
        #endregion

        #region Collections
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SkinTypeNames))]
        private ObservableCollection<Skin> _skins = new ObservableCollection<Skin>();

        public IEnumerable<string> SkinTypeNames => Skins.Select(x => x.SkinType).Select(x => x.Name).Distinct();

        public IEnumerable<string> SkinSubTypeNames => Skins.Where(x => x.SkinType.Name == SelectedSkinType).Select(x => x.SubType).Distinct();

        public IEnumerable<string> AvailableSkinNames => Skins.Where(x => x.SkinType.Name == SelectedSkinType && x.SubType == SelectedSkinSubType).Select(x => x.Name);

        [ObservableProperty]
        private ObservableCollection<Skin> _appliedSkins = new ObservableCollection<Skin>();

        public IEnumerable<string> OriginalSkinNames => Skins.Where(x => x.IsOriginal == true).Select(x => x.Name);

        [ObservableProperty]
        private ObservableCollection<GameInfo> _gamesList = new ObservableCollection<GameInfo>();

        [ObservableProperty]
        private ObservableCollection<KnownGameInfo> _knownGamesList = new ObservableCollection<KnownGameInfo>();

        public IEnumerable<string> SkinsSourceList => new List<string>()
        {
            "Local",
            KnownGamesList.SingleOrDefault(x => x.GameName == SelectedGame.GameName)?.SkinsSiteName ?? string.Empty,
        };
        #endregion

        #region Properties
        public bool GameIsKnown => KnownGamesList.Where(x => x.GameName == SelectedGame.GameName).Any();
        [ObservableProperty]
        private string _selectedSource = string.Empty;
        public string SelectedSkinLocation => Skins.SingleOrDefault(x => x.Name == SelectedSkinName)?.Location ?? string.Empty;
        public string AppliedSkinName => GetAppliedSkinNameFromLocation();

        public string BackUpLocation => Path.Combine(SkinsLocation, "Originals", SelectedSkinType, SelectedSkinSubType);
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        public string _skinsLocation = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public string _installLocation = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        public string _gameExecutableLocation = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public string _selectedSkinName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SkinSubTypeNames))]
        public string _selectedSkinType = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
        [NotifyPropertyChangedFor(nameof(AppliedSkinName))]
        public string _selectedSkinSubType = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(GameIsKnown))]
        [NotifyPropertyChangedFor(nameof(SkinsSourceList))]
        public GameInfo _selectedGame = new GameInfo();

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

        public MainWindowViewModel(Window currentWindow, string directoriesFile, string appliedSkinsFile, string knownGamesFile, IMessenger theMessenger)
        {
            _directoriesFile = directoriesFile;
            _appliedSkinsFile = appliedSkinsFile;
            _knownGamesFile = knownGamesFile;
            _currentWindow = currentWindow;
            _theMessenger = theMessenger;
            _skinsAccessService = new LocalSkinsAccessService();

            this.PropertyChanged += SkinManagerViewModel_PropertyChanged;
            _currentWindow.Loaded += WindowLoaded;
            _theMessenger.Register<NewGameMessage>(this);
            _theMessenger.Register<OperationErrorMessage>(this);
            _theMessenger.Register<DirectoryNotEmptyMessage>(this);

        }

        private async void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            await LoadGameInfoAsync();
        }

        public async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _theMessenger.UnregisterAll(this);

            if (GamesList.Count > 1)
            {
                SavePathInformation();
                await SettingsLoaderService.SaveGameInfoAsync(GamesList, _directoriesFile, _theMessenger);
                for (int i = 0; i < GamesList.Count; i++)
                {
                    if (GamesList[i].AppliedSkins.Any())
                    {
                        await _skinsAccessService.SaveAppliedSkinsAsync(GamesList[i].AppliedSkins, Path.Combine(SelectedGame.GameName, _appliedSkinsFile), _theMessenger);
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
            else if (e.PropertyName == nameof(SelectedSource) && !string.IsNullOrEmpty(SelectedSource))
            {
                SetSkinAccessService();
            }
        }

        private async Task GameChanged()
        {
            if (SelectedGame is not null)
            {
                if (!SelectedGame.GameName.Equals("New"))
                {
                    UpdateLocationsInfo();
                    SavePathInformation();

                    ComboBox? skinsSourcecbx = _currentWindow.Find<ComboBox>("skinsSourceCbx");
                    if (skinsSourcecbx != null)
                    {
                        skinsSourcecbx.SelectedIndex = 0;
                    }

                    await LoadSkinsAsync();
                    await LoadAppliedSkinsAsync();
                }
                else if (SelectedGame.GameName.Equals("New"))
                {
                    await ShowAddNewGameDialog();
                }
            }

        }

        private void SavePathInformation()
        {
            int gameIndex = GetSelectedGameIndex();
            GamesList[gameIndex].SkinsLocation = SkinsLocation;
            GamesList[gameIndex].GameLocation = InstallLocation;
            GamesList[gameIndex].GameExecutable = GameExecutableLocation;
        }

        private void UpdateLocationsInfo()
        {
            int gameIndex = GetSelectedGameIndex();
            SkinsLocation = GamesList[gameIndex].SkinsLocation;
            InstallLocation = GamesList[gameIndex].GameLocation;
            GameExecutableLocation = GamesList[gameIndex].GameExecutable;
        }

        private async Task ShowAddNewGameDialog()
        {
            AddGameView addGameView = new AddGameView();
            addGameView.DataContext = new AddGameViewModel(addGameView, _theMessenger);
            await addGameView.ShowDialog(_currentWindow);
        }

        #region LoadMethods
        private async Task LoadGameInfoAsync()
        {
            GameInfo newgameInfo = new GameInfo()
            {
                GameName = "New"
            };
            GamesList = new ObservableCollection<GameInfo>(await SettingsLoaderService.GetGameInfoAsync(_directoriesFile, _theMessenger));
            KnownGamesList = new ObservableCollection<KnownGameInfo>(await SettingsLoaderService.GetKnowGamesInfoAsync(_knownGamesFile, _theMessenger));
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
            if(GamesList.SingleOrDefault(x => x.GameName == newgameInfo.GameName) is null)
            {
                GamesList.Add(newgameInfo);
            }
            else
            {
                ComboBox? gamesListcbx = _currentWindow.Find<ComboBox>("gamesListcbx");
                if (gamesListcbx != null)
                {
                    gamesListcbx.SelectedIndex = 0;
                }
            }
        }
        private async Task LoadAppliedSkinsAsync()
        {
            int gameIndex = GetSelectedGameIndex();
            GamesList[gameIndex].AppliedSkins = new List<string>(
                await _skinsAccessService.GetAppliedSkinsAsync(Path.Combine(SelectedGame.GameName, _appliedSkinsFile), _theMessenger));
        }
        public async Task LoadSkinsAsync()
        {
            Skins = new ObservableCollection<Skin>(await _skinsAccessService.GetAvailableSkinsAsync(SkinsLocation, _theMessenger));
            if (Skins.Any())
            {
                SelectedSkinType = SkinTypeNames.First();
                SelectedSkinSubType = SkinSubTypeNames.First();

                StructureCreated = true;
            }
            else
            {
                StructureCreated = false;
            }
        }
        #endregion

        public void AddAppliedSkin(string appliedSkinName)
        {
            int gameIndex = GetSelectedGameIndex();
            string appliedSkin = Skins.Single(x => x.Name == appliedSkinName).Location;
            if (!GamesList[gameIndex].AppliedSkins.Contains(appliedSkin))
            {
                GamesList[gameIndex].AppliedSkins.Add(appliedSkin);
            }

        }


        #region Commands
        public bool CanStartGame => !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
        public bool CanReloadSkins => !string.IsNullOrEmpty(SkinsLocation) && !Busy;
        public bool CanCreateStructure => !string.IsNullOrEmpty(SkinsLocation) && !StructureCreated && !Busy;
        public bool CanBackup => StructureCreated && !string.IsNullOrEmpty(SkinsLocation)
                && !string.IsNullOrEmpty(InstallLocation)
                && !string.IsNullOrEmpty(SelectedSkinName) && !Busy;
        public bool CanBrowse => !string.IsNullOrEmpty(SelectedGame?.GameName) && !string.Equals(SelectedGame?.GameName, "New") && !Busy;
        public bool CanApply => !string.IsNullOrEmpty(SkinsLocation) && !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
        public bool CanRestore => File.Exists(BackUpLocation) && !Busy;

        [RelayCommand(CanExecute = nameof(CanStartGame))]
        public async Task StartGameAsync()
        {
            Busy = true;

            await _skinsAccessService.StartGameAsync(GameExecutableLocation, _theMessenger);

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
                IEnumerable<SkinType> knownStructure = await _skinsAccessService.GetSkinTypesAsync(SelectedGame.GameName + ".json", _theMessenger);
                await _skinsAccessService.CreateStructureAsync(knownStructure, SkinsLocation, _theMessenger);
            }
            else
            {
                await _skinsAccessService.CreateStructureAsync(Skins.Select(x => x.SkinType).Distinct(), SkinsLocation, _theMessenger);
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
                int gameIndex = GetSelectedGameIndex();
                if (parameter?.ToString() == "Skins" && location[0].CanBookmark)
                {
                    SkinsLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
                else
                {
                    InstallLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
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
                    int gameIndex = GetSelectedGameIndex();
                    GameExecutableLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBackup))]
        public async Task CreateBackup()
        {
            Busy = true;

            await _skinsAccessService.CreateBackUpAsync(SelectedSkinLocation, BackUpLocation, InstallLocation, _theMessenger);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        public async Task ApplySkin()
        {
            Busy = true;

            await _skinsAccessService.ApplySkinAsync(SelectedSkinLocation, InstallLocation, _theMessenger);
            AddAppliedSkin(SelectedSkinName);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRestore))]
        public async Task Restore()
        {
            Busy = true;

            await _skinsAccessService.RestoreBackupAsync(BackUpLocation, InstallLocation, _theMessenger);

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
                GameName = message.newGameName
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
            ErrorMessageBoxView emboxView = new ErrorMessageBoxView();

            emboxView.DataContext = new ErrorMessageBoxViewModel(emboxView, message);

            await emboxView.ShowDialog(_currentWindow);
        }

        private async Task HandleDirectoryNotEmptyMessageAsync(DirectoryNotEmptyMessage message)
        {
            MessageBoxView mboxView = new MessageBoxView();

            mboxView.DataContext = new MessageBoxViewViewModel(mboxView, message.DirectoryPath + " is not empty." + Environment.NewLine + "Please select an empty directory.");

            await mboxView.ShowDialog(_currentWindow);
        }
        #endregion

        private int GetSelectedGameIndex()
        {
            return GamesList.IndexOf(GamesList.Single(x => x.GameName == SelectedGame.GameName));
        }

        private void SetSkinAccessService()
        {
            //if (SelectedSource == "Local")
            //{
            _skinsAccessService = new LocalSkinsAccessService();
            //}
            //else
            //{
            //    if (SelectedGame.GameName == "Phantasy Star Online BB")
            //    {
            //        _skinsAccessService = new UniversePSAccessService(KnownGamesList.Single(x => x.GameName == SelectedGame.GameName).SkinsSiteAddress);

            //    }
            //}
        }

        private string GetAppliedSkinNameFromLocation()
        {
            if (!string.IsNullOrEmpty(SelectedGame.GameName))
            {
                foreach (string skinLocation in GamesList[GetSelectedGameIndex()].AppliedSkins)
                {
                    Skin? foundSkin = Skins.SingleOrDefault(x => x.Location == skinLocation);
                    if (foundSkin?.SkinType.Name == SelectedSkinType && foundSkin?.SubType == SelectedSkinSubType)
                    {
                        return foundSkin.Name;
                    }
                }
            }

            return "None";
        }

    }
}