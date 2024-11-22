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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using SkinManager.Extensions;

namespace SkinManager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<OperationErrorMessage>
        , IRecipient<DirectoryNotEmptyMessage>, IRecipient<FatalErrorMessage>
    {
        #region Variables

        private readonly Locations _locations;
        private readonly SkinsAccessService _skinsAccessService;
        private readonly FileAccessService _fileAccessService;
        private readonly Window _currentWindow;
        private readonly IServiceScopeFactory _scopeFactory;
        private bool _cleanShutdown = true;

        #endregion

        #region Collections

        private List<Skin> _skins = [];
        public ObservableCollection<string> SkinTypeNames { get; set; } = [];
        public ObservableCollection<string> SkinSubTypes { get; set; } = [];
        public ObservableCollection<string> AvailableSkinNames { get; set; } = [];
        public ObservableCollection<string> OriginalSkinNames { get; set; } = [];

        private ObservableCollection<Skin> AppliedSkins { get; set; } = [];

        public IEnumerable<string> WebSources
            => Enum.GetNames<SkinsSource>()
                .Where(x => !string.Equals(x, SkinsSource.Local.ToString(), StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Properties

        [ObservableProperty] private string _applySkinButtonText = "Apply Skin";
        [ObservableProperty] private string _processingText = "Processing. Please wait.";
        [ObservableProperty] private string _skinsLocation = string.Empty;
        [ObservableProperty] private string _gameExecutableLocation = string.Empty;
        [ObservableProperty] private string _gameLocation = string.Empty;

        [ObservableProperty] private SkinsSource _selectedSource;

        private IEnumerable<string> SelectedSkinLocation =>
            _skins.SingleOrDefault(x => x.Name == SelectedSkinName)?.Locations ?? [];

        public int SkinNameLength => AvailableSkinNames.Max(skinName => skinName.Length);

        [ObservableProperty] private string _appliedSkinName = string.Empty;

        private string BackUpLocation =>
            Path.Combine(SkinsLocation, SelectedSkinTypeName, "Originals", SelectedSkinSubType);

        public Bitmap? Screenshot1 { get; private set; }
        public Bitmap? Screenshot2 { get; private set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        [NotifyPropertyChangedFor(nameof(Screenshot1))]
        [NotifyPropertyChangedFor(nameof(Screenshot2))]
        private string _selectedSkinName = string.Empty;

        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))] [ObservableProperty]
        private string _selectedSkinTypeName = string.Empty;

        [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        private string _selectedSkinSubType = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        private bool _structureCreated = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RefreshLocalSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(RefreshWebSkinsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateStructureCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
        [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
        [NotifyCanExecuteChangedFor(nameof(CreateBackupCommand))]
        private bool _busy = false;

        private bool _webSkinSelected = false;

        #endregion

        public MainWindowViewModel(IServiceScopeFactory scopeFactory, MainWindow currentWindow, Locations locations,
            SkinsAccessService skinsAccessService, FileAccessService fileAccessService,
            IMessenger theMessenger) : base(theMessenger)
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

        private async void WindowLoaded(object? sender, RoutedEventArgs e)
        {
            await LoadInformation();
        }

        public async void OnWindowClosing(object? sender, CancelEventArgs e)
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

            await RefreshLocalSkinsAsync();

            foreach (string currentSkinType in _skinsAccessService.GetSkinTypes().Select(skinType => skinType.Name)
                         .Order())
            {
                SkinTypeNames.Add(currentSkinType);
            }

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

        private async void SkinManagerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedSource))
            {
                SetSkinAccessService();
            }
            else if (e.PropertyName == nameof(SelectedSkinTypeName))
            {
                UpdateAvailableSkinSubTypeNames();
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

        private async Task HandleSkinNameChanged()
        {
            if (_skins.SingleOrDefault(x => x.Name == SelectedSkinName) is { } currentSkin)
            {
                if (currentSkin.IsWebSkin())
                {
                    ApplySkinButtonText = "Download Skin";
                    _webSkinSelected = true;
                }
                else
                {
                    ApplySkinButtonText = "Apply Skin";
                    _webSkinSelected = false;
                }
            }

            await SetScreenshots();
        }

        private void UpdateAvailableSkinSubTypeNames()
        {
            SkinSubTypes.Clear();

            foreach (string currentSubType in _skins.Where(x => x.SkinType == SelectedSkinTypeName)
                         .Select(x => x.SubType).Distinct().Order())
            {
                SkinSubTypes.Add(currentSubType);
            }

            if (SkinSubTypes.Any())
            {
                SelectedSkinSubType = SkinSubTypes[0];
                RefreshAvailableSkinNames();
            }
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

            foreach (string currentSkinName in _skins
                         .Where(x => x.SkinType == SelectedSkinTypeName && x.SubType == SelectedSkinSubType)
                         .Select(x => x.Name))
            {
                AvailableSkinNames.Add(currentSkinName);
            }
        }

        private async Task RefreshLocalSkinsAsync()
        {
            _skins = [..(await _skinsAccessService.RefreshLocalSkinsAsync()).OrderBy(x => x.IsWebSkin()).ThenBy(x => x.Name)];

            StructureCreated = _skins.Count != 0;

            OriginalSkinNames = [.._skinsAccessService.GetOriginalSkinNames()];

            RefreshAvailableSkinNames();
        }

        private async Task RefreshWebSkinsAsync()
        {
            _skins = [..await _skinsAccessService.RefreshWebSkinsAsync()];

            RefreshAvailableSkinNames();
        }

        private void AddAppliedSkin(string appliedSkinName)
        {
            _skinsAccessService.AddAppliedSkin(appliedSkinName);
        }

        private void RemoveAppliedSkin(string removedSkinName)
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

        public bool CanBrowse => !Busy;

        public bool CanApply => !string.IsNullOrEmpty(SkinsLocation) && !string.IsNullOrEmpty(GameLocation) && !Busy;
        public bool CanRestore => Directory.Exists(BackUpLocation) && !Busy;


        [RelayCommand(CanExecute = nameof(CanApply))]
        public async Task ApplySkin()
        {
            Busy = true;

            if (!_webSkinSelected)
            {
                ProcessingText = "Applying skin. Please wait.";
                await _fileAccessService.ApplySkinAsync(SelectedSkinLocation.First(), GameLocation);
                AddAppliedSkin(SelectedSkinName);
            }
            else
            {
                ProcessingText = "Downloading skin. Please wait.";
                Skin skinToDownload = _skins.Single(x => x.Name == SelectedSkinName);
                string extractLocation = Path.Combine(SkinsLocation, skinToDownload.SkinType, skinToDownload.SubType);

                (bool Downloaded, bool Extracted) downloadStatus =
                    await _skinsAccessService.DownloadSkin(skinToDownload, 0, skinToDownload.Screenshots);

                string message = string.Empty;
                if (downloadStatus is { Downloaded: true, Extracted: false })
                {
                    message = $"Skin downloaded to {SkinsLocation}. " +
                              $"{Environment.NewLine} Please put the skin in the appropriate area, switch back to local and click reload skins.";
                }
                else
                {
                    message = $"Skin downloaded to {SkinsLocation}. ";
                }

                _skins = [.._skinsAccessService.GetCurrentSkins()];
                RefreshAvailableSkinNames();
                

                using IServiceScope serviceScope = _scopeFactory.CreateScope();
                MessageBoxView mbView = serviceScope.ServiceProvider.GetRequiredService<MessageBoxView>();
                mbView.DataContext = serviceScope.ServiceProvider.GetRequiredService<MessageBoxViewModel>();
                Messenger.Send(new MessageBoxMessage(message));
                await mbView.ShowDialog(_currentWindow);
            }

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseExecutable()
        {
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
            await _fileAccessService.CreateBackUpAsync(SelectedSkinLocation.First(), BackUpLocation, GameLocation);

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanBrowse))]
        public async Task BrowseFolder(object? parameter)
        {
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
            //await _fileAccessService.CreateStructureAsync(_skins.Select(x => x.SkinType).DistinctBy(x => x.Name), SkinsLocation);
            await _fileAccessService.CreateStructureAsync(_skinsAccessService.GetSkinTypes(), SkinsLocation);

            StructureCreated = true;

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanReloadSkins))]
        public async Task RefreshLocalSkins()
        {
            Busy = true;

            ProcessingText = "Refreshing local skins. Please wait.";

            await RefreshLocalSkinsAsync();

            Busy = false;
        }

        [RelayCommand(CanExecute = nameof(CanReloadSkins))]
        public async Task RefreshWebSkins()
        {
            Busy = true;

            ProcessingText = "Refreshing web skins. Please wait.";

            await RefreshWebSkinsAsync();

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
            Messenger.Send<MessageBoxMessage>(new(message.DirectoryPath + " is not empty." + Environment.NewLine +
                                                  "Please select an empty directory."));
            await mboxView.ShowDialog(_currentWindow);
        }

        private async Task HandleOperationErrorMessageAsync(OperationErrorMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            Messenger.Send<ErrorMessage>(new(message.ErrorType, message.ErrorText));
            await emboxView.ShowDialog(_currentWindow);
        }

        public async void Receive(FatalErrorMessage message)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            ErrorMessageBoxView emboxView = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxView>();
            emboxView.DataContext = serviceScope.ServiceProvider.GetRequiredService<ErrorMessageBoxViewModel>();
            Messenger.Send<ErrorMessage>(new(message.ErrorType, message.ErrorText));
            await emboxView.ShowDialog(_currentWindow);
            _cleanShutdown = false;
            _currentWindow.Close();
        }

        #endregion

        /// <summary>
        /// Gets the name of the applied skin from the skin location.
        /// </summary>
        /// <returns>name of the applied skin or none if none are found.</returns>
        private string GetAppliedSkinNameFromLocation()
        {
            return _skinsAccessService.GetAppliedSkinNameFromLocation(SelectedSkinTypeName, SelectedSkinSubType);
        }

        private void SetSkinAccessService()
        {
            _skinsAccessService.SetSkinsSource(SelectedSource);
        }
    }
}