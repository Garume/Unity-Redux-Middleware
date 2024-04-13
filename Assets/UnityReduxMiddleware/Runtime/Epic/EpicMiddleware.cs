#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
using System;
using System.Linq;
using R3;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Epic
{
    public static class EpicMiddleware
    {
        public static EpicMiddleware<TState, NoDependencies> Default<TState>()
        {
            return new EpicMiddleware<TState, NoDependencies>();
        }

        public static EpicMiddleware<TState, TDependencies> Default<TState, TDependencies>(TDependencies dependencies)
        {
            return new EpicMiddleware<TState, TDependencies>(dependencies);
        }
    }

    public class EpicMiddleware<TState, TDependencies> : IDisposable
    {
        private readonly Subject<Action> _actionSubject = new();
        private readonly TDependencies _dependencies;
        private readonly Subject<Epic<TState, TDependencies>> _epicSubject = new();
        private DisposableBag _disposables;
        private ReactiveProperty<TState> _stateProperty = new();

        private MiddlewareStore _store;

        internal EpicMiddleware()
        {
        }

        internal EpicMiddleware(TDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _actionSubject?.Dispose();
        }


        public MiddlewareDelegate Create()
        {
            return store =>
            {
                if (_store != null) throw new Exception("EpicMiddleware can only be used with one store.");

                _store = store;

                var initialState = (TState)store.GetState().First(x => x.Value is TState).Value;
                _stateProperty = new ReactiveProperty<TState>(initialState);
                _stateProperty.AddTo(ref _disposables);
                var readOnlyStateProperty = (ReadOnlyReactiveProperty<TState>)_stateProperty;

                _epicSubject
                    .Select(this, (epic, self) =>
                    {
                        var output = epic(_actionSubject, readOnlyStateProperty, self._dependencies);
                        if (output == null) throw new Exception("Epic must return an observable.");
                        return output;
                    })
                    .Merge()
                    //.ObserveOnMainThread()
                    .Subscribe(this, static (action, self) => self._store.Dispatch(action, true))
                    .AddTo(ref _disposables);

                return next => async (action, token) =>
                {
                    await next(action, token);
                    var state = (TState)store.GetState().First(x => x.Value is TState).Value;

                    _stateProperty.OnNext(state);
                    _actionSubject.OnNext(action);
                };
            };
        }

        public void Run(Epic<TState, TDependencies> root)
        {
            _epicSubject.OnNext(root);
        }

        public void Run(Epic<TState> root)
        {
            _epicSubject.OnNext((action, state, _) => root(action, state));
        }
    }
}
#endif