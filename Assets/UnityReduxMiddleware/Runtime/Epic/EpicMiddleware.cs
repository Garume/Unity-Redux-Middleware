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

    public class EpicMiddleware<TState, TDependencies>
    {
        private readonly TDependencies _dependencies;
        private readonly Subject<Epic<TState, TDependencies>> _epicSubject = new();
        private MiddlewareStore _store;

        internal EpicMiddleware()
        {
        }

        internal EpicMiddleware(TDependencies dependencies)
        {
            _dependencies = dependencies;
        }


        public MiddlewareDelegate Create()
        {
            return store =>
            {
                if (_store != null) throw new Exception("EpicMiddleware can only be used with one store.");

                _store = store;

                var initialState = (TState)store.GetState().First(x => x.Value is TState).Value;
                var actionSubjects = new Subject<Action>();
                var stateProperty = new ReactiveProperty<TState>(initialState);
                var actionObservable = actionSubjects.AsObservable().ObserveOnMainThread();
                var readOnlyStateProperty = (ReadOnlyReactiveProperty<TState>)stateProperty;

                _epicSubject
                    .Select(this, (epic, self) =>
                    {
                        var output = epic(actionObservable, readOnlyStateProperty, self._dependencies);
                        if (output == null) throw new Exception("Epic must return an observable.");
                        return output;
                    })
                    .Merge()
                    .Subscribe(this, static (action, self) => self._store.Dispatch(action));

                return next => async (action, token) =>
                {
                    await next(action, token);
                    var state = (TState)store.GetState().First(x => x.Value is TState).Value;
                    stateProperty.OnNext(state);
                    actionSubjects.OnNext(action);
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