using System;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Views;

namespace SkinManager.ViewModels;

public partial class ConfirmationWindowViewModel : ViewModelBase, IRecipient<MessageBoxMessage>
{
    private readonly Window _currentWindow;

    [ObservableProperty] private string _messageText = string.Empty;

    public ConfirmationWindowViewModel(MessageBoxView currentWindow, IMessenger theMessenger) : base(theMessenger)
    {
        _currentWindow = currentWindow;

        Messenger.RegisterAll(this);
        _currentWindow.Closing += OnWindowClosing;
    }

    [RelayCommand]
    public void OK()
    {
        Messenger.Send(new ConfirmationResponse(true));
        _currentWindow.Close();
    }
    
    [RelayCommand]
    public void Cancel()
    {
        Messenger.Send(new ConfirmationResponse(false));
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
    }
}