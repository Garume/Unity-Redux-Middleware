#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Collections;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware
{
    public delegate Func<DispatchDelegate, DispatchDelegate> MiddlewareDelegate(MiddlewareStore store);

#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
    public delegate UniTask DispatchDelegate(Action action, CancellationToken token = default);
#else
    public delegate Task DispatchDelegate(Action action, CancellationToken token = default);
#endif

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
#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
                return UniTask.CompletedTask;
#else
                return Task.CompletedTask;
#endif
            };

            var middlewares = _middlewares.AsMemory();

            for (var index = middlewares.Length - 1; index >= 0; index--)
            {
                var oldNext = next;
                var current = middlewares.Span[index];
                next = (a, t) =>
                {
                    current(oldNext)(a);
#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
                    return UniTask.CompletedTask;
#else
                    return Task.CompletedTask;
#endif
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


#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
        public async UniTask DispatchAsync(Action action, CancellationToken token = default)
#else
        public async Task DispatchAsync(Action action, CancellationToken token = default)
#endif
        {
            ThrowIfDuringSetupMiddleware();
            DispatchDelegate next = (a, _) =>
            {
#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
                var tcs = new UniTaskCompletionSource<bool>();
#else
                var tcs = new TaskCompletionSource<bool>();
#endif
                _mainThreadContext.Post(_ =>
                {
                    try
                    {
                        _store.Dispatch(a);
#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
                        tcs.TrySetResult(true);
#else
                        tcs.SetResult(true);
#endif
                    }
                    catch (Exception ex)
                    {
#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
                        tcs.TrySetException(ex);
#else
                        tcs.SetException(ex);
#endif
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

#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
        public async UniTask DispatchAsync(string actionType, CancellationToken token = default)
#else
        public async Task DispatchAsync(string actionType, CancellationToken token = default)
#endif
        {
            ThrowIfDuringSetupMiddleware();
            await DispatchAsync(Store.CreateAction(actionType).Invoke(), token);
        }

#if UNITYREDUXMIDDLEWARE_UNITASK_INTEGRATION
        public async UniTask DispatchAsync<T>(string actionType, T payload, CancellationToken token = default)
#else
        public async Task DispatchAsync<T>(string actionType, T payload, CancellationToken token = default)
#endif
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