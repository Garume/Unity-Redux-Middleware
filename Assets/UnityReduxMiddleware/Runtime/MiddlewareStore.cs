using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Collections;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware
{
    public delegate Func<DispatchDelegate, DispatchDelegate> MiddlewareDelegate(MiddlewareStore store);

    public delegate Task DispatchDelegate(Action action, CancellationToken token = default);

    public sealed class MiddlewareStore : IDisposable
    {
        private readonly SynchronizationContext _mainThreadContext = SynchronizationContext.Current;
        private readonly Store _store = new();
        private bool _duringSetupMiddleware;
        private SimpleListCore<Func<DispatchDelegate, DispatchDelegate>> _middlewares;

        public void Dispose()
        {
            _middlewares.Clear();
        }

        public void AddMiddleware(MiddlewareDelegate middleware)
        {
            _duringSetupMiddleware = true;
            var setUpdMiddleware = middleware(this);
            _duringSetupMiddleware = false;
            _middlewares.Add(setUpdMiddleware);
        }

        public Slice<TState> CreateSlice<TState>(
            string sliceName,
            TState initialState, System.Action<SliceReducerSwitchBuilder<TState>> reducers,
            System.Action<ReducerSwitchBuilder<TState>> extraReducers = null)

        {
            return _store.CreateSlice(sliceName, initialState, reducers, extraReducers);
        }


        public TState GetState<TState>(string name)
        {
            return _store.GetState<TState>(name);
        }

        public Dictionary<string, object> GetState()
        {
            return _store.GetState();
        }

        private void ThrowIfDuringSetupMiddleware()
        {
            if (_duringSetupMiddleware)
                throw new ArgumentException("Dispatching actions during middleware setup is not allowed.");
        }

        public void Dispatch(Action action, bool excludeMiddleware = false)
        {
            ThrowIfDuringSetupMiddleware();

            if (excludeMiddleware)
            {
                _store.Dispatch(action);
                return;
            }

            DispatchDelegate next = (a, _) =>
            {
                _store.Dispatch(a);
                return Task.CompletedTask;
            };

            var middlewares = _middlewares.AsMemory();

            for (var index = middlewares.Length - 1; index >= 0; index--)
            {
                var oldNext = next;
                var current = middlewares.Span[index];
                next = (a, t) =>
                {
                    current(oldNext)(a).Wait(t);
                    return Task.CompletedTask;
                };
            }

            next(action);
        }

        public void Dispatch(string actionType, bool excludeMiddleware = false)
        {
            ThrowIfDuringSetupMiddleware();
            Dispatch(Store.CreateAction(actionType).Invoke(), excludeMiddleware);
        }

        public void Dispatch<T>(string actionType, T payload, bool excludeMiddleware = false)
        {
            ThrowIfDuringSetupMiddleware();
            Dispatch(Store.CreateAction<T>(actionType).Invoke(payload), excludeMiddleware);
        }

        public async Task DispatchAsync(Action action, CancellationToken token = default)
        {
            ThrowIfDuringSetupMiddleware();
            DispatchDelegate next = (a, _) =>
            {
                var tcs = new TaskCompletionSource<bool>();
                _mainThreadContext.Post(_ =>
                {
                    try
                    {
                        _store.Dispatch(a);
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }, null);

                return tcs.Task;
            };

            var middlewares = _middlewares.AsMemory();

            for (var index = middlewares.Length - 1; index >= 0; index--)
            {
                var oldNext = next;
                var current = middlewares.Span[index];
                next = async (a, t) => await current(oldNext)(a, t);
            }

            await next(action, token);
        }

        public async Task DispatchAsync(string actionType, CancellationToken token = default)
        {
            ThrowIfDuringSetupMiddleware();
            await DispatchAsync(Store.CreateAction(actionType).Invoke(), token);
        }

        public async Task DispatchAsync<T>(string actionType, T payload, CancellationToken token = default)
        {
            ThrowIfDuringSetupMiddleware();
            await DispatchAsync(Store.CreateAction<T>(actionType).Invoke(payload), token);
        }

        public Unsubscriber Subscribe<TState>(string name, System.Action<TState> listener)
        {
            return _store.Subscribe(name, listener);
        }

        public void NotifyStateChanged(string name)
        {
            _store.NotifyStateChanged(name);
        }
    }
}