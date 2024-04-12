using System.Threading.Tasks;
using NUnit.Framework;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Epic;
using UnityReduxMiddleware.Tests.Runtime.Utility;

namespace UnityReduxMiddleware.Tests.Runtime.Epics
{
    public class UseCaseTest
    {
        [Test]
        public async Task EpicMiddlewareUseCase_EpicFilteredActionAdded_Dispatch()
        {
            // Arrange
            var store = new MiddlewareStore();
            var appName = "App";
            var countIncrement = $"{appName}/CountIncrement";
            var countIncrement1Action = Store.CreateAction($"{appName}/CountIncrement1");
            var countIncrement2Action = Store.CreateAction($"{appName}/CountIncrement2");
            var countIncrementAction = Store.CreateAction<int>(countIncrement);

            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var rootEpic = CreateEpic(countIncrement1Action, countIncrementAction, countIncrement2Action);

            store.CreateSlice(appName, new MockState(),
                builder =>
                {
                    builder.Add<int>(countIncrement,
                        (state, action) => state with { Value = state.Value + action.payload });
                });

            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(rootEpic);
            
            // Act
            await store.DispatchAsync(countIncrement1Action.Invoke());
            await store.DispatchAsync(countIncrement2Action.Invoke());
            
            // Assert
            // Wait for the state to be updated
            // In fact, since it is left unthrown, the State should not be checked.
            // If you want to detect the end, add a process to Epic.
            await Task.Delay(100);
            Assert.That(store.GetState<MockState>(appName).Value, Is.EqualTo(3));
        }

        private static Epic<MockState> CreateEpic(ActionCreator countIncrement1Action,
            ActionCreator<int> countIncrementAction,
            ActionCreator countIncrement2Action)
        {
            var builder = Epic.Epic.CreateBuilder<MockState>();
            countIncrement1Action.CreateEpic<MockState>((action, _) =>
                action.Dispatch(countIncrementAction.Invoke(1))).AddTo(ref builder);
            countIncrement2Action.CreateEpic<MockState>((action, _) =>
                action.Dispatch(countIncrementAction.Invoke(2))).AddTo(ref builder);
            return builder.Build();
        }
    }
}