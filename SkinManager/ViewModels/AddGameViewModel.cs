﻿using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using SkinManager.Views;

namespace SkinManager.ViewModels
{
    public partial class AddGameViewModel(AddGameView theWindow, IMessenger theMessenger) : ViewModelBase(theMessenger)
    {
        private readonly Window _theWindow = theWindow;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OKClickedCommand))]
        public string _gameName = string.Empty;

        public bool CanOK => !string.IsNullOrEmpty(GameName);

        [RelayCommand(CanExecute =nameof(CanOK))]
        public void OKClicked()
        {
            Messenger.Send(new NewGameMessage(GameName));
            _theWindow.Close();
        }

        [RelayCommand]
        public void CancelClickedCommand()
        {
            _theWindow.Close();
        }
    }
}
