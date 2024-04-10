using System;

namespace UnityReduxMiddleware.Tests.Runtime.Utility
{
    public static class MockMiddleware
    {
        public static MiddlewareDelegate Create(Action onDispatched)
        {
            return store => next => async (action, token) =>
            {
                onDispatched();
                await next(action, token);
            };
        }

        public static MiddlewareDelegate CreateDispatchDuringSetup()
        {
            return store =>
            {
                store.Dispatch(new Unity.AppUI.Redux.Action(""));
                return next => async (action, token) => await next(action, token);
            };
        }
    }
}