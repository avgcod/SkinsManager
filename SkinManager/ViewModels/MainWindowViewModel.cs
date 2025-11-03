using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinManager.Services;
using SkinManager.Types;
using SkinManager.Views;

namespace SkinManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase{
    #region Variables
    private readonly SkinsAccessService _skinsAccessService;
    private readonly Window _currentWindow;
    private readonly Locations _locations;
    private bool _cleanShutdown = true;
    private bool _applyNoBackup = false;
    private readonly Timer _screenshotTimer;
    #endregion

    #region Collections
    public ObservableCollection<string> SkinTypeNames{ get; set; } = [];
    public ObservableCollection<string> SkinSubTypes{ get; set; } = [];
    public ObservableCollection<string> AvailableSkinNames{ get; set; } = [];
    public IEnumerable<string> WebSources =>
        Enum.GetNames<SkinsSource>()
            .Where(x => !string.Equals(x, nameof(SkinsSource.Local), StringComparison.OrdinalIgnoreCase));
    #endregion

    #region Properties
    [ObservableProperty] private string _processingText = "Processing. Please wait.";
    [ObservableProperty] private string _skinsLocation = string.Empty;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
    private string _gameExecutableLocation = string.Empty;
    
    [ObservableProperty] private string _gameLocation = string.Empty;
    [ObservableProperty] private bool _isEphinea = true;
    [ObservableProperty] private bool _includeWeb = false;
    [ObservableProperty] private bool _showRestore = false;

    [ObservableProperty] private SkinsSource _selectedSource;

    private bool WebSkinSelected => _skinsAccessService.GetIsWebSkinSelected(SelectedSkinName);

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
    private string _appliedSkinName = string.Empty;

    [ObservableProperty] private Bitmap _screenshot = new RenderTargetBitmap(new PixelSize(1, 1));

