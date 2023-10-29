using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;

namespace SkinManager.ViewModels
{
    public partial class MessageBoxViewViewModel : ViewModelBase
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _messageText = string.Empty;

        public MessageBoxViewViewModel(Window currentWindow, string messageText)
        {
            MessageText = messageText;
            _currentWindow = currentWindow;

            _currentWindow.Opened += OnWindowOpened;
            _currentWindow.Closing += OnWindowClosing;
        }

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _currentWindow.Opened -= OnWindowOpened;
            _currentWindow.Closing -= OnWindowClosing;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            _currentWindow.FindControl<Button>("btnOk")?.Focus();
        }
    }
}
