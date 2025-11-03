using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinManager.Views;

namespace SkinManager.ViewModels;

public partial class ConfirmationWindowViewModel : ViewModelBase
{
    private readonly Window _currentWindow;

    [ObservableProperty] private string _messageText = string.Empty;

    public bool Response{ get; set; } = false;
    

    public ConfirmationWindowViewModel(ConfirmationView currentWindow, string messageText)
    {
        _currentWindow = currentWindow;
        _messageText = messageText;
    }

    [RelayCommand]
    private void OK(){
        Response = true;
        _currentWindow.Close();
    }
    
    [RelayCommand]
    private void Cancel(){
        Response = false;
        _currentWindow.Close();
    }
}