    private List<Bitmap> Screenshots{ get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
    [NotifyPropertyChangedFor(nameof(Screenshots))]
    [NotifyPropertyChangedFor(nameof(WebSkinSelected))]
    private string _selectedSkinName = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
    private string _selectedSkinTypeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableSkinNames))]
    [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
    private string _selectedSkinSubType = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshSkinsClickedCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
    private bool _busy = false;
    #endregion

    public MainWindowViewModel(MainWindow currentWindow){
        _locations = new Locations("GameInfo.json", "AppliedSkins.json", "CachedSkins.json");
        _skinsAccessService = new SkinsAccessService();

        _currentWindow = currentWindow;

        _currentWindow.Closing += OnWindowClosing;
        _currentWindow.Loaded += WindowLoaded;

        _screenshotTimer = new Timer();
        _screenshotTimer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
        _screenshotTimer.AutoReset = true;
        _screenshotTimer.Elapsed += ScreenshotTimerOnElapsed;
    }

    private void ScreenshotTimerOnElapsed(object? sender, ElapsedEventArgs e){
        int index = Screenshots.FindIndex(currentScreenshot => currentScreenshot == Screenshot);
        Screenshot = index == Screenshots.Count - 1 ? Screenshots[0] : Screenshots[++index];
    }
    private async void WindowLoaded(object? sender, RoutedEventArgs e){
        await _skinsAccessService.LoadInformation(_locations);
        await _skinsAccessService.RefreshSkins(false);
        
        LoadLocations();
        RefreshSkins();
    }
    private async void OnWindowClosing(object? sender, CancelEventArgs e){
        _currentWindow.Closing -= OnWindowClosing;
        _currentWindow.Loaded -= WindowLoaded;

        if (_cleanShutdown){
            await SaveInformation();
        }
    }
    
    private void LoadLocations(){
        GameLocation = _skinsAccessService.GetGameLocation();
        SkinsLocation = _skinsAccessService.GetSkinsLocation();
        GameExecutableLocation = _skinsAccessService.GetGameExecutableLocation();
    }
    private async Task SaveInformation(){
        await _skinsAccessService.SaveInformation(_locations);
    }
        
    #region Property Changed Handlers
    partial void OnSelectedSourceChanged(SkinsSource value){
        _skinsAccessService.SetSkinsSource(SelectedSource);
    }
    partial void OnSelectedSkinTypeNameChanged(string value){
        ResetScreenShot();
        RefreshSkinSubTypeNames();

        if (SkinSubTypes.Any()){
            SelectedSkinSubType = SkinSubTypes[0];
            RefreshAvailableSkinNames();
        }
    }
    async partial void OnSelectedSkinSubTypeChanged(string value){
        ResetScreenShot();
        if (string.IsNullOrEmpty(SelectedSkinSubType)){
            SelectedSkinSubType = string.Empty;
        }

        (await _skinsAccessService.BackUpExists(SelectedSkinName)).IfSucc(backUpExists => {
            ShowRestore = backUpExists;
        });

        RefreshAvailableSkinNames();

        AppliedSkinName = _skinsAccessService.GetAppliedSkinName(SelectedSkinTypeName, SelectedSkinSubType);
    }
    async partial void OnSelectedSkinNameChanged(string value){
        ResetScreenShot();
        await SetScreenshots();
    }
    partial void OnSkinsLocationChanged(string value){
        _skinsAccessService.SetSkinsLocation(SkinsLocation);
    }
    partial void OnGameLocationChanged(string value){
        _skinsAccessService.SetGameLocation(GameLocation);
    }
    partial void OnGameExecutableLocationChanged(string value){
        _skinsAccessService.SetGameExecutableLocation(GameExecutableLocation);
    }
    #endregion
        
    private async Task SetScreenshots(){
        Screenshots.Clear();

        if (string.IsNullOrEmpty(SelectedSkinName)){
            return;
        }

        ImmutableList<string> currentScreenshots = _skinsAccessService.GetSkinScreenshots(SelectedSkinName);

        foreach (string screenshot in currentScreenshots){
            Screenshots.Add(screenshot.Contains("http") switch{
                true => await ImageHelperService.LoadFromWeb(new Uri(screenshot)) ?? new RenderTargetBitmap(new PixelSize(1, 1)),
                _ => ImageHelperService.LoadFromResource(new Uri(screenshot))
            });
        }
            
        if(Screenshots.Any()) Screenshot = Screenshots.First();
        if (Screenshots.Count > 1) _screenshotTimer.Start();

        OnPropertyChanged(nameof(Screenshot));
    }
    private void RefreshSkinTypeNames(){
        SkinTypeNames.Clear();

        IEnumerable<string> newSkinTypeNames = _skinsAccessService.GetSkinTypes()
            .Order();

        foreach (string newSkinTypeName in newSkinTypeNames){
            SkinTypeNames.Add(newSkinTypeName);
        }
    }
    private void RefreshSkinSubTypeNames(){
        SkinSubTypes.Clear();

        IEnumerable<string> newSkinSubTypeNames = _skinsAccessService.GetSkinSubTypes(SelectedSkinTypeName).Order();

        foreach (string newSkinSubTypeName in newSkinSubTypeNames){
            SkinSubTypes.Add(newSkinSubTypeName);
        }
    }
    private void RefreshAvailableSkinNames(){
        AvailableSkinNames.Clear();
        foreach (string currentSkinName in _skinsAccessService.GetAvailableSkinNames(SelectedSkinTypeName, SelectedSkinSubType).Order()){
            AvailableSkinNames.Add(currentSkinName);
        }
    }
    private void RefreshSkins(){
        RefreshAvailableSkinNames();
        RefreshSkinTypeNames();
        RefreshSkinSubTypeNames();
            
        if (SkinTypeNames.Count > 0) SelectedSkinTypeName = SkinTypeNames.First();
    }

    #region Commands

    public bool CanStartGame => !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
    public bool CanRefreshSkins => !Busy;
    public bool CanBrowse => !Busy;
    public bool CanApply => !string.IsNullOrEmpty(SelectedSkinName) && !Busy;
    public bool CanRestore => !string.IsNullOrEmpty(AppliedSkinName) && !Busy;


    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplySkin(){
        Busy = true;

        if (WebSkinSelected){
            ProcessingText = "Downloading and applying skin. Please wait.";
            string archivePath = await _skinsAccessService.DownloadSkin(new HttpClient(), SelectedSkinName, 0);

            if (await _skinsAccessService.ExtractSkin(SelectedSkinName, archivePath)) RefreshAvailableSkinNames();

            if (Screenshots.Any()) await _skinsAccessService.SaveScreenshots(SelectedSkinName);
        }
        else{
            ProcessingText = "Applying skin. Please wait.";
        }

        (await _skinsAccessService.CreateBackup(SelectedSkinName)).IfSucc(async _ => {
            await ShowConfirmationWindow(
                $"Failed to create backup.{Environment.NewLine}Do you still want to apply the skin?");

            if (_applyNoBackup){
                if (await _skinsAccessService.ApplySkin(SelectedSkinName, IsEphinea)){
                    await ShowMessageBox($"Skin {SelectedSkinName} has been applied successfully.");
                }
                else{
                    await ShowMessageBox($"Unable to apply skin {SelectedSkinName}");
                }
            }
        });

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanBrowse))]
    private async Task BrowseExecutable(){
        IStorageProvider storageProvider = _currentWindow.StorageProvider;
        if (!storageProvider.CanOpen){
            return;
        }

        Busy = true;

        ProcessingText = "Waiting for the game executable to be selected. Please wait.";

        IReadOnlyList<IStorageFile> location = await _currentWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions{
                Title = "Choose Game Executable",
                AllowMultiple = false
            });

        if (location.Any()){
            if (location[0].CanBookmark){
                //await location[0].SaveBookmarkAsync();
                GameExecutableLocation = location[0].TryGetLocalPath() ?? string.Empty;
            }
        }

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanBrowse))]
    private async Task BrowseFolder(object? parameter){
        IStorageProvider storageProvider = _currentWindow.StorageProvider;
        if (!storageProvider.CanPickFolder){
            return;
        }

        Busy = true;

        ProcessingText = "Waiting for a folder to be selected. Please wait.";

        string titleText = parameter?.ToString() == "Skins" ? "Choose Skins Location" : "Choose Game Location";

        IReadOnlyList<IStorageFolder> location = await _currentWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions{
                Title = titleText,
                AllowMultiple = false
            });

        if (location.Any()){
            if (parameter?.ToString() == "Skins" && location[0].CanBookmark){
                //await location[0].SaveBookmarkAsync();
                SkinsLocation = location[0].TryGetLocalPath() ?? string.Empty;
            }
            else if (location[0].CanBookmark){
                //await location[0].SaveBookmarkAsync();
                GameLocation = location[0].TryGetLocalPath() ?? string.Empty;
            }
        }

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanRefreshSkins))]
    private async Task OnRefreshSkinsClicked(){
        ResetScreenShot();
        Busy = true;

        ProcessingText = "Refreshing skins. Please wait.";

        await _skinsAccessService.RefreshSkins(IncludeWeb);

        RefreshSkins();

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanRestore))]
    private async Task Restore(){
        Busy = true;

        ProcessingText = "Restoring from backup. Please wait.";

        await _skinsAccessService.RestoreBackup(SelectedSkinName);

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanStartGame))]
    private async Task StartGameAsync(){
        Busy = true;

        ProcessingText = "Starting the game. Please wait.";
        await FileAccessService.StartGameAsync(GameExecutableLocation);

        Busy = false;
    }

    #endregion

    private async Task ShowConfirmationWindow(string message){
        ConfirmationView confView = new ConfirmationView();
        confView.DataContext = new ConfirmationWindowViewModel(confView, message);
        await confView.ShowDialog(_currentWindow);
    }
    private async Task ShowMessageBox(string message){
        MessageBoxView mbView = new MessageBoxView();
        mbView.DataContext = new MessageBoxViewModel(mbView, message);
        await mbView.ShowDialog(_currentWindow);
    }
    private async Task ShowErrorMessageBox(string errorType, string errorText){
        ErrorMessageBoxView emboxView = new ErrorMessageBoxView();
        emboxView.DataContext = new ErrorMessageBoxViewModel(emboxView, errorType, errorText);
        await emboxView.ShowDialog(_currentWindow);
    }
    private void ResetScreenShot(){
        _screenshotTimer.Stop();
        Screenshot = new RenderTargetBitmap(new PixelSize(1, 1));
    }
}