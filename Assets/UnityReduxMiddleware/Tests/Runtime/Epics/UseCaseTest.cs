using System.Threading.Tasks;
using NUnit.Framework;
using Unity.AppUI.Redux;
using UnityReduxMiddleware.Epic;
using UnityReduxMiddleware.Tests.Runtime.Utility;

namespace UnityReduxMiddleware.Tests.Runtime.Epics
{
    public class UseCaseTest
    {
        private const string AppName = "App";
        private const string CountIncrement = AppName + "/CountIncrement";
        private readonly ActionCreator _countIncrement1Action = Store.CreateAction(AppName + "/CountIncrement1");
        private readonly ActionCreator _countIncrement2Action = Store.CreateAction(AppName + "/CountIncrement2");
        private readonly ActionCreator<int> _countIncrementAction = Store.CreateAction<int>(CountIncrement);

        [Test]
        public void EpicMiddlewareUseCase_EpicFilteredActionAdded_Dispatch()
        {
            // Arrange
            var store = new MiddlewareStore();
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var rootEpic = CreateEpic();

            store.CreateSlice(AppName, new MockState(),
                builder =>
                {
                    builder.Add<int>(CountIncrement,
                        (state, action) => state with { Value = state.Value + action.payload });
                });

            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(rootEpic);

            // Act
            store.Dispatch(_countIncrement1Action.Invoke());
            store.Dispatch(_countIncrement2Action.Invoke());

            // Assert
            Assert.That(store.GetState<MockState>(AppName).Value, Is.EqualTo(3));
        }

        [Test]
        public async Task EpicMiddlewareUseCase_EpicFilteredActionAdded_DispatchAsync()
        {
            // Arrange
            var store = new MiddlewareStore();
            var epicMiddleware = EpicMiddleware.Default<MockState>();
            var rootEpic = CreateEpic();

            store.CreateSlice(AppName, new MockState(),
                builder =>
                {
                    builder.Add<int>(CountIncrement,
                        (state, action) => state with { Value = state.Value + action.payload });
                });

            store.AddMiddleware(epicMiddleware.Create());
            epicMiddleware.Run(rootEpic);

            // Act
            await store.DispatchAsync(_countIncrement1Action.Invoke());
            await store.DispatchAsync(_countIncrement2Action.Invoke());

            // Assert
            Assert.That(store.GetState<MockState>(AppName).Value, Is.EqualTo(3));
        }

        private Epic<MockState> CreateEpic()
        {
            var builder = Epic.Epic.CreateBuilder<MockState>();
            _countIncrement1Action.CreateEpic<MockState>((action, _) =>
                action.Dispatch(_countIncrementAction.Invoke(1))).AddTo(ref builder);
            _countIncrement2Action.CreateEpic<MockState>((action, _) =>
                action.Dispatch(_countIncrementAction.Invoke(2))).AddTo(ref builder);
            return builder.Build();
        }
    }
}