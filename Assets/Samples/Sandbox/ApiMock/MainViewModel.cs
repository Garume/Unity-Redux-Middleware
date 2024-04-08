using Sandbox.ApiMock.Models;
using Unity.AppUI.MVVM;

namespace Sandbox.ApiMock
{
    public class MainViewModel : ObservableObject
    {
        private readonly IStoreService _storeService;
        public readonly RelayCommand SendCommend;
        public readonly RelayCommand SendRequestCommend;
        private string _message;

        public MainViewModel(IStoreService storeService)
        {
            _storeService = storeService;
            SendCommend = new RelayCommand(Send);
            SendRequestCommend = new RelayCommand(SendRequest);

            _message = _storeService.Store.GetState<ApiMockState>(ApiMockApp.AppName).Message;
            _storeService.Store.Subscribe<ApiMockState>(ApiMockApp.AppName, OnStateChanged);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private void OnStateChanged(ApiMockState state)
        {
            if (state.Message != _message) Message = state.Message;
        }

        private void Send()
        {
            _storeService.Store.Dispatch(Actions.Send, "Hello, Unity!");
        }

        private void SendRequest()
        {
            _storeService.Store.Dispatch(Actions.SendRequest);
        }
    }
}