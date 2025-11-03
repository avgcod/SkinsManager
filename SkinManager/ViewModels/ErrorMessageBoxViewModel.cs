using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinManager.Views;

namespace SkinManager.ViewModels;

public partial class ErrorMessageBoxViewModel : ViewModelBase{
    private readonly Window _currentWindow;

    [ObservableProperty] private string _errorType = string.Empty;

    [ObservableProperty] private string _errorText = string.Empty;

    public ErrorMessageBoxViewModel(ErrorMessageBoxView theWindow, string errorType, string errorText){
        _currentWindow = theWindow;
        _errorType = errorType;
        _errorText = errorText;
    }


    [RelayCommand]
    public void OK() => _currentWindow.Close();
    
}