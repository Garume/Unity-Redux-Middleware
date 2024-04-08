using System.Threading;
using Unity.AppUI.Redux;
using UnityEngine;
using UnityReduxMiddleware;

namespace Sandbox
{
    public class Test : MonoBehaviour
    {
        public const string SliceName = "app";
        private const string Increment = SliceName + "/increment";

        private void Start()
        {
            var store = new MiddlewareStore();

            store.CreateSlice(
                SliceName,
                new AppState(0),
                builder => { builder.Add(Increment, Reducer.Increment); }
            );

            store.AddMiddleware(Middlewares.HeavyTestMiddleware());
            store.AddMiddleware(Middlewares.LoggerMiddleware());

            var cts = new CancellationTokenSource();
            store.Dispatch(Increment, cts.Token);
        }
    }

    public record AppState(int Value)
    {
        public int Value { get; set; } = Value;
    }

    public static class Reducer
    {
        public static AppState Increment(AppState state, Action action)
        {
            return state with { Value = state.Value + 1 };
        }
    }
}