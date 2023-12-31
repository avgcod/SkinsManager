using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Views;
using System;

namespace SkinManager.ViewModels
{
    public partial class ErrorMessageBoxViewModel : ViewModelBase, IRecipient<ErrorMessage>
    {
        private readonly Window _currentWindow;
        private readonly IMessenger _theMessenger;

        [ObservableProperty]
        private string _errorType = string.Empty;

        [ObservableProperty]
        private string _errorText = string.Empty;

        public ErrorMessageBoxViewModel(ErrorMessageBoxView theWindow, IMessenger theMessenger)
        {
            _currentWindow = theWindow;
            _theMessenger = theMessenger;

            _theMessenger.RegisterAll(this);
            _currentWindow.Closed += Window_Closed;

        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _theMessenger.UnregisterAll(this);
            _currentWindow.Closed -= Window_Closed;
        }

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

        public void Receive(ErrorMessage message)
        {
            HandleErrorMessage(message);
        }

        private void HandleErrorMessage(ErrorMessage message)
        {
            ErrorType = message.ErrorType;
            ErrorText = message.ErrorText;
        }
    }
}
