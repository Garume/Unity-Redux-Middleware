#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
using System;
using System.Linq;
using R3;
using Unity.AppUI.Redux;
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
        private readonly Subject<Epic<TState, TDependencies>> EpicSubject = new();
        private Store _store;

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

                var actionSubjects = new Subject<Action>();
                var stateSubjects = new Subject<TState>();
                var actionObservable = actionSubjects.AsObservable().ObserveOnMainThread();
                var initialState = (TState)store.GetState().First(x => x.Value is TState).Value;
                var stateObservable =
                    new StateObservable<TState>(stateSubjects.AsObservable().ObserveOnMainThread(), initialState);
                var result = EpicSubject
                    .Select(this, (epic, self) =>
                    {
                        var output = epic(actionObservable, stateObservable, self._dependencies);
                        if (output == null) throw new Exception("Epic must return an observable.");
                        return output;
                    }).Merge();

                result.Subscribe(this, (action, self) => { self._store.Dispatch(action); });

                return next => async (action, token) =>
                {
                    await next(action, token);
                    var state = (TState)store.GetState().First(x => x.Value is TState).Value;
                    stateSubjects.OnNext(state);
                    actionSubjects.OnNext(action);
                };
            };
        }

        public void Run(Epic<TState, TDependencies> root)
        {
            EpicSubject.OnNext(root);
        }

        public void Run(Epic<TState> root)
        {
            EpicSubject.OnNext((action, state, _) => root(action, state));
        }
    }
}
#endif