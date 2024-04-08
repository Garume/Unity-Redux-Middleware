using System;
using R3;
using Sandbox.ApiMock.Models;
using UnityReduxMiddleware;
using UnityReduxMiddleware.Epic;
using UnityReduxMiddleware.Middlewares;

namespace Sandbox.ApiMock
{
    public class StoreService : IStoreService
    {
        public StoreService()
        {
            Store = new MiddlewareStore();
            Store.AddMiddleware(ExceptionMiddleware.Create());

            var epicMiddleware = EpicMiddleware.Default<ApiMockState>();

            Store.CreateSlice(ApiMockApp.AppName, new ApiMockState(),
                builder => { builder.Add<string>(Actions.Send, Reducers.Send); });
            Store.AddMiddleware(epicMiddleware.Create());

            epicMiddleware.Run(RootEpic());
        }

        public MiddlewareStore Store { get; }

        private Epic<ApiMockState> RootEpic()
        {
            return Epic.Combine(SendRequestLoadingEpic(), SendRequestHelloEpic());
        }


        public Epic<ApiMockState> SendRequestLoadingEpic()
        {
            return (action, state) => action
                .OfAction(Actions.SendRequest)
                .Dispatch(Actions.SendAction.Invoke("Loading..."));
        }


        public Epic<ApiMockState> SendRequestHelloEpic()
        {
            return (action, state) => action
                .OfAction(Actions.SendRequest)
                .Delay(TimeSpan.FromSeconds(2))
                .Dispatch(Actions.SendAction.Invoke("Hello, Unity!"));
        }
    }
}