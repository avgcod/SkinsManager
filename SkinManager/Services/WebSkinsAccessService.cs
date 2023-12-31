using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class WebSkinsAccessService(IMessenger theMessenger) : ISkinsAccessService
    {
        private readonly IMessenger _theMessenger = theMessenger;

        public IEnumerable<Skin> GetAvailableSkins(string skinsLocation)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                throw new NotImplementedException();
            }
            
        }

        public Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsDirectoryName)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                throw new NotImplementedException();
            }
        }

        public IEnumerable<string> GetSkinScreenshots(Skin currentSkin)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                throw new NotImplementedException();
            }
        }

        public Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                throw new NotImplementedException();
            }
        }
    }
}
