using UnityEngine;
using UnityReduxMiddleware;
using Task = System.Threading.Tasks.Task;

namespace Sandbox
{
    public static class Middlewares
    {
        public static MiddlewareDelegate LoggerMiddleware()
        {
            return store => next => async (action, token) =>
            {
                var stateName = action.type.Split("/")[0];
                Debug.Log($"Start: {store.GetState()[stateName]}");
                await next(action, token);
                Debug.Log($"End: {store.GetState()[stateName]}");
            };
        }

        public static MiddlewareDelegate HeavyTestMiddleware()
        {
            return store => next => async (action, token) =>
            {
                Debug.Log("Start Before Heavy Test");
                await Task.Delay(3000, token);
                Debug.Log("End Before Heavy Test");
                await next(action, token);
                Debug.Log("Start After Heavy Test");
                await Task.Delay(3000, token);
                Debug.Log("End After Heavy Test");
            };
        }

        public static MiddlewareDelegate TripleTestMiddleware()
        {
            return store => next => async (action, token) =>
            {
                await next(action, token);
                await next(action, token);
                await next(action, token);
            };
        }
    }
}