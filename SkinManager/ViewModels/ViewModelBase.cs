using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace SkinManager.ViewModels
{
    public class ViewModelBase : ObservableRecipient
    {
        public ViewModelBase(IMessenger theMessenger) : base(theMessenger)
        {
            
        }
    }
}