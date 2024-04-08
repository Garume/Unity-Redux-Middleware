using Unity.AppUI.MVVM;

namespace Sandbox.ApiMock
{
    public class ApiMockAppBuilder : UIToolkitAppBuilder<ApiMockApp>
    {
        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);

            builder.services.AddSingleton<IStoreService, StoreService>();
            builder.services.AddTransient<MainViewModel>();
            builder.services.AddTransient<MainPage>();
        }
    }
}