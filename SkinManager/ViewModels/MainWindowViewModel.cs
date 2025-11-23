using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinManager.Services;
using SkinManager.Types;
using SkinManager.Views;
using SkinManager.Extensions;
using LanguageExt;

namespace SkinManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase{
    #region Variables

    private readonly Window _currentWindow;
    private readonly string _appSettingsFile;
    private bool _applyNoBackup = false;
    private AppSettings _appSettings = AppSettings.Empty();
    private SkinsState _skinsState = SkinsState.Empty();
    private GameInfo _gameInfo = GameInfo.Empty();

    #endregion

    #region Collections

    public IEnumerable<string> SkinTypeNames => RefreshSkinTypeNames();
    public List<string> AvailableSkinSubTypes => RefreshSkinSubTypeNames();
    public IEnumerable<Skin> AvailableSkins => RefreshAvailableSkins();
    //public IEnumerable<DisplaySkin> AvailableSkins => RefreshAvailableSkins();

    public IEnumerable<string> WebSources =>
        Enum.GetNames<SkinsSource>()
            .Where(x => !string.Equals(x, nameof(SkinsSource.Local), StringComparison.OrdinalIgnoreCase));

    [ObservableProperty] private List<Bitmap> _screenshots = [];

    #endregion

    #region Properties

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AvailableSkins))]
    private string _searchText = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AvailableSkins))]
    private bool _showWebSkins = true;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ApplySkinCommand))]
    private Option<DisplaySkin> _selectedSkin = Option<DisplaySkin>.None;

    [ObservableProperty] private string _processingText = "Processing. Please wait.";
    [ObservableProperty] private string _skinsLocation = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
    private string _gameExecutableLocation = string.Empty;

    [ObservableProperty] private string _gameLocation = string.Empty;
    [ObservableProperty] private bool _isEphinea = true;
    [ObservableProperty] private bool _includeWeb = true;
    [ObservableProperty] private bool _showRestore = false;

    [ObservableProperty] private SkinsSource _selectedSource;

    private bool WebSkinSelected => _skinsState.GetIsWebSkinSelected(SelectedSkin);

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
    private Option<LocalSkin> _appliedSkin = Option<LocalSkin>.None;

    [ObservableProperty] private string _selectedSkinType = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableSkins))]
    [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
    private int _selectedSkinSubTypeIndex = -1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshSkinsClickedCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(BrowseFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(BrowseExecutableCommand))]
    private bool _busy = false;

    #endregion

    public MainWindowViewModel(MainWindow currentWindow, string appSettingsFile){
        _appSettingsFile = appSettingsFile;

        _currentWindow = currentWindow;

        _currentWindow.Closing += OnWindowClosing;
        _currentWindow.Loaded += WindowLoaded;
    }

    private async void WindowLoaded(object? sender, RoutedEventArgs e){
        Busy = true;

        await LoadInformation();

        GameLocation = _gameInfo.GameLocation;
        SkinsLocation = _gameInfo.SkinsLocation;
        GameExecutableLocation = _gameInfo.GameExecutable;

        RefreshSkins();

        Busy = false;
    }

    private async void OnWindowClosing(object? sender, CancelEventArgs e){
        _currentWindow.Closing -= OnWindowClosing;
        _currentWindow.Loaded -= WindowLoaded;

        await SaveInformation();
    }

    private async Task LoadInformation(){
        (await FileAccessService.LoadJsonToObject<AppSettings>(_appSettingsFile))
            .IfSucc(currentLocations => _appSettings = currentLocations);

        (await FileAccessService.LoadJsonToObject<GameInfo>(nameof(GameInfo) + ".json"))
            .IfSucc(currentGameInfo => _gameInfo = currentGameInfo);

        var addressBooks = (await Enum.GetNames<SkinsSource>()
                .Select(currentSource => currentSource + "AddressBook.json")
                .SelectAsync(FileAccessService.LoadJsonToObject<AddressBook>))
            .Successes();
        _skinsState = _skinsState with{ AddressBooks = _skinsState.AddressBooks.AddRange(addressBooks) };

        LocalSkinsAccessService.GetAvailableSkinsAsync(_gameInfo.SkinsLocation)
            .IfSucc(localSkins => _skinsState = _skinsState.AddLocalSkins(localSkins));

        (await FileAccessService.LoadJsonToImmutableList<WebSkin>("CachedSkins.json"))
            .IfSucc(cachedSkins => _skinsState = _skinsState.AddWebSkins(cachedSkins));

        (await FileAccessService.LoadJsonToImmutableList<LocalSkin>("AppliedSkins.json"))
            .IfSucc(appliedSkins => _skinsState = _skinsState.AddAppliedSkins(appliedSkins));
    }

    private async Task SaveInformation(){
        List<Task> tasks =[
            FileAccessService.SaveIEnumerableToJson(_skinsState.WebSkins, _appSettings.CachedSkinsFile),
            FileAccessService.SaveObjectToJson(_gameInfo, _appSettings.GameInfoFile),
            FileAccessService.SaveIEnumerableToJson(_skinsState.AppliedSkins, _appSettings.AppliedSkinsFile)
        ];
        await Task.WhenAll(tasks);
    }

    #region Property Changed Handlers

    async partial void OnSelectedSkinSubTypeIndexChanged(int value){
        if (value > -1){
            SelectedSkin.IfSome(currentSkin => {
                if (_skinsState.LocalSkins.FirstOrDefault(x => x.SkinName == currentSkin.SkinName) is
                    { } selectedSkin){
                    string backupPath = Path.Combine(_gameInfo.SkinsLocation, selectedSkin.SkinType, "Originals",
                        selectedSkin.SkinSubType);
                    FileAccessService.FolderHasFiles(backupPath).IfSucc(backUpExists => {
                        ShowRestore = backUpExists;
                    });
                }
            });


            AppliedSkin = _skinsState.GetAppliedSkin(SelectedSkinType, GetSelectedSkinSubType());
        }
        else{
            AppliedSkin = Option<LocalSkin>.None;
        }
    }

    partial void OnSelectedSkinTypeChanged(string value){
        OnPropertyChanged(nameof(AvailableSkinSubTypes));
        if (AvailableSkinSubTypes.Any()) SelectedSkinSubTypeIndex = 0;
    }

    partial void OnAppliedSkinChanged(Option<LocalSkin> value){
        if (AppliedSkin.IsSome)
            ShowRestore = true;
    }

    private string GetSelectedSkinSubType() =>
        SelectedSkinSubTypeIndex > -1 ? AvailableSkinSubTypes[SelectedSkinSubTypeIndex] : string.Empty;

    partial void OnSelectedSkinChanged(Option<DisplaySkin> value){
        Screenshots = [];
        value.IfSome(async currentSkin => {
            Skin foundSkin = _skinsState.GetSkinFromDisplaySkin(currentSkin);
            Screenshots = foundSkin switch{
                WebSkin currentWekSin => new List<Bitmap>(await GetScreenshots(currentWekSin.SkinName)),
                LocalSkin currentLocalSkin => new List<Bitmap>(await GetScreenshots(currentLocalSkin.SkinName))
            };
        });

        OnPropertyChanged(nameof(WebSkinSelected));
    }

    partial void OnSkinsLocationChanged(string value) => _gameInfo = _gameInfo.UpdateSkinsLocation(SkinsLocation);
    partial void OnGameLocationChanged(string value) => _gameInfo = _gameInfo.UpdateGameLocation(GameLocation);

    partial void OnGameExecutableLocationChanged(string value) =>
        _gameInfo = _gameInfo.UpdateGameExecutableLocation(GameExecutableLocation);

    #endregion

    private async Task<IEnumerable<Bitmap>> GetScreenshots(string selectedSkinName){
        if (string.IsNullOrEmpty(selectedSkinName)) return [];

        return await _skinsState.GetSkinScreenshots(selectedSkinName).SelectAsync(async currentScreenshot =>
            currentScreenshot.Contains("http") switch{
                true => await ImageHelperService.LoadFromWeb(new Uri(currentScreenshot)) ??
                        new RenderTargetBitmap(new PixelSize(1, 1)),
                _ => ImageHelperService.LoadFromResource(currentScreenshot)
            }
        );
    }

    private IEnumerable<string> RefreshSkinTypeNames() => _skinsState.GetSkinTypes().Order();
    private List<string> RefreshSkinSubTypeNames() => _skinsState.GetSkinSubTypes(SelectedSkinType).Order().ToList();

    private IEnumerable<Skin> RefreshAvailableSkins() =>
        string.IsNullOrEmpty(SearchText)
            ? _skinsState.GetAvailableSkins(SelectedSkinType, GetSelectedSkinSubType(), ShowWebSkins)
            : GetSearchResults();

    private void RefreshSkins(){
        if (SkinTypeNames.FirstOrDefault() is{ } skinType){
            OnPropertyChanged(nameof(SkinTypeNames));
            SelectedSkinType = skinType;
        }

        if (AvailableSkinSubTypes.Any()) SelectedSkinSubTypeIndex = 0;
    }

    #region Commands

    public bool CanStartGame => !string.IsNullOrEmpty(GameExecutableLocation) && !Busy;
    public bool CanRefreshSkins => !Busy;
    public bool CanBrowse => !Busy;

    public bool CanApply() =>
        SelectedSkin.Match(
            currentSkin => !string.IsNullOrEmpty(currentSkin.SkinName) && !Busy &&
                           !string.IsNullOrWhiteSpace(SkinsLocation),
            () => false);

    public bool CanRestore => AppliedSkin.IsSome && !Busy && !string.IsNullOrWhiteSpace(SkinsLocation);

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplySkin(){
        Busy = true;
        SelectedSkin.IfSome(async currentSkin => {
                if (_skinsState.GetSkinFromDisplaySkin(currentSkin) is WebSkin currentWebSkin){
                    ProcessingText = "Downloading skin. Please wait.";
                    await DownloadSkin(currentWebSkin);
                }
        });

        ProcessingText = "Creating backups. Please wait.";

        LocalSkin selectedSkin = _skinsState.LocalSkins.First(x => x.SkinName == SelectedSkin.GetSkinName());

        var backupResult = await CreateBackup(selectedSkin);

        await ApplySkin(backupResult, selectedSkin);

        AppliedSkin = _skinsState.GetAppliedSkin(SelectedSkinType, GetSelectedSkinSubType());
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
        Busy = true;

        ProcessingText = "Refreshing skins. Please wait.";

        LocalSkinsAccessService.GetAvailableSkinsAsync(_gameInfo.SkinsLocation).IfSucc(async localSkins => {
            _skinsState = _skinsState.ReplaceLocalSkins(localSkins);
        });
        
        if (IncludeWeb){
            var newWebSkins = await WebAccessService.GetWebSkins(new HttpClient(), _skinsState.AddressBooks);
            _skinsState = _skinsState.ReplaceWebSkins(newWebSkins);
        }

        RefreshSkins();

        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanRestore))]
    private async Task Restore(){
        Busy = true;

        ProcessingText = "Restoring from backup. Please wait.";

        await _skinsState.GetAppliedSkin(SelectedSkinType, GetSelectedSkinSubType()).Match(async appliedSkin => {
                await (await FileAccessService.RestoreBackupAsync(appliedSkin.SkinLocation, _gameInfo.GameLocation))
                    .Match(async _ => {
                            _skinsState = _skinsState.RemoveAppliedSkin(appliedSkin);
                            await ShowMessageBox(
                                $"Successfully restored the back up for {appliedSkin.SkinType}-{appliedSkin.SkinSubType}");
                        },
                        async error => await ShowMessageBox(error.Message));
            },
            () => Task.CompletedTask);

        AppliedSkin = _skinsState.GetAppliedSkin(SelectedSkinType, GetSelectedSkinSubType());
        Busy = false;
    }

    [RelayCommand(CanExecute = nameof(CanStartGame))]
    private void StartGame(){
        Busy = true;

        ProcessingText = "Starting the game. Please wait.";
        FileAccessService.StartGame(GameExecutableLocation);

        Busy = false;
    }

    #endregion

    private async Task DownloadSkin(WebSkin currentWebSkin){
        var downloadInfo = await WebAccessService.DownloadSkin(new HttpClient(),
            currentWebSkin, _gameInfo.SkinsLocation);

        await downloadInfo.Match(async archivePath => {
                string newSkinPath = !string.IsNullOrWhiteSpace(currentWebSkin.Author)
                    ? Path.Combine(_gameInfo.SkinsLocation, currentWebSkin.SkinType,
                        currentWebSkin.SkinSubType,
                        currentWebSkin.SkinName.RemoveSpecialCharacters() + "_by_" + currentWebSkin.Author)
                    : Path.Combine(_gameInfo.SkinsLocation, currentWebSkin.SkinType,
                        currentWebSkin.SkinSubType, currentWebSkin.SkinName.RemoveSpecialCharacters());
                (await FileAccessService.ExtractSkinAsync(archivePath, newSkinPath)).IfSucc(async _ => {
                    if (Screenshots.FirstOrDefault() is not null){
                        string screenshotsPath = Path.Combine(newSkinPath, "Screenshots");
                        var newScreenshotLinks =
                            FileAccessService.SaveScreenshots(screenshotsPath, Screenshots
                                .Zip(currentWebSkin.ScreenshotLinks)
                                .Select(currentInfo =>
                                    new ValueTuple<Bitmap, string>(currentInfo.First,
                                        Path.GetExtension(currentInfo.Second))));
                        newScreenshotLinks.IfSucc(currentLinks =>
                            currentWebSkin = currentWebSkin with{
                                ScreenshotLinks = currentLinks.ToImmutableList()
                            });
                    }

                    _skinsState = _skinsState.ChangeWebSkinToLocalSkin(currentWebSkin, newSkinPath);
                });
            },
            async error => await ShowMessageBox(error.Message));
    }

    private async Task<Fin<bool>> CreateBackup(LocalSkin selectedSkin){
        string backupLocation = _skinsState.GetBackupLocation(SelectedSkin.GetSkinName(), _gameInfo.SkinsLocation);

        return await FileAccessService.CreateBackUpAsync(selectedSkin.SkinLocation, backupLocation,
            _gameInfo.GameLocation);
    }

    private async Task ApplySkin(Fin<bool> backupResult, LocalSkin selectedSkin){
        backupResult.IfSucc(async _ => {
            if (FileAccessService.ApplySkin(selectedSkin.SkinLocation, _gameInfo.GameLocation)){
                _skinsState = _skinsState.AddAppliedSkin(selectedSkin);
                await ShowMessageBox($"Skin {SelectedSkin.GetSkinName()} has been applied successfully.");
            }
            else{
                await ShowMessageBox($"Unable to apply skin {SelectedSkin.GetSkinName()}");
            }
        });

        backupResult.IfFail(async error => {
            _applyNoBackup = await ShowConfirmationWindow(
                $"Failed to create backup.{Environment.NewLine}Do you still want to apply the skin?");
        });

        if (_applyNoBackup){
            if (FileAccessService.ApplySkin(selectedSkin.SkinLocation, _gameInfo.GameLocation)){
                _skinsState = _skinsState.AddAppliedSkin(selectedSkin);
                await ShowMessageBox($"Skin {SelectedSkin.GetSkinName()} has been applied successfully.");
            }
            else{
                await ShowMessageBox($"Unable to apply skin {SelectedSkin.GetSkinName()}");
            }
        }
    }

    private IEnumerable<Skin> GetSearchResults(){
        return _skinsState.SearchForSkins(SearchText);
    }

    private async Task<bool> ShowConfirmationWindow(string message){
        ConfirmationView confView = new ConfirmationView();
        confView.DataContext = new ConfirmationWindowViewModel(confView, message);
        await confView.ShowDialog(_currentWindow);
        return ((ConfirmationWindowViewModel)confView.DataContext).Response;
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
}