#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
using System;
using System.Linq;
using R3;
using Unity.AppUI.Redux;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Epic
{
    public delegate Observable<Action> Epic<TState, in TDependencies>(
        Observable<Action> action,
        ReadOnlyReactiveProperty<TState> state,
        TDependencies dependencies
    );

    public delegate Observable<Action> Epic<TState>(
        Observable<Action> action,
        ReadOnlyReactiveProperty<TState> state
    );

    public static class Epic
    {
        public static Epic<TState> Create<TState>(
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, Observable<Action>> epic)
        {
            return (action, state) => epic(action, state);
        }

        public static Epic<TState> Create<TState>(
            string actionType,
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, Observable<Action>> epic)
        {
            return Create<TState>((action, state) =>
                epic(action.OfAction(actionType), state));
        }

        public static Epic<TState> Create<TState>(ActionCreator action,
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, Observable<Action>> epic)
        {
            return Create(action.type, epic);
        }

        public static Epic<TState, TDependencies> Create<TState, TDependencies>(
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, TDependencies, Observable<Action>> epic)
        {
            return (action, state, dependencies) => epic(action, state, dependencies);
        }

        public static Epic<TState, TDependencies> Create<TState, TDependencies>(
            string actionType,
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, TDependencies, Observable<Action>> epic)
        {
            return Create<TState, TDependencies>((action, state, dependencies) =>
                epic(action.OfAction(actionType), state, dependencies));
        }

        public static Epic<TState, TDependencies> Create<TState, TDependencies>(
            ActionCreator action,
            Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, TDependencies, Observable<Action>> epic)
        {
            return Create(action.type, epic);
        }

        public static EpicBuilder<TState> CreateBuilder<TState>()
        {
            return new EpicBuilder<TState>();
        }

        public static EpicBuilder<TState, TDependencies> CreateBuilder<TState, TDependencies>()
        {
            return new EpicBuilder<TState, TDependencies>();
        }

        public static void AddTo<TState>(this Epic<TState> epic, ref EpicBuilder<TState> builder)
        {
            builder.Add(epic);
        }

        public static void AddTo<TState, TDependencies>(this Epic<TState, TDependencies> epic,
            ref EpicBuilder<TState, TDependencies> builder)
        {
            builder.Add(epic);
        }

        public static Epic<TState> Combine<TState>(params Epic<TState>[] epics)
        {
            return (action, state) =>
                epics.Where(static x => x != null)
                    .Select(epic => epic(action, state))
                    .Merge();
        }

        public static Epic<TState, TDependencies> Combine<TState, TDependencies>(
            params Epic<TState, TDependencies>[] epics)
        {
            return (action, state, dependencies) =>
                epics.Where(static x => x != null)
                    .Select(epic => epic(action, state, dependencies))
                    .Merge();
        }
    }
}
#endif