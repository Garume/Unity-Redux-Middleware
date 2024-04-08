using System;
using UnityEngine;

namespace UnityReduxMiddleware.Middlewares
{
    public static class ExceptionMiddleware
    {
        public static MiddlewareDelegate Create()
        {
            return store => next => async (action, token) =>
            {
                try
                {
                    await next(action, token);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    Debug.LogError(e);
                    throw;
                }
            };
        }
    }
}