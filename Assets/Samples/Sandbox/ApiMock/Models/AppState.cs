using Unity.AppUI.Redux;

namespace Sandbox.ApiMock.Models
{
    public record ApiMockState
    {
        public string Message { get; set; } = "Hello, World!";
    }

    public class Actions
    {
        public const string Send = ApiMockApp.AppName + "/send";
        public const string SendRequest = ApiMockApp.AppName + "/sendRequest";

        public static readonly ActionCreator<string> SendAction = Store.CreateAction<string>(Send);
    }

    public class Reducers
    {
        public static ApiMockState Send(ApiMockState state, Action<string> action)
        {
            return state with { Message = action.payload };
        }
    }
}