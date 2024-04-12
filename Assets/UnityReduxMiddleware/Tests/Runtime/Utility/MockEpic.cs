using System;
using UnityReduxMiddleware.Epic;

namespace UnityReduxMiddleware.Tests.Runtime.Utility
{
    public static class MockEpic
    {
        public static Epic<MockState> Create(Action onDispatched)
        {
            return Epic.Epic.Create<MockState>((action, _) =>
            {
                onDispatched();
                return action;
            });
        }

        public static Epic<MockState, MockDependency.Int> CreateWithIntDependency(
            Action<MockDependency.Int> onDispatched)
        {
            return Epic.Epic.Create<MockState, MockDependency.Int>((action, _, dependency) =>
            {
                onDispatched(dependency);
                return action;
            });
        }

        public static Epic<MockState> CreateReturnNull()
        {
            return Epic.Epic.Create<MockState>((_, _) => null);
        }
    }
}