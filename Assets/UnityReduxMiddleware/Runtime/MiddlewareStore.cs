using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AppUI.Redux;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware
{
    public delegate Func<DispatchDelegate, DispatchDelegate> MiddlewareDelegate(Store store);

    public delegate Task DispatchDelegate(Action action, CancellationToken token = default);

    public sealed class MiddlewareStore
    {
        private readonly SynchronizationContext _mainThreadContext = SynchronizationContext.Current;
        private readonly List<Func<DispatchDelegate, DispatchDelegate>> _middlewares = new();
        private readonly Store _store = new();
        private bool _alreadyPrepared;

        private Func<DispatchDelegate, DispatchDelegate>[] _preparedMiddlewares;


        public void AddMiddleware(MiddlewareDelegate middleware)
        {
            _middlewares.Add(middleware(_store));
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

        public void Dispatch(Action action)
        {
            Task.Run(() => DispatchAsync(action));
        }

        public void Dispatch(string actionType)
        {
            Task.Run(() => DispatchAsync(actionType));
        }

        public void Dispatch<T>(string actionType, T payload)
        {
            Task.Run(() => DispatchAsync(actionType, payload));
        }

        public async Task DispatchAsync(Action action, CancellationToken token = default)
        {
            if (!_alreadyPrepared)
            {
                _preparedMiddlewares = _middlewares.AsEnumerable().Reverse().ToArray();
                _alreadyPrepared = true;
            }

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

            for (var index = 0; index < _preparedMiddlewares.Length; index++)
            {
                var oldNext = next;
                var current = _preparedMiddlewares[index];
                next = async (a, t) =>
                {
                    try
                    {
                        await current(oldNext)(a, t);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        throw;
                    }
                };
            }

            await next(action, token);
        }

        public async Task DispatchAsync(string actionType, CancellationToken token = default)
        {
            await DispatchAsync(Store.CreateAction(actionType).Invoke(), token);
        }

        public async Task DispatchAsync<T>(string actionType, T payload, CancellationToken token = default)
        {
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