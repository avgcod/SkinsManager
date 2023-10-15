using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Services;
using SkinManager.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewGameMessage>
    {
        #region ReadOnly Variables
        private readonly string _directoriesFile;
        private readonly string _appliedSkinsFile;
        private readonly ISkinsAccessService _skinsAccessService;
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
        #endregion

        #region Properties
        public string SelectedSkinLocation => Skins.SingleOrDefault(x => x.Name == SelectedSkinName)?.Location ?? string.Empty;
        public string AppliedSkinName => AppliedSkins.SingleOrDefault(x => x.SkinType.Name == SelectedSkinType
        && x.SkinType.SubTypes.Contains(SelectedSkinSubType))?.SkinType.Name ?? "None";
        
        public string BackUpLocation => Path.Combine(SkinsLocation, "Originals", SelectedSkinType, SelectedSkinSubType);
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
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
        public string _selectedSkinSubType = string.Empty;
        
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(SkinsLocation))]
        [NotifyPropertyChangedFor(nameof(InstallLocation))]
        [NotifyPropertyChangedFor(nameof(GameExecutableLocation))]
        public GameInfo _selectedGame = new GameInfo();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        public bool _structureCreated = false;
        
        [ObservableProperty]
        public bool _busy = false;
        #endregion

        public MainWindowViewModel(Window currentWindow, string directoriesFile, string appliedSkinsFile, ISkinsAccessService skinService, IMessenger theMessenger)
        {
            _directoriesFile = directoriesFile;
            _appliedSkinsFile = appliedSkinsFile;
            _skinsAccessService = skinService;
            _currentWindow = currentWindow;
            _theMessenger = theMessenger;

            this.PropertyChanged += SkinManagerViewModel_PropertyChanged;
            _currentWindow.Loaded += WindowLoaded;
            _theMessenger.Register<NewGameMessage>(this);

        }

        private async void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            await LoadGameInfoAsync();
        }

        private async void SkinManagerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGame))
            {
                await GameChanged();
            }
        }

        private async Task GameChanged()
        {
            if (SelectedGame is not null)
            {
                if (!SelectedGame.GameName.Equals("New"))
                {
                    SkinsLocation = SelectedGame.SkinsLocation;
                    InstallLocation = SelectedGame.GameLocation;
                    GameExecutableLocation = SelectedGame.GameExecutable;

                    await LoadSkinsAsync();
                    await LoadAppliedSkinsAsync();                    
                }
                else if (SelectedGame.GameName.Equals("New"))
                {
                    AddGameView addGameView = new AddGameView();
                    addGameView.DataContext = new AddGameViewModel(addGameView, _theMessenger);
                    await addGameView.ShowDialog(_currentWindow);
                }
            }

        }

        #region LoadMethods
        private async Task LoadGameInfoAsync()
        {
            GamesList = new ObservableCollection<GameInfo>(await _skinsAccessService.GetGameInfoAsync(_directoriesFile));
            if (GamesList.Count == 0)
            {
                GamesList.Add(new GameInfo()
                {
                    GameName = "New"
                });
            }
        }
        private async Task LoadAppliedSkinsAsync()
        {
            int i = GamesList.IndexOf(GamesList.Single(x => x.GameName == SelectedGame.GameName));
            GamesList[i].AppliedSkins = new List<string>(await _skinsAccessService.GetAppliedSkinsAsync(Path.Combine(SelectedGame.GameName, _appliedSkinsFile)));
        }
        public async Task LoadSkinsAsync()
        {
            Skins = new ObservableCollection<Skin>(await _skinsAccessService.GetAvailableSkinsAsync(SkinsLocation));
            if(Skins.Any())
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

        public async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _theMessenger.Unregister<NewGameMessage>(this);
            if (GamesList.Count > 1)
            {
                await _skinsAccessService.SaveGameInfoAsync(GamesList, _directoriesFile);
                for (int i = 0; i < GamesList.Count; i++)
                {
                    if(GamesList[i].AppliedSkins.Any())
                    {
                        await _skinsAccessService.SaveAppliedSkinsAsync(GamesList[i].AppliedSkins, Path.Combine(SelectedGame.GameName, _appliedSkinsFile));
                    }                    
                }
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

            await _skinsAccessService.StartGameAsync(GameExecutableLocation);

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

            StructureCreated = await _skinsAccessService.CreateStructureAsync(Skins.Select(x => x.SkinType).Distinct(), SkinsLocation);

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
                GamesList[gameIndex] = SelectedGame;
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
                    GamesList[gameIndex] = SelectedGame;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBackup))]
        public async Task CreateBackup()
        {
            Busy = true;

            await _skinsAccessService.CreateBackUpAsync(SelectedSkinLocation, BackUpLocation, InstallLocation);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        public async Task ApplySkin()
        {
            Busy = true;

            await _skinsAccessService.ApplySkinAsync(SelectedSkinLocation, InstallLocation);
            AddAppliedSkin(SelectedSkinName);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRestore))]
        public async Task Restore()
        {
            Busy = true;

            await _skinsAccessService.RestoreBackupAsync(BackUpLocation, InstallLocation);

            Busy = false;
        }
        #endregion

        public void Receive(NewGameMessage message)
        {
            AddNewGame(message);
        }

        private void AddNewGame(NewGameMessage message)
        {
            Busy = true;

            GamesList.Add(new GameInfo()
            {
                GameName = message.newGameName
            });
            SelectedGame = GamesList[GamesList.Count - 1];

            Busy = false;
        }

        private int GetSelectedGameIndex()
        {
            return GamesList.IndexOf(GamesList.Single(x => x.GameName == SelectedGame.GameName));
        }

    }
}