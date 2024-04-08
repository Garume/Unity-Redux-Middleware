using Unity.AppUI.MVVM;

namespace Sandbox.ApiMock
{
    public class ApiMockApp : App
    {
        public const string AppName = "ApiMock";

        public ApiMockApp(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }
    }
}