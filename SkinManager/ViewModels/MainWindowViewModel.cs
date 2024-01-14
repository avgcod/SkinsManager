using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using SkinManager.Services;
using SkinManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NewGameMessage>, IRecipient<OperationErrorMessage>, IRecipient<DirectoryNotEmptyMessage>
    {
        #region Variables
        private readonly Locations _locations;
        private readonly ISkinsAccessService _skinsAccessService;
        private readonly IFileAccessService _fileAccessService;
        private readonly Window _currentWindow;
        private readonly IServiceScopeFactory _scopeFactory;
        private const string readingLocalSkinsText = "Refreshing local skins. Please wait.";
        private const string readingWebSkinsText = "Fetching skins from the website. Please wait.";
        #endregion

        #region Collections
        private List<Skin> _skins = [];
        public ObservableCollection<string> SkinTypeNames { get; set; } = [];
        public ObservableCollection<string> SkinSubTypes { get; set; } = [];
        public ObservableCollection<string> AvailableSkinNames { get; set; } = [];
        public ObservableCollection<string> OriginalSkinNames { get; set; } = [];
        private ObservableCollection<Skin> AppliedSkins { get; set; } = [];
        private ObservableCollection<string> GamesList { get; set; } = [];
        #endregion

        #region Properties
        [ObservableProperty]
        private string _appySkinButtonText = "Apply Skin";
        [ObservableProperty]
        private string _processingText = "Processing. Please wait.";
        [ObservableProperty]
        private string _skinsLocation = string.Empty;
        [ObservableProperty]
        private string _gameExecutableLocation = string.Empty;
        [ObservableProperty]
        private string _gameLocation = string.Empty;
        public bool GameIsKnown => SelectedGameIsKnown();
        [ObservableProperty]
        private SkinsSource _selectedSource = SkinsSource.Local;
        public string SelectedSkinLocation => _skins.SingleOrDefault(x => x.Name == SelectedSkinName)?.Location ?? string.Empty;
        [ObservableProperty]
        private string _appliedSkinName = string.Empty;
        public string BackUpLocation => Path.Combine(SkinsLocation, SelectedSkinTypeName, "Originals", SelectedSkinSubType);

        public Bitmap? Screenshot1 { get; private set; }
        public Bitmap? Screenshot2 { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(Screenshot1))]
        [NotifyPropertyChangedFor(nameof(Screenshot2))]
        private string _selectedSkinName = string.Empty;

        [ObservableProperty]
        private string _selectedSkinTypeName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        private string _selectedSkinSubType = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(GameIsKnown))]
        private string _selectedGameName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        private bool _structureCreated = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        private bool _busy = false;

        private bool _webSkinSelected = false;
        #endregion

        public MainWindowViewModel(IServiceScopeFactory scopeFactory, MainWindow currentWindow, Locations locations,
            ISkinsAccessService skinsAccessService, IFileAccessService fileAccessService, IMessenger theMessenger) : base(theMessenger)
        {
            _scopeFactory = scopeFactory;

            _locations = locations;

            _currentWindow = currentWindow;
            _fileAccessService = fileAccessService;
            _skinsAccessService = skinsAccessService;

            PropertyChanged += SkinManagerViewModel_PropertyChanged;

            _currentWindow.Closing += OnWindowClosing;
            _currentWindow.Loaded += WindowLoaded;

            Messenger.RegisterAll(this);
        }

        private void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            _skinsAccessService.LoadGamesInformation();
            GamesList = new(_skinsAccessService.GetGameNames());
            if (GamesList.Count == 1)
            {
                SelectedGameName = GamesList[0];
            }
        }

        public void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            Messenger.UnregisterAll(this);
            _currentWindow.Closing -= OnWindowClosing;
            _currentWindow.Loaded -= WindowLoaded;

            SaveInformation();
        }

        private void SaveInformation()
        {
            if (GamesList.Count > 0)
            {
                _skinsAccessService.SaveGamesInformation();
            }
        }
        private async void SkinManagerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedGameName))
            {
                await HandleSelectedGameNameChanged();
            }
            else if (e.PropertyName == nameof(SelectedSource))
            {
                await SetSkinAccessService();
            }
            else if (e.PropertyName == nameof(SelectedSkinTypeName))
            {
                await HandleSkinTypeChanged();
            }
            else if (e.PropertyName == nameof(SelectedSkinSubType))
            {
                HandleSkinSubTypeChanged();
            }
            else if (e.PropertyName == nameof(SelectedSkinName))
            {
                await HandleSkinNameChanged();
            }
        }

        private async Task SetScreenshots()
        {
            Screenshot1 = null;
            Screenshot2 = null;

            List<string> screenshots = _skins.Single(x => x.Name == SelectedSkinName).Screenshots;
            if (screenshots.Count == 1)
            {
                if (screenshots[0].Contains("http"))
                {
                    Screenshot1 = await ImageHelperService.LoadFromWeb(new Uri(screenshots[0]));
                }
                else
                {
                    Screenshot1 = ImageHelperService.LoadFromResource(new Uri(screenshots[0]));
                }
            }
            else if(screenshots.Count > 1)
            {
                if (screenshots[0].Contains("http"))
                {
                    Screenshot1 = await ImageHelperService.LoadFromWeb(new Uri(screenshots[0]));
                }
                else
                {
                    Screenshot1 = ImageHelperService.LoadFromResource(new Uri(screenshots[0]));
                }

                if (screenshots[1].Contains("http"))
                {
                    Screenshot2 = await ImageHelperService.LoadFromWeb(new Uri(screenshots[1]));
                }
                else
                {
                    Screenshot2 = ImageHelperService.LoadFromResource(new Uri(screenshots[1]));
                }
            }

            OnPropertyChanged(nameof(Screenshot1));
            OnPropertyChanged(nameof(Screenshot2));
        }
        private async Task HandleSkinNameChanged()
        {
            Skin? currentSkin = _skins.SingleOrDefault(x => x.Name == SelectedSkinName);
            if (currentSkin is not null)
            {
                if (currentSkin.IsWebSkin)
                {
                    AppySkinButtonText = "Download Skin";
                    _webSkinSelected = true;
                }
                else
                {
                    AppySkinButtonText = "Apply Skin";
                    _webSkinSelected = false;
                }

                await SetScreenshots();
            }
        }

        private async Task HandleSkinTypeChanged()
        {
            if (SelectedSource == SkinsSource.Web)
            {
                await LoadSkinsAsync();
            }

            SkinSubTypes.Clear();
            foreach (string currentSubType in _skins.Where(x => x.SkinType.Name == SelectedSkinTypeName).Select(x => x.SubType).Distinct())
            {
                SkinSubTypes.Add(currentSubType);
            }
            SelectedSkinSubType = SkinSubTypes[0];

            RefreshAvailableSkinNames();
        }

        private void HandleSkinSubTypeChanged()
        {
            if (string.IsNullOrEmpty(SelectedSkinSubType))
            {
                SelectedSkinSubType = string.Empty;
            }

            RefreshAvailableSkinNames();

            AppliedSkinName = GetAppliedSkinNameFromLocation();
        }

        private void RefreshAvailableSkinNames()
        {
            AvailableSkinNames.Clear();
            foreach (string currentSkinName in _skins.Where(x => x.SkinType.Name == SelectedSkinTypeName && x.SubType == SelectedSkinSubType).Select(x => x.Name))
            {
                AvailableSkinNames.Add(currentSkinName);
            }
        }

        private async Task HandleSelectedGameNameChanged()
        {
            if (!string.IsNullOrEmpty(SelectedGameName))
            {
                _skinsAccessService.SetCurrentGame(SelectedGameName);
                SkinsLocation = _skinsAccessService.GetSkinsLocation();
                GameLocation = _skinsAccessService.GetGameLocation();
                GameExecutableLocation = _skinsAccessService.GetGameExecutableLocation();
                await LoadSkinsAsync();
                SkinTypeNames.Clear();
                foreach (string skinTypeName in _skins.Select(x => x.SkinType).DistinctBy(x => x.Name).OrderBy(x => x.Name).Select(x => x.Name))
                {
                    SkinTypeNames.Add(skinTypeName);
                }
                SelectedSkinTypeName = SkinTypeNames[0];

                if (_currentWindow.Find<ComboBox>("skinsSourceCbx") is { } skinsSourceCbx)
                {
                    skinsSourceCbx.SelectedIndex = GamesList.IndexOf(SelectedGameName);
                }
            }
        }

        public async Task LoadSkinsAsync()
        {
            Busy = true;

            _skinsAccessService.SetSkinsSource(SelectedSource);
            IEnumerable<SkinType> skinTypes = [];

            if (SelectedSource == SkinsSource.Local)
            {
                _skins = new(await _skinsAccessService.GetAvailableSkinsAsync());
            }
            else
            {
                if (string.IsNullOrEmpty(SelectedSkinTypeName))
                {
                    skinTypes = _skinsAccessService.GetSkinTypesForWeb();
                    _skins.AddRange(await _skinsAccessService.GetWebSkinsForSpecificSkinType(skinTypes.First().Name));
                }
                else
                {
                    _skins.AddRange(await _skinsAccessService.GetWebSkinsForSpecificSkinType(SelectedSkinTypeName));
                }
            }

            if (_skins.Count != 0)
            {
                if (SelectedSource == SkinsSource.Local)
                {
                    StructureCreated = true;
                }
            }
            else if (SelectedSource == SkinsSource.Local)
            {
                StructureCreated = false;
            }

            OriginalSkinNames = new(_skinsAccessService.GetOriginalSkinNames());

            RefreshAvailableSkinNames();

            Busy = false;
        }

        public void AddAppliedSkin(string appliedSkinName)
        {
            _skinsAccessService.AddAppliedSkin(appliedSkinName);
        }

        public void RemoveAppliedSkin(string removedSkinName)
        {
            _skinsAccessService.RemoveAppliedSkin(removedSkinName);
        }

        #region Commands
        public bool CanStartGame => !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
        public bool CanReloadSkins => !string.IsNullOrEmpty(SkinsLocation) && !Busy;
        public bool CanCreateStructure => !string.IsNullOrEmpty(SkinsLocation) && !StructureCreated && !Busy;
        public bool CanBackup => StructureCreated && !string.IsNullOrEmpty(SkinsLocation)
                && !string.IsNullOrEmpty(GameLocation)
                && !string.IsNullOrEmpty(SelectedSkinName) && !Busy;
        public bool CanBrowse => !string.IsNullOrEmpty(SelectedGameName) && !string.Equals(SelectedGameName, "New") && !Busy;
        public bool CanApply => !string.IsNullOrEmpty(SkinsLocation) && !string.IsNullOrEmpty(GameLocation) && !Busy;
        public bool CanRestore => Directory.Exists(BackUpLocation) && !Busy;

        [RelayCommand]
        public async Task AddNewGame()
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            AddGameView addGameView = serviceScope.ServiceProvider.GetRequiredService<AddGameView>();
            addGameView.DataContext = serviceScope.ServiceProvider.GetRequiredService<AddGameViewModel>();
            await addGameView.ShowDialog(_currentWindow);
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        public async Task ApplySkin()
        {
            Busy = true;

            if (SelectedSource == SkinsSource.Local && !_webSkinSelected)
            {
                ProcessingText = "Applying skin. Please wait.";
                await _fileAccessService.ApplySkinAsync(SelectedSkinLocation, GameLocation);
                AddAppliedSkin(SelectedSkinName);
            }
            else
            {
                ProcessingText = "Downloading skin. Please wait.";
                Skin skinToDownload = _skins.SingleOrDefault(x => x.Name == SelectedSkinName) ?? new();
                if (await _skinsAccessService.DownloadSkin(skinToDownload, SkinsLocation))
                {
                    using (IServiceScope serviceScope = _scopeFactory.CreateScope())
                    {
                        MessageBoxView mbView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
                        mbView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
                        Messenger.Send(new MessageBoxMessage($"Skin download to {SkinsLocation}. " +
                            $"{Environment.NewLine} Please put the skin in the appropriate area, switch back to local and click reload skins."));
                        await mbView.ShowDialog(_currentWindow);
                    }

                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseExecutable()
        {
            Busy = true;

            ProcessingText = "Waiting for the game executable to be selected. Please wait.";

            IReadOnlyList<IStorageFile> location = await _currentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose Game Executable",
                AllowMultiple = false
            });

            if (location.Any())
            {
                if (location[0].CanBookmark)
                {
                    GameExecutableLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBackup))]
        public async Task CreateBackup()
        {
            Busy = true;

            ProcessingText = "Creating backup. Please wait.";
            await _fileAccessService.CreateBackUpAsync(SelectedSkinLocation, BackUpLocation, GameLocation);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseFolder(object? parameter)
        {
            Busy = true;

            ProcessingText = "Waiting for a folder to be selected. Please wait.";

            string titleText = parameter?.ToString() == "Skins" ? "Choose Skins Location" : "Choose Game Location";

            IReadOnlyList<IStorageFolder> location = await _currentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = titleText,
                AllowMultiple = false
            });

            if (location.Any())
            {
                if (parameter?.ToString() == "Skins" && location[0].CanBookmark)
                {
                    SkinsLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
                else
                {
                    GameLocation = await location[0].SaveBookmarkAsync() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanCreateStructure))]
        public async Task CreateStructureAsync()
        {
            Busy = true;

            ProcessingText = "Creating folder structure. Please wait.";
            await _fileAccessService.CreateStructureAsync(_skins.Select(x => x.SkinType).DistinctBy(x => x.Name), SkinsLocation);

            StructureCreated = true;

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanReloadSkins))]
        public async Task ReloadSkinsAsync()
        {
            Busy = true;

            ProcessingText = "Reloading skins. Please wait.";
            await LoadSkinsAsync();

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRestore))]
        public async Task Restore()
        {
            Busy = true;

            ProcessingText = "Restoring from backup. Please wait.";

            if (await _fileAccessService.RestoreBackupAsync(BackUpLocation, GameLocation))
            {
                RemoveAppliedSkin(SelectedSkinName);
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanStartGame))]
        public async Task StartGameAsync()
        {
            Busy = true;

            ProcessingText = "Starting the game. Please wait.";
            await _fileAccessService.StartGameAsync(GameExecutableLocation);

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

        private async Task HandleDirectoryNotEmptyMessageAsync(DirectoryNotEmptyMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            MessageBoxView mboxView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
            mboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
            Messenger.Send<MessageBoxMessage>(new(message.DirectoryPath + " is not empty." + Environment.NewLine + "Please select an empty directory."));
            await mboxView.ShowDialog(_currentWindow);
        }
        private void HandleNewGameMessage(NewGameMessage message)
        {
            Busy = true;

            if (GamesList.Contains(message.NewGameName))
            {
                Messenger.Send<MessageBoxMessage>(new($"Game {message.NewGameName} is already in the list."));
            }
            else
            {
                _skinsAccessService.AddGame(new GameInfo() { GameName = message.NewGameName });
                GamesList.Add(message.NewGameName);
                SelectedGameName = GamesList[^1];
            }

            Busy = false;
        }
        private async Task HandleOperationErrorMessageAsync(OperationErrorMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            Messenger.Send<ErrorMessage>(new(message.ErrorType, message.ErrorText));
            await emboxView.ShowDialog(_currentWindow);
        }
        #endregion

        private async Task SetSkinAccessService()
        {
            if (SelectedSource == SkinsSource.Local)
            {
                ProcessingText = "Refreshing local skins. Please wait.";
            }
            else
            {
                _skins.Clear();
                ProcessingText = "Fetching skins from the website. Please wait.";
            }
            await LoadSkinsAsync();
        }

        /// <summary>
        /// Gets the name of the applied skin from the skin location.
        /// </summary>
        /// <returns>name of the applied skin or none if none are found.</returns>
        private string GetAppliedSkinNameFromLocation()
        {
            return _skinsAccessService.GetAppliedSkinNameFromLocation(SelectedSkinTypeName, SelectedSkinSubType);
        }

        private bool SelectedGameIsKnown()
        {
            return _skinsAccessService.SelectedGameIsKnown(SelectedGameName);
        }
    }
}