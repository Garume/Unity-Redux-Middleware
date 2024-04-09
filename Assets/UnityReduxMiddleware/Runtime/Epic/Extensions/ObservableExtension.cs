#if UNITYREDUXMIDDLEWARE_R3_INTEGRATION
using System;
using R3;
using Action = Unity.AppUI.Redux.Action;

namespace UnityReduxMiddleware.Epic
{
    public static class ObservableExtension
    {
        public static Observable<Action> OfAction(this Observable<Action> source, string actionType)
        {
            if (string.IsNullOrEmpty(actionType)) throw new ArgumentNullException(nameof(actionType));
            return source.Where(actionType, (action, type) => action.type == type);
        }

        public static Observable<Action> OfAction(this Observable<Action> source, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return source.Where(action, (input, expected) => input == expected);
        }

        public static Observable<Action> OfAction<T>(this Observable<Action> source, Unity.AppUI.Redux.Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return source.Where(action, (input, expected) => input == expected);
        }

        public static Observable<Action> Dispatch(this Observable<Action> source, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return source.Select(action, (_, self) => self);
        }

        public static Observable<Action> Dispatch<T>(this Observable<Action> source, Unity.AppUI.Redux.Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return source.Select(action, (_, self) => self as Action);
        }
    }
}
#endif