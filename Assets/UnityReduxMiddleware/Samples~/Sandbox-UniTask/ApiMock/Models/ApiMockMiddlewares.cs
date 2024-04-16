using System.Threading.Tasks;
using Unity.AppUI.Redux;
using UnityEngine;
using UnityReduxMiddleware;

namespace Sandbox.ApiMock.Models
{
    public static class ApiMockMiddlewares
    {
        private static readonly ActionCreator<string> Send = Store.CreateAction<string>(Actions.Send);

        public static MiddlewareDelegate ApiMockRequestMiddleware()
        {
            return store => next => async (action, token) =>
            {
                if (action.type != Actions.SendRequest)
                {
                    await next(action, token);
                }
                else
                {
                    await next(Send.Invoke("Loading..."), token);
                    await Task.Delay(2000, token);
                    await next(Send.Invoke("Hello, World!"), token);
                }
            };
        }
    }
}