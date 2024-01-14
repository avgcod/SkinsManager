using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Views;
using System;
using System.ComponentModel;

namespace SkinManager.ViewModels
{
    public partial class MessageBoxViewModel : ViewModelBase, IRecipient<MessageBoxMessage>
    {
        private readonly Window _currentWindow;

        [ObservableProperty]
        private string _messageText = string.Empty;

        public MessageBoxViewModel(MessageBoxView currentWindow, IMessenger theMessenger) : base(theMessenger)
        {
            _currentWindow = currentWindow;

            Messenger.RegisterAll(this);
            _currentWindow.Opened += OnWindowOpened;
            _currentWindow.Closing += OnWindowClosing;
        }

        [RelayCommand]
        public void OK()
        {
            _currentWindow.Close();
        }

        public void Receive(MessageBoxMessage message)
        {
            HandleMessageBoxMessage(message);
        }

        private void HandleMessageBoxMessage(MessageBoxMessage message)
        {
            MessageText = message.Message;
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            Messenger.UnregisterAll(this);
            _currentWindow.Opened -= OnWindowOpened;
            _currentWindow.Closing -= OnWindowClosing;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            _currentWindow.FindControl<Button>("btnOk")?.Focus();
        }
    }
}
