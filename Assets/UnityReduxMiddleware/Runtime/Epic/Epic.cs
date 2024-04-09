#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
using System.Linq;
using R3;
using Unity.AppUI.Redux;

namespace UnityReduxMiddleware.Epic
{
    public delegate Observable<Action> Epic<TState, in TDependencies>(
        Observable<Action> action,
        StateObservable<TState> state,
        TDependencies dependencies
    );

    public delegate Observable<Action> Epic<TState>(
        Observable<Action> action,
        StateObservable<TState> state
    );

    public static class Epic
    {
        public static Epic<TState, TDependencies> Combine<TState, TDependencies>(
            params Epic<TState, TDependencies>[] epics)
        {
            return (action, state, dependencies) =>
            {
                var magedEpics = epics.Select(epic => epic(action, state, dependencies));
                return magedEpics.Merge();
            };
        }

        public static Epic<TState> Combine<TState>(
            params Epic<TState>[] epics)
        {
            return (action, state) =>
            {
                var magedEpics = epics.Select(epic => epic(action, state));
                return magedEpics.Merge();
            };
        }
    }
}
#endif