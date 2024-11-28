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
using System.Linq;
using System.Threading.Tasks;
using SkinManager.Extensions;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<OperationErrorMessage>
        , IRecipient<DirectoryNotEmptyMessage>, IRecipient<FatalErrorMessage>, IRecipient<ConfirmationResponse>
    {
        #region Variables
        private readonly SkinsAccessService _skinsAccessService;
        private readonly Window _currentWindow;
        private readonly IServiceScopeFactory _scopeFactory;
        private bool _cleanShutdown = true;
        private bool _applyNoBackup = false;
        #endregion

        #region Collections
        private List<Skin> _skins = [];
        public ObservableCollection<string> SkinTypeNames { get; set; } = [];
        public ObservableCollection<string> SkinSubTypes { get; set; } = [];
        public ObservableCollection<string> AvailableSkinNames { get; set; } = [];

        public IEnumerable<string> WebSources
            => Enum.GetNames<SkinsSource>()
                .Where(x => !string.Equals(x, SkinsSource.Local.ToString(), StringComparison.OrdinalIgnoreCase));
        #endregion

        #region Properties
        [ObservableProperty] private string _processingText = "Processing. Please wait.";
        [ObservableProperty] private string _skinsLocation = string.Empty;
        [ObservableProperty] private string _gameExecutableLocation = string.Empty;
        [ObservableProperty] private string _gameLocation = string.Empty;
        [ObservableProperty] private bool _isEphinea = true;
        [ObservableProperty] private bool _includeWeb = false;
        [ObservableProperty] private bool _showRestore = false;

        [ObservableProperty] private SkinsSource _selectedSource;

        private bool WebSkinSelected => _skins.First(x => x.Name == SelectedSkinName).IsWebSkin();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        private string _appliedSkinName = string.Empty;

        public Bitmap? Screenshot1 { get; private set; }
        public Bitmap? Screenshot2 { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
        [NotifyPropertyChangedFor(nameof(Screenshot1))]
        [NotifyPropertyChangedFor(nameof(Screenshot2))]
        [NotifyPropertyChangedFor(nameof(WebSkinSelected))]
        private string _selectedSkinName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
        private string _selectedSkinTypeName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        private string _selectedSkinSubType = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RefreshSkinsCommand))]
        private bool _structureCreated = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RefreshSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        private bool _busy = false;
        #endregion

        public MainWindowViewModel(IServiceScopeFactory scopeFactory, MainWindow currentWindow,
            SkinsAccessService skinsAccessService, IMessenger theMessenger) : base(theMessenger)
        {
            _scopeFactory = scopeFactory;

            _currentWindow = currentWindow;
            _skinsAccessService = skinsAccessService;

            _currentWindow.Closing += OnWindowClosing;
            _currentWindow.Loaded += WindowLoaded;

            Messenger.RegisterAll(this);
        }

        private async void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            await LoadInformation();
        }
        private async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            Messenger.UnregisterAll(this);
            _currentWindow.Closing -= OnWindowClosing;
            _currentWindow.Loaded -= WindowLoaded;

            if (_cleanShutdown)
            {
                await SaveInformation();
            }
        }

        private async Task LoadInformation()
        {
            await _skinsAccessService.LoadInformation();

            LoadLocations();


            StructureCreated = _skinsAccessService.FolderStructureIsCreated();

            if (StructureCreated)
            {
                await RefreshSkinsAsync();                
            }

            RefreshSkinTypeNames();

            if (SkinTypeNames.Count > 0)
            {
                SelectedSkinTypeName = SkinTypeNames.First();
            }

        }
        private void LoadLocations()
        {
            GameLocation = _skinsAccessService.GetGameLocation();
            SkinsLocation = _skinsAccessService.GetSkinsLocation();
            GameExecutableLocation = _skinsAccessService.GetGameExecutableLocation();
        }

        private async Task SaveInformation()
        {
            await _skinsAccessService.SaveInformation();
        }

        #region Property Changed Handlers
        partial void OnSelectedSourceChanged(SkinsSource value)
        {
            _skinsAccessService.SetSkinsSource(SelectedSource);
        }
        partial void OnSelectedSkinTypeNameChanged(string value)
        {
            RefreshSkinSubTypeNames();

            if (SkinSubTypes.Any())
            {
                SelectedSkinSubType = SkinSubTypes[0];
                RefreshAvailableSkinNames();
            }
        }
        async partial void OnSelectedSkinSubTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(SelectedSkinSubType))
            {
                SelectedSkinSubType = string.Empty;
            }

            ShowRestore = await _skinsAccessService.BackUpExists(SelectedSkinName);

            RefreshAvailableSkinNames();

            AppliedSkinName = _skinsAccessService.GetAppliedSkinName(SelectedSkinTypeName, SelectedSkinSubType);
        }
        async partial void OnSelectedSkinNameChanged(string value)
        {
            await SetScreenshots();

        }
        partial void OnSkinsLocationChanged(string value)
        {
            _skinsAccessService.SetSkinsLocation(SkinsLocation);
        }
        partial void OnGameLocationChanged(string value)
        {
            _skinsAccessService.SetGameLocation(GameLocation);
        }
        partial void OnGameExecutableLocationChanged(string value)
        {
            _skinsAccessService.SetGameExecutableLocation(GameExecutableLocation);
        }
        #endregion

        private async Task SetScreenshots()
        {
            Screenshot1 = null;
            Screenshot2 = null;

            if (string.IsNullOrEmpty(SelectedSkinName))
            {
                return;
            }

            List<string> screenshots = _skins.First(x => x.Name == SelectedSkinName).Screenshots.ToList();

            if (screenshots.Count == 1)
            {
                Screenshot1 = screenshots[0].Contains("http") switch
                {
                    true => await ImageHelperService.LoadFromWeb(new Uri(screenshots[0])),
                    _ => ImageHelperService.LoadFromResource(new Uri(screenshots[0]))
                };
            }
            else if (screenshots.Count > 1)
            {
                Screenshot1 = screenshots[0].Contains("http") switch
                {
                    true => await ImageHelperService.LoadFromWeb(new Uri(screenshots[0])),
                    _ => ImageHelperService.LoadFromResource(new Uri(screenshots[0]))
                };

                Screenshot2 = screenshots[1].Contains("http") switch
                {
                    true => await ImageHelperService.LoadFromWeb(new Uri(screenshots[1])),
                    _ => ImageHelperService.LoadFromResource(new Uri(screenshots[1]))
                };
            }

            OnPropertyChanged(nameof(Screenshot1));
            OnPropertyChanged(nameof(Screenshot2));
        }

        private void RefreshSkinTypeNames()
        {
            SkinTypeNames.Clear();

            IEnumerable<string> newSkinTypeNames = _skinsAccessService.GetSkinTypes().Select(skinType => skinType.Name)
                .Order();

            foreach (string newSkinTypeName in newSkinTypeNames)
            {
                SkinTypeNames.Add(newSkinTypeName);
            }
        }
        private void RefreshSkinSubTypeNames()
        {
            SkinSubTypes.Clear();

            IEnumerable<string> newSkinSubTypeNames = _skinsAccessService.GetSkinSubTypes(SelectedSkinTypeName).Order();

            foreach (string newSkinSubTypeName in newSkinSubTypeNames)
            {
                SkinSubTypes.Add(newSkinSubTypeName);
            }
        }
        private void RefreshAvailableSkinNames()
        {
            AvailableSkinNames.Clear();
            IEnumerable<string> newVailableSkinNames = _skins
                .Where(x => x.SkinType == SelectedSkinTypeName && x.SubType == SelectedSkinSubType)
                .Select(x => x.Name);
            foreach (string currentSkinName in newVailableSkinNames)
            {
                AvailableSkinNames.Add(currentSkinName);
            }
        }
        private async Task RefreshSkinsAsync()
        {
            _skins = [.. await _skinsAccessService.RefreshSkins(IncludeWeb)];

            RefreshAvailableSkinNames();
        }
        
        private async Task CreateStructureAsync()
        {
            Busy = true;

            ProcessingText = "Creating folder structure. Please wait.";
            await _skinsAccessService.CreateStructureAsync();

            StructureCreated = true;

            Busy = false;
        }

        #region Commands
        public bool CanStartGame => !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
        public bool CanRefreshSkins => StructureCreated && !Busy;
        public bool CanBrowse => !Busy;
        public bool CanApply => !string.IsNullOrEmpty(SelectedSkinName) && !Busy;
        public bool CanRestore => !string.IsNullOrEmpty(AppliedSkinName) && !Busy;


        [RelayCommand(CanExecute = nameof(CanApply))]
        private async Task ApplySkin()
        {
            Busy = true;

            if (WebSkinSelected)
            {
                ProcessingText = "Downloading and applying skin. Please wait.";
                string archivePath = await _skinsAccessService.DownloadSkin(SelectedSkinName, 0);

                if (await _skinsAccessService.ExtractSkin(SelectedSkinName, archivePath))
                {
                    _skins = [.. _skinsAccessService.GetSkins()];
                    RefreshAvailableSkinNames();
                }

                if (Screenshot1 is not null)
                {
                    await _skinsAccessService.SaveScreenshots(SelectedSkinName);
                }
            }
            else
            {
                ProcessingText = "Applying skin. Please wait.";
            }

            if (!await _skinsAccessService.CreateBackup(SelectedSkinName))
            {
                await ShowConfirmationWindow(
                    $"Failed to create backup.{Environment.NewLine}Do you still want to apply the skin?");

                if (_applyNoBackup)
                {
                    if (await _skinsAccessService.ApplySkin(SelectedSkinName, IsEphinea))
                    {
                        await ShowMessageBox($"Skin {SelectedSkinName} has been applied successfully.");
                    }
                    else
                    {
                        await ShowMessageBox($"Unable to apply skin {SelectedSkinName}");
                    }
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        private async Task BrowseExecutable()
        {
            IStorageProvider storageProvider = _currentWindow.StorageProvider;
            if (!storageProvider.CanOpen)
            {
                return;
            }
            Busy = true;

            ProcessingText = "Waiting for the game executable to be selected. Please wait.";

            IReadOnlyList<IStorageFile> location = await _currentWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Choose Game Executable",
                    AllowMultiple = false
                });

            if (location.Any())
            {
                if (location[0].CanBookmark)
                {
                    //await location[0].SaveBookmarkAsync();
                    GameExecutableLocation = location[0].TryGetLocalPath() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        private async Task BrowseFolder(object? parameter)
        {
            IStorageProvider storageProvider = _currentWindow.StorageProvider;
            if (!storageProvider.CanPickFolder)
            {
                return;
            }
            Busy = true;

            ProcessingText = "Waiting for a folder to be selected. Please wait.";

            string titleText = parameter?.ToString() == "Skins" ? "Choose Skins Location" : "Choose Game Location";

            IReadOnlyList<IStorageFolder> location = await _currentWindow.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = titleText,
                    AllowMultiple = false
                });

            if (location.Any())
            {
                if (parameter?.ToString() == "Skins" && location[0].CanBookmark)
                {
                    //await location[0].SaveBookmarkAsync();
                    SkinsLocation = location[0].TryGetLocalPath() ?? string.Empty;
                    await CreateStructureAsync();
                }
                else if (location[0].CanBookmark)
                {
                    //await location[0].SaveBookmarkAsync();
                    GameLocation = location[0].TryGetLocalPath() ?? string.Empty;
                }
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRefreshSkins))]
        private async Task RefreshSkins()
        {
            Busy = true;

            ProcessingText = "Refreshing skins. Please wait.";

            await RefreshSkinsAsync();

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanRestore))]
        private async Task Restore()
        {
            Busy = true;

            ProcessingText = "Restoring from backup. Please wait.";

            await _skinsAccessService.RestoreBackup(SelectedSkinName);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanStartGame))]
        private async Task StartGameAsync()
        {
            Busy = true;

            ProcessingText = "Starting the game. Please wait.";
            await FileAccessService.StartGameAsync(GameExecutableLocation);

            Busy = false;
        }
        #endregion

        #region Message Handling
        public async void Receive(OperationErrorMessage message)
        {
            await ShowErrorMessageBox(message.ErrorType, message.ErrorText);
        }
        public async void Receive(DirectoryNotEmptyMessage message)
        {
            string directory = string.Concat(message.DirectoryPath, " is not empty.", Environment.NewLine,
                               "Please select an empty directory.");

            await ShowMessageBox(directory);
        }
        public void Receive(FatalErrorMessage message)
        {
            _ = ShowErrorMessageBox(message.ErrorType, message.ErrorText);
            _cleanShutdown = false;
            _currentWindow.Close();
        }
        public void Receive(ConfirmationResponse message)
        {
            _applyNoBackup = message.IsOK;
        }
        #endregion

        private async Task ShowConfirmationWindow(string message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ConfirmationView confView = serviceScope.ServiceProvider.GetRequiredService<ConfirmationView>();
            confView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ConfirmationWindowViewModel>();
            Messenger.Send<ConfirmationViewMessage>(new(message));
            await confView.ShowDialog(_currentWindow);
        }
        private async Task ShowMessageBox(string message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            MessageBoxView mbView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
            mbView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
            Messenger.Send(new MessageBoxMessage(message));
            await mbView.ShowDialog(_currentWindow);
        }
        private async Task ShowErrorMessageBox(string errorType, string errorText)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            Messenger.Send<ErrorMessage>(new(errorType, errorText));
            await emboxView.ShowDialog(_currentWindow);
        }
    }
}