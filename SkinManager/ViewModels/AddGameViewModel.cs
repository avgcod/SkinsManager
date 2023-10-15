using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;

namespace SkinManager.ViewModels
{
    public partial class AddGameViewModel : ViewModelBase
    {
        private readonly IMessenger _theMessenger;
        private readonly Window _theWindow;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OKClickedCommand))]
        public string _gameName = string.Empty;

        public AddGameViewModel(Window theWindow, IMessenger theMessenger)
        {
            _theWindow = theWindow;
            _theMessenger = theMessenger;            
        }

        public bool CanOK => !string.IsNullOrEmpty(GameName);

        [RelayCommand(CanExecute =nameof(CanOK))]
        public void OKClicked()
        {
            _theMessenger.Send(new NewGameMessage(GameName));
            _theWindow.Close();
        }

        [RelayCommand]
        public void CancelClickedCommand()
        {
            _theWindow.Close();
        }
    }
}
