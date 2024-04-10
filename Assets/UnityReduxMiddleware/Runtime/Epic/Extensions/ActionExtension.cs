using R3;
using Unity.AppUI.Redux;

#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
namespace UnityReduxMiddleware.Epic
{
    public static class ActionExtension
    {
        public static Epic<TState> CreateEpic<TState>(this string actionType,
            System.Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, Observable<Action>> epic)
        {
            return Epic.Create(actionType, epic);
        }

        public static Epic<TState> CreateEpic<TState>(this ActionCreator action,
            System.Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, Observable<Action>> epic)
        {
            return Epic.Create(action, epic);
        }

        public static Epic<TState, TDependencies> CreateEpic<TState, TDependencies>(this string actionType,
            System.Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, TDependencies, Observable<Action>> epic)
        {
            return Epic.Create(actionType, epic);
        }

        public static Epic<TState, TDependencies> CreateEpic<TState, TDependencies>(this ActionCreator action,
            System.Func<Observable<Action>, ReadOnlyReactiveProperty<TState>, TDependencies, Observable<Action>> epic)
        {
            return Epic.Create(action, epic);
        }
    }
}
#endif